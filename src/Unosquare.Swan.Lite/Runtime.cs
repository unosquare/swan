namespace Unosquare.Swan
{
    using Components;
    using System;
    using System.IO;
    using System.Threading;
    using Reflection;
#if !NETSTANDARD1_3 && !UWP
    using System.Reflection;
#endif

    /// <summary>
    /// Provides utility methods to retrieve information about the current application
    /// </summary>
#if !NETSTANDARD1_3 && !UWP
    public class Runtime : MarshalByRefObject
#else
    public static class Runtime
#endif
    {
#if NET452
        private static readonly Lazy<Assembly> EntryAssemblyLazy = new Lazy<Assembly>(() => Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly());
#endif

#if NETSTANDARD2_0
        private static readonly Lazy<Assembly> EntryAssemblyLazy = new Lazy<Assembly>(Assembly.GetEntryAssembly);
#endif

#if NET452
        private static readonly Lazy<System.Diagnostics.Process> ProcessLazy = new Lazy<System.Diagnostics.Process>(System.Diagnostics.Process.GetCurrentProcess);
#endif

#if !NETSTANDARD1_3 && !UWP
        private static readonly Lazy<string> CompanyNameLazy = new Lazy<string>(() =>
        {
            var attribute =
                EntryAssembly.GetCustomAttribute(typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;
            return attribute?.Company ?? string.Empty;
        });

        private static readonly Lazy<string> ProductNameLazy = new Lazy<string>(() =>
        {
            var attribute =
                EntryAssembly.GetCustomAttribute(typeof(AssemblyProductAttribute)) as AssemblyProductAttribute;
            return attribute?.Product ?? string.Empty;
        });

        private static readonly Lazy<string> ProductTrademarkLazy = new Lazy<string>(() =>
        {
            var attribute =
                EntryAssembly.GetCustomAttribute(typeof(AssemblyTrademarkAttribute)) as AssemblyTrademarkAttribute;
            return attribute?.Trademark ?? string.Empty;
        });
#endif

        private static readonly Lazy<ArgumentParser> _argumentParser =
            new Lazy<ArgumentParser>(() => new ArgumentParser());

        private static readonly Lazy<ObjectMapper> _objectMapper = new Lazy<ObjectMapper>(() => new ObjectMapper());
        
        private static OperatingSystem? _oS = default(OperatingSystem?);

#if !NETSTANDARD1_3 && !UWP
        private static readonly string ApplicationMutexName = "Global\\{{" + EntryAssembly.FullName + "}}";
#else
        private const string ApplicationMutexName = "Global\\{{SWANINSTANCE}}";
#endif

        private static readonly object SyncLock = new object();
        
        #region Properties

        /// <summary>
        /// Gets the current Operating System.
        /// </summary>
        /// <value>
        /// The os.
        /// </value>
        public static OperatingSystem OS
        {
            get
            {
                if (_oS.HasValue == false)
                {
                    var windowsDirectory = Environment.GetEnvironmentVariable("windir");
                    if (string.IsNullOrEmpty(windowsDirectory) == false
                        && windowsDirectory.Contains(@"\")
                        && Directory.Exists(windowsDirectory))
                    {
                        _oS = OperatingSystem.Windows;
                    }
                    else
                    {
                        _oS = File.Exists(@"/proc/sys/kernel/ostype") ? OperatingSystem.Unix : OperatingSystem.Osx;
                    }
                }

                return _oS ?? OperatingSystem.Unknown;
            }
        }

#if NET452
        /// <summary>
        /// Gets the process associated with the current application.
        /// </summary>
        /// <value>
        /// The process.
        /// </value>
        public static System.Diagnostics.Process Process => ProcessLazy.Value;
#endif

        /// <summary>
        /// Checks if this application (including version number) is the only instance currently running.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is the only instance; otherwise, <c>false</c>.
        /// </value>
        public static bool IsTheOnlyInstance
        {
            get
            {
                lock (SyncLock)
                {
                    try
                    {
                        // Try to open existing mutex.
                        Mutex.OpenExisting(ApplicationMutexName);
                    }
                    catch
                    {
                        try
                        {
                            // If exception occurred, there is no such mutex.
                            var appMutex = new Mutex(true, ApplicationMutexName);
                            $"Application Mutex created {appMutex} named '{ApplicationMutexName}'".Debug(
                                typeof(Runtime));

                            // Only one instance.
                            return true;
                        }
                        catch
                        {
                            // Sometimes the user can't create the Global Mutex
                        }
                    }

                    // More than one instance.
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this application instance is using the MONO runtime.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is using MONO runtime; otherwise, <c>false</c>.
        /// </value>
        public static bool IsUsingMonoRuntime => Type.GetType("Mono.Runtime") != null;

        /// <summary>
        /// The property type cache
        /// </summary>
        /// <value>
        /// The property type cache.
        /// </value>
        public static Lazy<PropertyTypeCache> PropertyTypeCache { get; } =
            new Lazy<PropertyTypeCache>(() => new PropertyTypeCache());

        /// <summary>
        /// Gets the field type cache.
        /// </summary>
        /// <value>
        /// The field type cache.
        /// </value>
        public static Lazy<FieldTypeCache> FieldTypeCache { get; } =
            new Lazy<FieldTypeCache>(() => new FieldTypeCache());

#if !NETSTANDARD1_3 && !UWP
        /// <summary>
        /// Gets the assembly that started the application.
        /// </summary>
        /// <value>
        /// The entry assembly.
        /// </value>
        public static Assembly EntryAssembly => EntryAssemblyLazy.Value;

        /// <summary>
        /// Gets the name of the entry assembly.
        /// </summary>
        /// <value>
        /// The name of the entry assembly.
        /// </value>
        public static AssemblyName EntryAssemblyName => EntryAssemblyLazy.Value.GetName();

        /// <summary>
        /// Gets the entry assembly version.
        /// </summary>
        public static Version EntryAssemblyVersion => EntryAssemblyName.Version;

        /// <summary>
        /// Gets the full path to the folder containing the assembly that started the application.
        /// </summary>
        /// <value>
        /// The entry assembly directory.
        /// </value>
        public static string EntryAssemblyDirectory
        {
            get
            {
                var uri = new UriBuilder(EntryAssembly.CodeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// Gets the name of the company.
        /// </summary>
        /// <value>
        /// The name of the company.
        /// </value>
        public static string CompanyName => CompanyNameLazy.Value;

        /// <summary>
        /// Gets the name of the product.
        /// </summary>
        /// <value>
        /// The name of the product.
        /// </value>
        public static string ProductName => ProductNameLazy.Value;

        /// <summary>
        /// Gets the trademark.
        /// </summary>
        /// <value>
        /// The product trademark.
        /// </value>
        public static string ProductTrademark => ProductTrademarkLazy.Value;
#endif

        /// <summary>
        /// Gets a local storage path with a version
        /// </summary>
        /// <value>
        /// The local storage path.
        /// </value>
        public static string LocalStoragePath
        {
            get
            {
#if !NETSTANDARD1_3 && !UWP
                var localAppDataPath =
#if NET452
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        EntryAssemblyName.Name);
#else
                    Path.GetDirectoryName(EntryAssembly.Location);
#endif

                var returnPath = Path.Combine(localAppDataPath, EntryAssemblyVersion.ToString());
#else
                var returnPath = Directory.GetCurrentDirectory(); // Use current path...
#endif

                if (Directory.Exists(returnPath) == false)
                {
                    Directory.CreateDirectory(returnPath);
                }

                return returnPath;
            }
        }
        
        /// <summary>
        /// Gets the singleton instance created with basic defaults.
        /// </summary>
        /// <value>
        /// The argument parser.
        /// </value>
        public static ArgumentParser ArgumentParser => _argumentParser.Value;

        /// <summary>
        /// Gets the object mapper instance created with basic defaults.
        /// </summary>
        /// <value>
        /// The object mapper.
        /// </value>
        public static ObjectMapper ObjectMapper => _objectMapper.Value;

        #endregion

        #region Methods

#if !NETSTANDARD1_3 && !UWP
        /// <summary>
        /// Writes a standard banner to the standard output
        /// containing the company name, product name, assembly version and trademark.
        /// </summary>
        /// <param name="color">The color.</param>
        public static void WriteWelcomeBanner(ConsoleColor color = ConsoleColor.Gray)
        {
            $"{CompanyName} {ProductName} [Version {EntryAssemblyVersion}]".WriteLine(color);
            $"{ProductTrademark}".WriteLine(color);
        }

        /// <summary>
        /// Gets all the loaded assemblies in the current application domain.
        /// </summary>
        /// <returns>An array of assemblies</returns>
        public static Assembly[] GetAssemblies() => AppDomain.CurrentDomain.GetAssemblies();

        /// <summary>
        /// Build a full path pointing to the current user's desktop with the given filename
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns>
        /// The fully qualified location of path, such as "C:\MyFile.txt"
        /// </returns>
        /// <exception cref="ArgumentNullException">filename</exception>
        public static string GetDesktopFilePath(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename))
                throw new ArgumentNullException(nameof(filename));

            var pathWithFilename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                filename);
            return Path.GetFullPath(pathWithFilename);
        }
#endif

        #endregion
    }
}