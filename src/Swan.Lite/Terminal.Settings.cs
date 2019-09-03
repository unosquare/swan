using System;

namespace Swan
{
    /// <summary>
    /// A console terminal helper to create nicer output and receive input from the user
    /// This class is thread-safe :).
    /// </summary>
    public static partial class Terminal
    {
        /// <summary>
        /// Terminal global settings.
        /// </summary>
        public static class Settings
        {
            /// <summary>
            /// Gets or sets the default output color.
            /// </summary>
            /// <value>
            /// The default color.
            /// </value>
            public static ConsoleColor DefaultColor { get; set; } = Console.ForegroundColor;

            /// <summary>
            /// Gets the color of the border.
            /// </summary>
            /// <value>
            /// The color of the border.
            /// </value>
            public static ConsoleColor BorderColor { get; } = ConsoleColor.DarkGreen;

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
        }
    }
}
