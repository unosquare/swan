namespace Swan.Logging
{
    /// <summary>
    /// Interface for a logger implementation.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Gets the log level.
        /// </summary>
        /// <value>
        /// The log level.
        /// </value>
        LogMessageType LogLevel { get; }
        
        /// <summary>
        /// Logs the specified log event.
        /// </summary>
        /// <param name="logEvent">The <see cref="LogMessageReceivedEventArgs"/> instance containing the event data.</param>
        void Log(LogMessageReceivedEventArgs logEvent);
    }
}
