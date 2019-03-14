namespace Unosquare.Swan.Components
{
    using Swan.Abstractions;
    using System;
    using System.Collections.Concurrent;
    using System.Text;
    using System.Threading.Tasks;

    public class FileLogger : IDisposable
    {
        private static readonly object _lock = new Object();
        private static FileLogger _instance;
        
        private readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
        private readonly ExclusiveTimer _timer;

        private FileLogger(string path, bool dailyFile)
        {
            Path = path;
            DailyFile = dailyFile;

            _timer = new ExclusiveTimer(
                async () => await LogEntries(),
                TimeSpan.Zero,
                TimeSpan.FromSeconds(5));
        }

        private string Path { get; set; }
        private bool DailyFile { get; set; }

        public static void Register(string path = null, bool dailyFile = true)
        {
            var localPath = path ??
#if NETSTANDARD1_3
            Runtime.LocalStoragePath;
#else
            Runtime.EntryAssemblyDirectory;
#endif
            lock (_lock)
            {
                if (_instance == null)
                {
                    _instance = new FileLogger(localPath, dailyFile);
                    Terminal.OnLogMessageReceived += _instance.Write;
                }
                else
                {
                    // Change properties
                    _instance.Path = localPath;
                    _instance.DailyFile = dailyFile;
                }
            }
        }

        public static void Unregister()
        {
            Terminal.OnLogMessageReceived -= _instance.Write;
            // TODO: Dispose
        }

        private void Write(object sender, LogMessageReceivedEventArgs logEvent)
        {
            var msg = new StringBuilder();
            msg.AppendLine($"{logEvent.UtcDate:yyyy-MM-dd HH:mm:ss} ({logEvent.MessageType})");
            msg.AppendLine($"\t {nameof(logEvent.Source).ToUpper()}: {logEvent.Source}");
            msg.AppendLine($"\t {nameof(logEvent.Message).ToUpper()}: {logEvent.Message}");
            var exceptionType = logEvent.Exception?.GetType().Name;
            if (exceptionType != null)
                msg.AppendLine($"\t {exceptionType}: {logEvent.Exception}");
            if (logEvent.ExtendedData != null)
                msg.AppendLine($"\t {nameof(logEvent.ExtendedData).ToUpper()}: {logEvent.ExtendedData}");
            msg.AppendLine();

            _logQueue.Enqueue(msg.ToString());
        }

        private async Task LogEntries()
        {
            if (!_logQueue.IsEmpty)
            {
                while (!_logQueue.IsEmpty)
                {
                    if (_logQueue.TryDequeue(out var entry))
                    {
                        // TODO: Save to file
                    }
                }
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
