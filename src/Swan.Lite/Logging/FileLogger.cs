using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Swan.Threading;

namespace Swan.Logging
{
    /// <summary>
    /// A helper class to write into files the messages sent by the <see cref="Terminal" />.
    /// </summary>
    /// <seealso cref="ILogger" />
    public class FileLogger : TextLogger, ILogger
    {
        private readonly ManualResetEventSlim _doneEvent = new ManualResetEventSlim(true);
        private readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
        private readonly ExclusiveTimer _timer;
        private readonly string _filePath;

        private bool _disposedValue; // To detect redundant calls

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLogger"/> class.
        /// </summary>
        public FileLogger()
            : this(SwanRuntime.EntryAssemblyDirectory, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileLogger"/> class.
        /// </summary>
        /// <param name="filePath">The filePath.</param>
        /// <param name="dailyFile">if set to <c>true</c> [daily file].</param>
        public FileLogger(string filePath, bool dailyFile)
        {
            _filePath = filePath;
            DailyFile = dailyFile;

            _timer = new ExclusiveTimer(
                async () => await WriteLogEntries().ConfigureAwait(false),
                TimeSpan.Zero,
                TimeSpan.FromSeconds(5));
        }

        /// <inheritdoc />
        public LogLevel LogLevel { get; set; }

        /// <summary>
        /// Gets the file path.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        public string FilePath => DailyFile
                                    ? Path.Combine(Path.GetDirectoryName(_filePath), Path.GetFileNameWithoutExtension(_filePath) + $"_{DateTime.UtcNow:yyyyMMdd}" + Path.GetExtension(_filePath))
                                    : _filePath;

        /// <summary>
        /// Gets a value indicating whether [daily file].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [daily file]; otherwise, <c>false</c>.
        /// </value>
        public bool DailyFile { get; }

        /// <inheritdoc />
        public void Log(LogMessageReceivedEventArgs logEvent)
        {
            var (outputMessage, _) = GetOutputAndColor(logEvent);

            _logQueue.Enqueue(outputMessage);
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

            _disposedValue = true;
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
                using var file = File.AppendText(FilePath);
                while (!_logQueue.IsEmpty)
                {
                    if (_logQueue.TryDequeue(out var entry))
                        await file.WriteAsync(entry).ConfigureAwait(false);
                }
            }
            finally
            {
                if (!finalCall)
                    _doneEvent.Set();
            }
        }
    }
}
