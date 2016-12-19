namespace Unosquare.Swan
{
    using Runtime;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;

    /// <summary>
    /// Provides utility methods to retrieve information about the current application
    /// </summary>
    public static class CurrentApp
    {
        #region State Variables

        private static Mutex ApplicationMutex = null;
        private static readonly string ApplicationMutexName = "Global\\{{" + EntryAssembly.FullName + "}}";
        private static readonly object SyncLock = new object();

        #endregion

        #region Property Backing

        private static Assembly m_EntryAssembly = null;
        private static Process m_Process = null;
        private static OperatingSystem m_OS = OperatingSystem.Unknown;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current Operating System.
        /// </summary>
        public static OperatingSystem OS
        {
            get
            {
                if (m_OS == OperatingSystem.Unknown)
                {
                    var windir = Environment.GetEnvironmentVariable("windir");
                    if (!string.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir))
                    {
                        m_OS = OperatingSystem.Windows;
                    }
                    else
                    {
                        m_OS = File.Exists(@"/proc/sys/kernel/ostype") ? OperatingSystem.Unix : OperatingSystem.Osx;
                    }
                }

                return m_OS;
            }
        }

        /// <summary>
        /// Gets the process associated with the current application.
        /// </summary>
        public static Process Process
        {
            get
            {
                lock (SyncLock)
                {
                    return m_Process ?? (m_Process = Process.GetCurrentProcess());
                }
            }
        }

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
                            ApplicationMutex = new Mutex(true, ApplicationMutexName);

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
        public static bool IsUsingMonoRuntime { get; } = Type.GetType("Mono.Runtime") != null;

        /// <summary>
        /// Gets the application domain.
        /// </summary>
        /// <value>
        /// The application domain.
        /// </value>
        public static Unosquare.Swan.Runtime.AppDomain AppDomain => Unosquare.Swan.Runtime.AppDomain.Instance;

        /// <summary>
        /// Gets the assembly that started the application.
        /// </summary>
        public static Assembly EntryAssembly => m_EntryAssembly ?? (m_EntryAssembly = Assembly.GetEntryAssembly());

        /// <summary>
        /// Gets the entry assembly version.
        /// </summary>
        public static Version EntryAssemblyVersion => EntryAssembly.GetName().Version;

        /// <summary>
        /// Gets the full path to the folder containing the assembly that started the application.
        /// </summary>
        public static string EntryAssemblyDirectory
        {
            get
            {
                var codeBase = EntryAssembly.CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        /// <summary>
        /// Gets the name of the company.
        /// </summary>
        public static string CompanyName => (EntryAssembly.GetCustomAttribute(typeof(AssemblyCompanyAttribute)) as AssemblyCompanyAttribute)?.Company;

        /// <summary>
        /// Gets the name of the product.
        /// </summary>
        public static string ProductName => (EntryAssembly.GetCustomAttribute(typeof(AssemblyProductAttribute)) as AssemblyProductAttribute)?.Product;

        /// <summary>
        /// Gets the trademark.
        /// </summary>
        public static string ProductTrademark => (EntryAssembly.GetCustomAttribute(typeof(AssemblyTrademarkAttribute)) as AssemblyTrademarkAttribute)?.Trademark;

        /// <summary>
        /// Gets a path with a version
        /// </summary>
        public static string LocalStoragePath
        {
            get
            {
                var localAppDataPath =
#if NET452
                    Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), EntryAssembly.GetName().Name);
#else
                    Path.GetDirectoryName(EntryAssembly.Location);
#endif

                var returnPath = Path.Combine(localAppDataPath, EntryAssemblyVersion.ToString());

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
        public static DependencyContainer Container => Swan.Runtime.DependencyContainer.Current;

        /// <summary>
        /// Provides a Message Hub with the Publish/Subscribe pattern
        /// The implementation is based on TinyIoC Messenger
        /// </summary>
        public static MessageHub Messages => Container.Resolve<IMessageHub>() as MessageHub;

        #endregion

        #region Methods

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