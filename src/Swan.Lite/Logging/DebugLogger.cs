namespace Swan.Logging
{
    /// <summary>
    /// Represents a logger target. This target will write to the
    /// Debug console using System.Diagnostics.Debug.
    /// </summary>
    /// <seealso cref="ILogger" />
    public class DebugLogger : TextLogger, ILogger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DebugLogger"/> class.
        /// </summary>
        protected DebugLogger()
        {
            // Empty
        }
        
        /// <summary>
        /// Gets the current instance of DebugLogger.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static DebugLogger Instance { get; } = new DebugLogger();

        /// <summary>
        /// Gets a value indicating whether a debugger is attached.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is debugger attached; otherwise, <c>false</c>.
        /// </value>
        public static bool IsDebuggerAttached => System.Diagnostics.Debugger.IsAttached;

        /// <inheritdoc/>
        public LogLevel LogLevel { get; set; } = IsDebuggerAttached ? LogLevel.Trace : LogLevel.None;

        /// <inheritdoc/>
        public void Log(LogMessageReceivedEventArgs logEvent)
        {
            var (outputMessage, _) = GetOutputAndColor(logEvent);

            System.Diagnostics.Debug.Write(outputMessage);
        }
        
        /// <inheritdoc/>
        public void Dispose()
        {
            // do nothing
        }
    }
}
