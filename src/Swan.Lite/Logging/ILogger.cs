namespace Swan.Logging
{
    using System;

    /// <summary>
    /// Interface for a logger implementation.
    /// </summary>
    public interface ILogger : IDisposable
    {
        /// <summary>
        /// Gets the log level.
        /// </summary>
        /// <value>
        /// The log level.
        /// </value>
        LogLevel LogLevel { get; }
        
        /// <summary>
        /// Logs the specified log event.
        /// </summary>
        /// <param name="logEvent">The <see cref="LogMessageReceivedEventArgs"/> instance containing the event data.</param>
        void Log(LogMessageReceivedEventArgs logEvent);
    }
}
