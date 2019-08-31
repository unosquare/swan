namespace Swan.Logging
{
    /// <summary>
    /// Represents a logger target. This target will write to the
    /// Debug console using System.Diagnostics.Debug.
    /// </summary>
    /// <seealso cref="ILogger" />
    public class DebugLogger : ILogger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DebugLogger"/> class.
        /// </summary>
        protected DebugLogger()
        {
            // Empty
        }

        /// <inheritdoc/>
        public LogLevel LogLevel { get; set; } = Terminal.IsDebuggerAttached ? LogLevel.Trace : LogLevel.None;
        
        internal static DebugLogger Instance { get; } = new DebugLogger();

        /// <inheritdoc/>
        public void Log(LogMessageReceivedEventArgs logEvent)
        {
            ConsoleLogger.GetOutputAndColor(logEvent, true, out var outputMessage);

            System.Diagnostics.Debug.Write(outputMessage);
        }
        
        /// <inheritdoc/>
        public void Dispose()
        {
            // do nothing
        }
    }
}
