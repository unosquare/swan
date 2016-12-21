namespace Unosquare.Swan
{
    using System;

    partial class Terminal
    {
        /// <summary>
        /// Defines a callback to determine a function that is called upon logging messages are sent to the terminal
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="text">The text.</param>
        /// <param name="source">The source.</param>
        public delegate void LoggingMessageCallback(LoggingMessageType messageType, string text, string source);

        /// <summary>
        /// Terminal global settings
        /// </summary>
        public static class Settings
        {
            /// <summary>
            /// Gets or sets the callback to be called asynchronously, whenever a logging message is received by the terminal.
            /// Only called when Terminal writes data via Info, Error, Trace, Warn, Debug methods, regardless of whether or not
            /// the console is present.
            /// </summary>
            public static OnMessageLoggedCallback OnMessageLogged { get; set; }

            /// <summary>
            /// Gets or sets the default output color.
            /// </summary>
            /// <value>
            /// The default color.
            /// </value>
            public static ConsoleColor DefaultColor { get; set; } = Console.ForegroundColor;

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
            public static ConsoleColor ErrorColor { get; set; } = ConsoleColor.Red;

            /// <summary>
            /// Gets or sets the information logging prefix.
            /// </summary>
            /// <value>
            /// The information prefix.
            /// </value>
            public static string InfoPrefix { get; set; } = "INF";

            /// <summary>
            /// Gets or sets the user input prefix.
            /// </summary>
            /// <value>
            /// The user input prefix.
            /// </value>
            public static string UserInputPrefix { get; set; } = "USR";

            /// <summary>
            /// Gets or sets the user option text.
            /// </summary>
            /// <value>
            /// The user option text.
            /// </value>
            public static string UserOptionText { get; set; } = " Option: ";

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
            /// Gets or sets the error logging prefix.
            /// </summary>
            /// <value>
            /// The error prefix.
            /// </value>
            public static string ErrorPrefix { get; set; } = "ERR";

            /// <summary>
            /// Gets or sets the logging time format.
            /// set to null or empty to prevent output
            /// </summary>
            /// <value>
            /// The logging time format.
            /// </value>
            public static string LoggingTimeFormat { get; set; } = "HH:mm:ss";

            /// <summary>
            /// Gets or sets the console logging options in a bitwise mask.
            /// </summary>
            /// <value>
            /// The console options.
            /// </value>
            public static LoggingMessageType ConsoleOptions { get; set; }

            
        }
    }
}