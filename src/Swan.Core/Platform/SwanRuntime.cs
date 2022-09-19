namespace Swan.Platform;

using System.IO;
using System.Threading;

/// <summary>
/// Provides utility methods to retrieve information about the current application.
/// </summary>
public static class SwanRuntime
{
    private static readonly Lazy<Assembly> EntryAssemblyLazy = new(() => Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly());

    private static readonly string ApplicationMutexName = $"Global\\{{{{{EntryAssembly.FullName}}}}}";

    private static readonly Lazy<Encoding> Windows1252EncodingLazy = new(
        Encoding.GetEncodings().FirstOrDefault(c => c.CodePage == 1252)?.GetEncoding() ?? Encoding.GetEncoding(default(int)));

    private static readonly object SyncLock = new();

    /// <summary>
    /// Gets the Windows 1253 Encoding (if available). Otherwise returns the default ANSI encoding.
    /// </summary>
    public static Encoding Windows1252Encoding => Windows1252EncodingLazy.Value;

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
                    using var _ = Mutex.OpenExisting(ApplicationMutexName);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    try
                    {
                        // If exception occurred, there is no such mutex.
                        using var appMutex = new Mutex(true, ApplicationMutexName);

                        // Only one instance.
                        return true;
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch
#pragma warning restore CA1031 // Do not catch general exception types
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
    /// Gets the assembly that started the application.
    /// </summary>
    /// <value>
    /// The entry assembly.
    /// </value>
    public static Assembly EntryAssembly => EntryAssemblyLazy.Value;

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
            var uri = new UriBuilder(EntryAssembly.Location);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path) ?? string.Empty;
        }
    }
}
