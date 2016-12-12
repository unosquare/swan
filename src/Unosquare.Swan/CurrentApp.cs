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
        private static Os ApplicationOs = Os.Unknown;

        #endregion

        /// <summary>
        /// Determines if the current application is using the Mono runtime
        /// </summary>
        public static readonly bool IsUsingMonoRuntime = Type.GetType("Mono.Runtime") != null;

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
        /// Gets the OS.
        /// </summary>
        public static Os OS
        {
            get
            {
                if (ApplicationOs == Os.Unknown)
                {
                    var windir = Environment.GetEnvironmentVariable("windir");
                    if (!string.IsNullOrEmpty(windir) && windir.Contains(@"\") && Directory.Exists(windir))
                    {
                        ApplicationOs = Os.Windows;
                    }
                    else
                    {
                        ApplicationOs = File.Exists(@"/proc/sys/kernel/ostype") ? Os.Unix : Os.Osx;
                    }
                }

                return ApplicationOs;
            }
        }

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

        /// <summary>
        /// Gets the local storage path.
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
        /// Gets the assembly that started the application.
        /// </summary>
        public static Assembly EntryAssembly => m_EntryAssembly ?? (m_EntryAssembly = Assembly.GetEntryAssembly());

        /// <summary>
        /// Gets the full path of the assembly that started the application.
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
    }
}