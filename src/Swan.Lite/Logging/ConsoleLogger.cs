using System;

namespace Swan.Logging
{
    /// <summary>
    /// Represents a Console implementation of <c>ILogger</c>.
    /// </summary>
    /// <seealso cref="ILogger" />
    public class ConsoleLogger : ILogger
    {
        private static readonly object SyncLock = new object();
        
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
        /// Gets or sets the logging time format.
        /// set to null or empty to prevent output.
        /// </summary>
        /// <value>
        /// The logging time format.
        /// </value>
        public static string LoggingTimeFormat { get; set; } = "HH:mm:ss.fff";

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
        public LogMessageType LogLevel { get; set; } = LogMessageType.Info;

        /// <inheritdoc />
        public void Log(LogMessageReceivedEventArgs logEvent)
        {
            lock (SyncLock)
            {
                // TODO: Check level
                //if (!Terminal.GlobalLoggingMessageType.HasFlag(logEvent.MessageType))
                //    return;

                var prefix = GetConsoleColorAndPrefix(logEvent.MessageType, out var color);

                var loggerMessage = string.IsNullOrWhiteSpace(logEvent.Message)
                    ? string.Empty
                    : logEvent.Message.RemoveControlCharsExcept('\n');

                var outputMessage = CreateOutputMessage(logEvent.Source, loggerMessage, prefix, logEvent.UtcDate);

                // Select the writer based on the message type
                var writer = Terminal.IsConsolePresent
                    ? logEvent.MessageType.HasFlag(LogMessageType.Error) ? TerminalWriters.StandardError :
                    TerminalWriters.StandardOutput
                    : TerminalWriters.None;

                // Set the writer to Diagnostics if appropriate (Error and Debugging data go to the Diagnostics debugger
                // if it is attached at all
                if (Terminal.IsDebuggerAttached
                    && (Terminal.IsConsolePresent == false || logEvent.MessageType.HasFlag(LogMessageType.Debug) ||
                        logEvent.MessageType.HasFlag(LogMessageType.Error)))
                    writer = writer | TerminalWriters.Diagnostics;

                // Check if we really need to write this out
                if (writer == TerminalWriters.None) return;

                // Further format the output in the case there is an exception being logged
                if (writer.HasFlag(TerminalWriters.StandardError) && logEvent.Exception != null)
                {
                    try
                    {
                        outputMessage =
                            $"{outputMessage}{Environment.NewLine}{logEvent.Exception.Stringify().Indent()}";
                    }
                    catch
                    {
                        // Ignore  
                    }
                }

                Terminal.WriteLine(outputMessage, color, writer);
            }
        }

        internal static string GetConsoleColorAndPrefix(LogMessageType messageType, out ConsoleColor color)
        {
            string prefix;

            // Select color and prefix based on message type
            // and settings
            switch (messageType)
            {
                case LogMessageType.Debug:
                    color = DebugColor;
                    prefix = DebugPrefix;
                    break;
                case LogMessageType.Error:
                    color = ErrorColor;
                    prefix = ErrorPrefix;
                    break;
                case LogMessageType.Info:
                    color = InfoColor;
                    prefix = InfoPrefix;
                    break;
                case LogMessageType.Trace:
                    color = TraceColor;
                    prefix = TracePrefix;
                    break;
                case LogMessageType.Warning:
                    color = WarnColor;
                    prefix = WarnPrefix;
                    break;
                case LogMessageType.Fatal:
                    color = FatalColor;
                    prefix = FatalPrefix;
                    break;
                default:
                    color = Terminal.Settings.DefaultColor;
                    prefix = new string(' ', InfoPrefix.Length);
                    break;
            }

            return prefix;
        }

        internal static string CreateOutputMessage(string sourceName, string loggerMessage, string prefix, DateTime date)
        {
            var friendlySourceName = string.IsNullOrWhiteSpace(sourceName)
                ? string.Empty
                : sourceName.SliceLength(sourceName.LastIndexOf('.') + 1, sourceName.Length);

            var outputMessage = string.IsNullOrWhiteSpace(sourceName)
                ? loggerMessage
                : $"[{friendlySourceName}] {loggerMessage}";

            return string.IsNullOrWhiteSpace(LoggingTimeFormat)
                ? $" {prefix} >> {outputMessage}"
                : $" {date.ToLocalTime().ToString(LoggingTimeFormat)} {prefix} >> {outputMessage}";
        }
    }
}
