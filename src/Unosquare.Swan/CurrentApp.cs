namespace Unosquare.Swan
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    
    /// <summary>
    /// Provides utility methods to retrieve information about the current application
    /// </summary>
    static public class CurrentApp
    {
        private static Mutex _applicationMutex;
        private static readonly string ApplicationMutexName = "Global\\{{" + EntryAssembly.FullName + "}}";

        static private readonly object SyncLock = new object();

        static private Assembly m_EntryAssembly = null;
        static private Process m_Process = null;
        static private Os ApplicationOs = Os.Unknown;

        /// <summary>
        /// Determines if the app is running at mono
        /// </summary>
        public static readonly bool IsRunningAtMono = Type.GetType("Mono.Runtime") != null;

        /// <summary>
        /// Checks if this process is unique using a MUTEX.
        /// </summary>
        public static bool IsSingleInstance
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
                            _applicationMutex = new Mutex(true, ApplicationMutexName);

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
        /// Gets the OS.
        /// </summary>
        public static Os OS
        {
            get
            {
                if (ApplicationOs == Os.Unknown)
                {
#if NET452
                    var p = (int)Environment.OSVersion.Platform;

                    if ((p == 4) || (p == 6) || (p == 128))
                    {
                        var uname = ProcessHelper.GetProcessOutputAsync("uname").Result;

                        ApplicationOs = string.IsNullOrWhiteSpace(uname) == false && uname.ToLower().Contains("darwin")
                            ? Os.Osx
                            : Os.Unix;
                    }
                    else
                    {
                        ApplicationOs = Os.Windows;
                    }
#else
                    // TODO: pending NETCORE
                    ApplicationOs = Os.Windows;
#endif
                }

                return ApplicationOs;
            }
        }

        /// <summary>
        /// Gets the local storage path.
        /// </summary>
        /// <value>
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

                var returnPath = Path.Combine(localAppDataPath, EntryAssembly.GetName().Version.ToString());

                if (Directory.Exists(returnPath) == false)
                {
                    Directory.CreateDirectory(returnPath);
                }

                return returnPath;
            }
        }

        /// <summary>
        /// Gets the process associated with the current application.
        /// </summary>
        static public Process Process
        {
            get
            {
                lock (SyncLock)
                {
                    if (m_Process == null)
                        m_Process = Process.GetCurrentProcess();
                    return m_Process;
                }
            }
        }

        /// <summary>
        /// Gets the assembly that started the application.
        /// </summary>
        static public Assembly EntryAssembly
        {
            get
            {
                if (m_EntryAssembly == null)
                    m_EntryAssembly = Assembly.GetEntryAssembly();

                return m_EntryAssembly;
            }
        }

        /// <summary>
        /// Gets the full path of the assembly that started the application.
        /// </summary>
        static public string EntryAssemblyDirectory
        {
            get
            {
                var codeBase = EntryAssembly.CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
    }
}
