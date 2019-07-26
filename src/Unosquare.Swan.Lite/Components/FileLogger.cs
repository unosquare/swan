namespace Unosquare.Swan.Components
{
    using Abstractions;
    using System;
    using System.Collections.Concurrent;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A helper class to write into files the messages sent by the <see cref="Terminal"/>.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public class FileLogger : IDisposable
    {
        private static readonly object SyncLock = new object();
        private static FileLogger _instance;

        private readonly ManualResetEventSlim _doneEvent = new ManualResetEventSlim(true);
        private readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
        private readonly ExclusiveTimer _timer;
        
        private bool _disposedValue; // To detect redundant calls

        private FileLogger(string path, bool dailyFile)
        {
            LogPath = path;
            DailyFile = dailyFile;

            _timer = new ExclusiveTimer(
                async () => await WriteLogEntries().ConfigureAwait(false),
                TimeSpan.Zero,
                TimeSpan.FromSeconds(5));
        }

        private string LogPath { get; set; }
        private bool DailyFile { get; set; }

        /// <summary>
        /// Registers the log file generation.
        /// </summary>
        /// <param name="destinationPath">The destination path.</param>
        /// <param name="dailyFile">if set to <c>true</c> a daily file is created, otherwise, only one general file is created.</param>
        public static void Register(string destinationPath = null, bool dailyFile = true)
        {
            var localPath = destinationPath ?? SwanRuntime.EntryAssemblyDirectory;

            lock (SyncLock)
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
        /// Unregisters the log file generation.
        /// </summary>
        public static void Unregister()
        {
            lock (SyncLock)
            {
                if (_instance == null) return;

                Terminal.OnLogMessageReceived -= _instance.EnqueueEntries;
                _instance.Dispose();
                _instance = null;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue) return;

            if (disposing)
            {
                _timer.Pause();
                _timer.Dispose();

                _doneEvent.Wait();
                _doneEvent.Reset();
                WriteLogEntries(true).Await();
                _doneEvent.Dispose();
            }

            LogPath = null;
            _disposedValue = true;
        }

        private void EnqueueEntries(object sender, LogMessageReceivedEventArgs logEvent)
        {
            var outputMessage = Terminal.CreateOutputMessage(
                logEvent.Source,
                logEvent.Message,
                Terminal.GetConsoleColorAndPrefix(logEvent.MessageType, out var _),
                logEvent.UtcDate);

            _logQueue.Enqueue($"{outputMessage}{Environment.NewLine}{(logEvent.Exception != null ? $"{logEvent.Exception.Stringify().Indent()}{Environment.NewLine}" : String.Empty )}");
        }

        private async Task WriteLogEntries(bool finalCall = false)
        {
            if (_logQueue.IsEmpty)
                return;

            if (!finalCall && !_doneEvent.IsSet)
                return;

            _doneEvent.Reset();

            try
            {
                using (var file = File.AppendText(GetFileName()))
                {
                    while (!_logQueue.IsEmpty)
                    {
                        if (_logQueue.TryDequeue(out var entry))
                            await file.WriteAsync(entry).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                if (!finalCall)
                    _doneEvent.Set();
            }
        }

        private string GetFileName() => 
            Path.Combine(LogPath, $"Application{(DailyFile ? $"_{DateTime.UtcNow:yyyyMMdd}" : string.Empty)}.log");
    }
}
