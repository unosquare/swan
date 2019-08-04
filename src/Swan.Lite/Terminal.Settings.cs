﻿using System;

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
            /// Gets or sets a value indicating whether [override is console present].
            /// </summary>
            /// <value>
            ///   <c>true</c> if [override is console present]; otherwise, <c>false</c>.
            /// </value>
            public static bool OverrideIsConsolePresent { get; set; }
        }
    }
}
