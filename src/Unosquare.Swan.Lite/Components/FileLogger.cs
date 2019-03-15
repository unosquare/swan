namespace Unosquare.Swan.Components
{
    using Swan.Abstractions;
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class FileLogger : IDisposable
    {
        private static readonly object _syncLock = new Object();
        private static FileLogger _instance;
        
        private readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
        private readonly ExclusiveTimer _timer;

        private FileLogger(string path, bool dailyFile)
        {
            LogPath = path;
            DailyFile = dailyFile;

            _timer = new ExclusiveTimer(
                async () => await WriteLogEntries(),
                TimeSpan.Zero,
                TimeSpan.FromSeconds(5));
        }

        private string LogPath { get; set; }
        private bool DailyFile { get; set; }

        /// <summary>
        /// Registers the specified destination path.
        /// </summary>
        /// <param name="destinationPath">The destination path.</param>
        /// <param name="dailyFile">if set to <c>true</c> [daily file].</param>
        public static void Register(string destinationPath = null, bool dailyFile = true)
        {
            var localPath = destinationPath ??
#if NETSTANDARD1_3
            Runtime.LocalStoragePath;
#else
            Runtime.EntryAssemblyDirectory;
#endif
            lock (_syncLock)
            {
                if (_instance == null)
                {
                    _instance = new FileLogger(localPath, dailyFile);
                    Terminal.OnLogMessageReceived += _instance.EnqueueEntries;
                }
                else
                {
                    // Change properties
                    _instance.LogPath = localPath;
                    _instance.DailyFile = dailyFile;
                }
            }
        }

        /// <summary>
        /// Unregisters this instance.
        /// </summary>
        public static void Unregister()
        {
            lock (_syncLock)
            {
                if (_instance != null)
                {
                    Terminal.OnLogMessageReceived -= _instance.EnqueueEntries;
                    _instance.Dispose();
                    _instance = null;
                }
            }
        }

        private void EnqueueEntries(object sender, LogMessageReceivedEventArgs logEvent) =>
            _logQueue.Enqueue(Terminal.CreateOutputMessage(
                logEvent.Source, 
                logEvent.Message, 
                Terminal.GetConsoleColorAndPrefix(logEvent.MessageType, out var unusedColor), 
                logEvent.UtcDate));

        private async Task WriteLogEntries()
        {
            if (!_logQueue.IsEmpty)
            {
                using (var file = File.AppendText(GetFileName()))
                {
                    while (!_logQueue.IsEmpty)
                    {
                        if (_logQueue.TryDequeue(out var entry))
                        {
                            await file.WriteAsync(entry);
                        }
                    }
                }
            }
        }

        private string GetFileName() => 
            Path.Combine(LogPath, $"Application{(DailyFile ? $"_{DateTime.UtcNow:yyyyMMdd}" : String.Empty)}.log");

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _timer.Pause();
                    _timer.Dispose();
                }

                LogPath = null;
                disposedValue = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() =>
            Dispose(true);

        #endregion
    }
}
