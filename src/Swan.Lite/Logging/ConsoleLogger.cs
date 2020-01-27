using System;

namespace Swan.Logging
{
    /// <summary>
    /// Represents a Console implementation of <c>ILogger</c>.
    /// </summary>
    /// <seealso cref="ILogger" />
    public class ConsoleLogger : TextLogger, ILogger
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogger"/> class.
        /// </summary>
        protected ConsoleLogger()
        {
            // Empty
        }

        /// <summary>
        /// Gets the current instance of ConsoleLogger.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static ConsoleLogger Instance { get; } = new ConsoleLogger();

        /// <summary>
        /// Gets or sets the debug logging prefix.
        /// </summary>
        /// <value>
        /// The debug prefix.
        /// </value>
        public static string DebugPrefix { get; set; } = "DBG";

        /// <summary>
        /// Gets or sets the trace logging prefix.
        /// </summary>
        /// <value>
        /// The trace prefix.
        /// </value>
        public static string TracePrefix { get; set; } = "TRC";

        /// <summary>
        /// Gets or sets the warning logging prefix.
        /// </summary>
        /// <value>
        /// The warn prefix.
        /// </value>
        public static string WarnPrefix { get; set; } = "WRN";

        /// <summary>
        /// Gets or sets the fatal logging prefix.
        /// </summary>
        /// <value>
        /// The fatal prefix.
        /// </value>
        public static string FatalPrefix { get; set; } = "FAT";

        /// <summary>
        /// Gets or sets the error logging prefix.
        /// </summary>
        /// <value>
        /// The error prefix.
        /// </value>
        public static string ErrorPrefix { get; set; } = "ERR";

        /// <summary>
        /// Gets or sets the information logging prefix.
        /// </summary>
        /// <value>
        /// The information prefix.
        /// </value>
        public static string InfoPrefix { get; set; } = "INF";

        /// <summary>
        /// Gets or sets the color of the information output logging.
        /// </summary>
        /// <value>
        /// The color of the information.
        /// </value>
        public static ConsoleColor InfoColor { get; set; } = ConsoleColor.Cyan;

        /// <summary>
        /// Gets or sets the color of the debug output logging.
        /// </summary>
        /// <value>
        /// The color of the debug.
        /// </value>
        public static ConsoleColor DebugColor { get; set; } = ConsoleColor.Gray;

        /// <summary>
        /// Gets or sets the color of the trace output logging.
        /// </summary>
        /// <value>
        /// The color of the trace.
        /// </value>
        public static ConsoleColor TraceColor { get; set; } = ConsoleColor.DarkGray;

        /// <summary>
        /// Gets or sets the color of the warning logging.
        /// </summary>
        /// <value>
        /// The color of the warn.
        /// </value>
        public static ConsoleColor WarnColor { get; set; } = ConsoleColor.Yellow;

        /// <summary>
        /// Gets or sets the color of the error logging.
        /// </summary>
        /// <value>
        /// The color of the error.
        /// </value>
        public static ConsoleColor ErrorColor { get; set; } = ConsoleColor.DarkRed;

        /// <summary>
        /// Gets or sets the color of the error logging.
        /// </summary>
        /// <value>
        /// The color of the error.
        /// </value>
        public static ConsoleColor FatalColor { get; set; } = ConsoleColor.Red;

        /// <inheritdoc />
        public LogLevel LogLevel { get; set; } = DebugLogger.IsDebuggerAttached ? LogLevel.Trace : LogLevel.Info;

        /// <inheritdoc />
        public void Log(LogMessageReceivedEventArgs logEvent)
        {
            // Select the writer based on the message type
            var writer = logEvent.MessageType == LogLevel.Error
                    ? TerminalWriters.StandardError
                    : TerminalWriters.StandardOutput;

            var (outputMessage, color) = GetOutputAndColor(logEvent);

            Terminal.Write(outputMessage, color, writer);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            // Do nothing
        }
    }
}
