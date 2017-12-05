namespace Unosquare.Swan
{
    using System;

    /// <summary>
    /// A console terminal helper to create nicer output and receive input from the user
    /// This class is thread-safe :)
    /// </summary>
    public partial class Terminal
    {
        /// <summary>
        /// Terminal global settings
        /// </summary>
        public static class Settings
        {
            static Settings()
            {
                if (IsDebuggerAttached)
                {
                    DisplayLoggingMessageType =
                        LogMessageType.Debug |
                        LogMessageType.Error |
                        LogMessageType.Info |
                        LogMessageType.Trace |
                        LogMessageType.Warning;
                }
                else
                {
                    DisplayLoggingMessageType =
                        LogMessageType.Error |
                        LogMessageType.Info |
                        LogMessageType.Warning;
                }
            }

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
            public static string LoggingTimeFormat { get; set; } = "HH:mm:ss.fff";

            /// <summary>
            /// Gets or sets the logging message types (in a bitwise mask)
            /// to display in the console.
            /// </summary>
            /// <value>
            /// The console options.
            /// </value>
            public static LogMessageType DisplayLoggingMessageType { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether [override is console present].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [override is console present]; otherwise, <c>false</c>.
            /// </value>
            public static bool OverrideIsConsolePresent { get; set; }
        }
    }
}