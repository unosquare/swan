namespace Unosquare.Swan
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;


    /// <summary>
    /// Provides utility methods to retrieve information about the current application
    /// </summary>
    static public class CurrentApp
    {
        static private readonly object SyncLock = new object();

        static private Assembly m_EntryAssembly = null;
        static private Process m_Process = null;

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
