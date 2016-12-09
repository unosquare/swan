namespace Unosquare.Swan
{
    using System;

    partial class Terminal
    {

        /// <summary>
        /// Defines the bitwise flags to determine
        /// which types of messages get printed on the current console
        /// </summary>
        public enum LoggingMessageType
        {
            None = 0,
            Info = 1,
            Debug = 2,
            Trace = 4,
            Error = 8,
            Warning = 16,
        }

        /// <summary>
        /// Defines a callback to determine a function that is called upon logging messages are sent to the terminal
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="text">The text.</param>
        /// <param name="source">The source.</param>
        public delegate void LoggingMessageCallback(LoggingMessageType messageType, string text, string source);

        static public class Settings
        {
            /// <summary>
            /// Gets or sets the default output color.
            /// </summary>
            /// <value>
            /// The default color.
            /// </value>
            static public ConsoleColor DefaultColor { get; set; } = Console.ForegroundColor;
            /// <summary>
            /// Gets or sets the color of the information output logging.
            /// </summary>
            /// <value>
            /// The color of the information.
            /// </value>
            static public ConsoleColor InfoColor { get; set; } = ConsoleColor.Cyan;
            /// <summary>
            /// Gets or sets the color of the debug output logging.
            /// </summary>
            /// <value>
            /// The color of the debug.
            /// </value>
            static public ConsoleColor DebugColor { get; set; } = ConsoleColor.Gray;
            /// <summary>
            /// Gets or sets the color of the trace output logging.
            /// </summary>
            /// <value>
            /// The color of the trace.
            /// </value>
            static public ConsoleColor TraceColor { get; set; } = ConsoleColor.DarkGray;
            /// <summary>
            /// Gets or sets the color of the warning logging.
            /// </summary>
            /// <value>
            /// The color of the warn.
            /// </value>
            static public ConsoleColor WarnColor { get; set; } = ConsoleColor.Yellow;
            /// <summary>
            /// Gets or sets the color of the error logging.
            /// </summary>
            /// <value>
            /// The color of the error.
            /// </value>
            static public ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;

            /// <summary>
            /// Gets or sets the information logging prefix.
            /// </summary>
            /// <value>
            /// The information prefix.
            /// </value>
            static public string InfoPrefix { get; set; } = "INF";
            /// <summary>
            /// Gets or sets the debug logging prefix.
            /// </summary>
            /// <value>
            /// The debug prefix.
            /// </value>
            static public string DebugPrefix { get; set; } = "DBG";
            /// <summary>
            /// Gets or sets the trace logging prefix.
            /// </summary>
            /// <value>
            /// The trace prefix.
            /// </value>
            static public string TracePrefix { get; set; } = "TRC";
            /// <summary>
            /// Gets or sets the warnning logging prefix.
            /// </summary>
            /// <value>
            /// The warn prefix.
            /// </value>
            static public string WarnPrefix { get; set; } = "WRN";
            /// <summary>
            /// Gets or sets the error logging prefix.
            /// </summary>
            /// <value>
            /// The error prefix.
            /// </value>
            static public string ErrorPrefix { get; set; } = "ERR";

            /// <summary>
            /// Gets or sets the logging time format.
            /// set to null or empty to prevent output
            /// </summary>
            /// <value>
            /// The logging time format.
            /// </value>
            static public string LoggingTimeFormat { get; set; } = "HH:mm:ss";

            /// <summary>
            /// Gets or sets the console logging options in a bitwise mask.
            /// </summary>
            /// <value>
            /// The console options.
            /// </value>
            static public LoggingMessageType ConsoleOptions { get; set; }
        }
    }
}
