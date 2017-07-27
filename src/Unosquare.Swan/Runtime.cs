namespace Unosquare.Swan
{
    using Components;
    using System;
    using System.IO;
    using System.Threading;
#if !NETSTANDARD1_3 && !UWP
    using System.Reflection;
#endif
#if !UWP
    using System.Diagnostics;
#endif

    /// <summary>
    /// Provides utility methods to retrieve information about the current application
    /// </summary>
#if NET452
    public class Runtime : MarshalByRefObject
#else
    public static class Runtime
#endif
    {
        #region Property Backing

#if NET452
        private static readonly Lazy<Assembly> m_EntryAssembly = new Lazy<Assembly>(() => Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly());
#endif

#if NETSTANDARD1_6
        private static readonly Lazy<Assembly> m_EntryAssembly = new Lazy<Assembly>(Assembly.GetEntryAssembly);
#endif

#if !NETSTANDARD1_3 && !UWP
        private static readonly Lazy<AssemblyName> m_EntryAssemblyName = new Lazy<AssemblyName>(() => m_EntryAssembly.Value.GetName());
#endif

#if !UWP
        private static readonly Lazy<Process> m_Process = new Lazy<Process>(Process.GetCurrentProcess);
#endif
        private static readonly Lazy<bool?> m_IsUsingMonoRuntime = new Lazy<bool?>(() => Type.GetType("Mono.Runtime") != null);


#if !NETSTANDARD1_3 && !UWP
        private static readonly Lazy<string> m_CompanyName = new Lazy<string>(() =>
        {
            var attribute = EntryAssembly.GetCustomAttribute(typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute;
            return attribute == null ? string.Empty : attribute.Company;
        });

        private static readonly Lazy<string> m_ProductName = new Lazy<string>(() =>
        {
            var attribute = EntryAssembly.GetCustomAttribute(typeof(AssemblyProductAttribute)) as AssemblyProductAttribute;
            return attribute == null ? string.Empty : attribute.Product;
        });

        private static readonly Lazy<string> m_ProductTrademark = new Lazy<string>(() =>
        {
            var attribute = EntryAssembly.GetCustomAttribute(typeof(AssemblyTrademarkAttribute)) as AssemblyTrademarkAttribute;
            return attribute == null ? string.Empty : attribute.Trademark;
        });
#endif

        private static readonly Lazy<ArgumentParser> _argumentParser = new Lazy<ArgumentParser>(() => new ArgumentParser());

        private static readonly Lazy<ObjectMapper> _objectMapper = new Lazy<ObjectMapper>(() => new ObjectMapper());

        #endregion

        #region State Variables

        private static OperatingSystem? m_OS = new OperatingSystem?();

#if !NETSTANDARD1_3 && !UWP
        private static readonly string ApplicationMutexName = "Global\\{{" + EntryAssembly.FullName + "}}";
#else
        private const string ApplicationMutexName = "Global\\{{SWANINSTANCE}}";
#endif

        private static readonly object SyncLock = new object();

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current Operating System.
        /// </summary>
        public static OperatingSystem OS
        {
            get
            {
                if (m_OS.HasValue == false)
                {
                    var windowsDirectory = Environment.GetEnvironmentVariable("windir");
                    if (string.IsNullOrEmpty(windowsDirectory) == false
                        && windowsDirectory.Contains(@"\")
                        && Directory.Exists(windowsDirectory))
                    {
                        m_OS = OperatingSystem.Windows;
                    }
                    else
                    {
                        m_OS = File.Exists(@"/proc/sys/kernel/ostype") ?
                            OperatingSystem.Unix :
                            OperatingSystem.Osx;
                    }
                }

                return m_OS ?? OperatingSystem.Unknown;
            }
        }

#if !UWP
        /// <summary>
        /// Gets the process associated with the current application.
        /// </summary>
        public static Process Process => m_Process.Value;
#endif

        /// <summary>
        /// Checks if this application (including version number) is the only instance currently running.
        /// </summary>
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
                            $"Application Mutex created {appMutex} named '{ApplicationMutexName}'".Debug(typeof(Runtime));
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
        /// Gets a value indicating whether this application instance is using the Mono runtime.
        /// </summary>
        public static bool IsUsingMonoRuntime => m_IsUsingMonoRuntime.Value ?? false;

#if !NETSTANDARD1_3 && !UWP
        /// <summary>
        /// Gets the assembly that started the application.
        /// </summary>
        public static Assembly EntryAssembly => m_EntryAssembly.Value;

        /// <summary>
        /// Gets the name of the entry assembly.
        /// </summary>
        public static AssemblyName EntryAssemblyName => m_EntryAssemblyName.Value;

        /// <summary>
        /// Gets the entry assembly version.
        /// </summary>
        public static Version EntryAssemblyVersion => EntryAssemblyName.Version;

        /// <summary>
        /// Gets the full path to the folder containing the assembly that started the application.
        /// </summary>
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
        public static string CompanyName => m_CompanyName.Value;

        /// <summary>
        /// Gets the name of the product.
        /// </summary>
        public static string ProductName => m_ProductName.Value;

        /// <summary>
        /// Gets the trademark.
        /// </summary>
        public static string ProductTrademark => m_ProductTrademark.Value;
#endif

        /// <summary>
        /// Gets a local storage path with a version
        /// </summary>
        public static string LocalStoragePath
        {
            get
            {
#if !NETSTANDARD1_3 && !UWP
                var localAppDataPath =
#if NET452
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), EntryAssemblyName.Name);
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
        /// Provides a simple IoC Container based on TinyIoC
        /// </summary>
        public static DependencyContainer Container => DependencyContainer.Current;

        /// <summary>
        /// Provides a Message Hub with the Publish/Subscribe pattern
        /// The implementation is based on TinyIoC Messenger
        /// </summary>
        public static MessageHub Messages => Container.Resolve<IMessageHub>() as MessageHub;

        /// <summary>
        /// Gets the singleton instance created with basic defaults.
        /// </summary>
        public static ArgumentParser ArgumentParser => _argumentParser.Value;

        /// <summary>
        /// Gets the object mapper instance created with basic defaults.
        /// </summary>
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
        public static Assembly[] GetAssemblies()
        {
            return Reflection.AppDomain.CurrentDomain.GetAssemblies();
        } 
#endif

#if NET452
        /// <summary>
        /// Build a full path pointing to the current user's desktop with the given filename
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public static string GetDesktopFilePath(string filename)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            var pathWithFilename = Path.Combine(path, filename);
            return Path.GetFullPath(pathWithFilename);
        }
#endif

        #endregion
    }
}