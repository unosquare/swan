using System;
using System.Collections.Generic;
using System.Globalization;
using Swan.Logging;

namespace Swan
{
    /// <summary>
    /// A console terminal helper to create nicer output and receive input from the user
    /// This class is thread-safe :).
    /// </summary>
    public static partial class Terminal
    {
        #region ReadKey

        /// <summary>
        /// Reads a key from the Terminal. This is the closest equivalent to Console.ReadKey.
        /// </summary>
        /// <param name="intercept">if set to <c>true</c> the pressed key will not be rendered to the output.</param>
        /// <param name="disableLocking">if set to <c>true</c> the output will continue to be shown.
        /// This is useful for services and daemons that are running as console applications and wait for a key to exit the program.</param>
        /// <returns>The console key information.</returns>
        public static ConsoleKeyInfo ReadKey(bool intercept, bool disableLocking = false)
        {
            if (!IsConsolePresent) return default;
            if (disableLocking) return Console.ReadKey(intercept);

            lock (SyncLock)
            {
                Flush();
                InputDone.Reset();

                try
                {
                    Console.CursorVisible = true;
                    return Console.ReadKey(intercept);
                }
                finally
                {
                    Console.CursorVisible = false;
                    InputDone.Set();
                }
            }
        }

        /// <summary>
        /// Reads a key from the Terminal.
        /// </summary>
        /// <param name="prompt">The prompt.</param>
        /// <param name="preventEcho">if set to <c>true</c> [prevent echo].</param>
        /// <returns>The console key information.</returns>
        public static ConsoleKeyInfo ReadKey(string prompt, bool preventEcho = true)
        {
            if (!IsConsolePresent) return default;

            lock (SyncLock)
            {
                if (prompt != null)
                {
                    Write($"{GetNowFormatted()}{Settings.UserInputPrefix} << {prompt} ", ConsoleColor.White);
                }

                var input = ReadKey(true);
                var echo = preventEcho ? string.Empty : input.Key.ToString();
                WriteLine(echo);
                return input;
            }
        }

        #endregion

        #region Other Terminal Read Methods

        /// <summary>
        /// Clears the screen.
        /// </summary>
        public static void Clear()
        {
            Flush();
            Console.Clear();
        }

        /// <summary>
        /// Reads a line of text from the console.
        /// </summary>
        /// <returns>The read line.</returns>
        public static string? ReadLine()
        {
            if (IsConsolePresent == false) return default;

            lock (SyncLock)
            {
                Flush();
                InputDone.Reset();

                try
                {
                    Console.CursorVisible = true;
                    return Console.ReadLine();
                }
                finally
                {
                    Console.CursorVisible = false;
                    InputDone.Set();
                }
            }
        }

        /// <summary>
        /// Reads a line from the input.
        /// </summary>
        /// <param name="prompt">The prompt.</param>
        /// <returns>The read line.</returns>
        public static string? ReadLine(string prompt)
        {
            if (!IsConsolePresent) return null;

            lock (SyncLock)
            {
                Write($"{GetNowFormatted()}{Settings.UserInputPrefix} << {prompt}: ", ConsoleColor.White);

                return ReadLine();
            }
        }

        /// <summary>
        /// Reads a number from the input. If unable to parse, it returns the default number.
        /// </summary>
        /// <param name="prompt">The prompt.</param>
        /// <param name="defaultNumber">The default number.</param>
        /// <returns>
        /// Conversions of string representation of a number to its 32-bit signed integer equivalent.
        /// </returns>
        public static int ReadNumber(string prompt, int defaultNumber)
        {
            if (!IsConsolePresent) return defaultNumber;

            lock (SyncLock)
            {
                Write($"{GetNowFormatted()}{Settings.UserInputPrefix} << {prompt} (default is {defaultNumber}): ",
                    ConsoleColor.White);

                var input = ReadLine();
                return int.TryParse(input, out var parsedInt) ? parsedInt : defaultNumber;
            }
        }

        /// <summary>
        /// Creates a table prompt where the user can enter an option based on the options dictionary provided.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="options">The options.</param>
        /// <param name="anyKeyOption">Any key option.</param>
        /// <returns>
        /// A value that identifies the console key that was pressed.
        /// </returns>
        /// <exception cref="ArgumentNullException">options.</exception>
        public static ConsoleKeyInfo ReadPrompt(
            string title,
            IDictionary<ConsoleKey, string> options,
            string anyKeyOption)
        {
            if (!IsConsolePresent) return default;

            if (options == null)
                throw new ArgumentNullException(nameof(options));

            const ConsoleColor textColor = ConsoleColor.White;
            var lineLength = Console.WindowWidth;
            var lineAlign = -(lineLength - 2);
            var textFormat = "{0," + lineAlign + "}";

            // lock the output as an atomic operation
            lock (SyncLock)
            {
                {
                    // Top border
                    Table.TopLeft();
                    Table.Horizontal(-lineAlign);
                    Table.TopRight();
                }

                {
                    // Title
                    Table.Vertical();
                    var titleText = string.Format(CultureInfo.CurrentCulture,
                        textFormat,
                        string.IsNullOrWhiteSpace(title) ? " Select an option from the list below." : $" {title}");
                    Write(titleText, textColor);
                    Table.Vertical();
                }

                {
                    // Title Bottom
                    Table.LeftTee();
                    Table.Horizontal(lineLength - 2);
                    Table.RightTee();
                }

                // Options
                foreach (var kvp in options)
                {
                    Table.Vertical();
                    Write(string.Format(
                            CultureInfo.CurrentCulture,
                            textFormat,
                            $"    {"[ " + kvp.Key + " ]",-10}  {kvp.Value}"),
                        textColor);
                    Table.Vertical();
                }

                // Any Key Options
                if (string.IsNullOrWhiteSpace(anyKeyOption) == false)
                {
                    Table.Vertical();
                    Write(string.Format(CultureInfo.CurrentCulture, textFormat, " "), ConsoleColor.Gray);
                    Table.Vertical();

                    Table.Vertical();
                    Write(string.Format(
                            CultureInfo.CurrentCulture,
                            textFormat,
                            $"    {" ",-10}  {anyKeyOption}"),
                        ConsoleColor.Gray);
                    Table.Vertical();
                }

                {
                    // Input section
                    Table.LeftTee();
                    Table.Horizontal(lineLength - 2);
                    Table.RightTee();

                    Table.Vertical();
                    Write(string.Format(CultureInfo.CurrentCulture, textFormat, Settings.UserOptionText),
                        ConsoleColor.Green);
                    Table.Vertical();

                    Table.BottomLeft();
                    Table.Horizontal(lineLength - 2);
                    Table.BottomRight();
                }
            }

            var inputLeft = Settings.UserOptionText.Length + 3;

            SetCursorPosition(inputLeft, CursorTop - 1);
            var userInput = ReadKey(true);
            Write(userInput.Key.ToString(), ConsoleColor.Gray);

            SetCursorPosition(0, CursorTop + 2);
            return userInput;
        }

        #endregion

        private static string GetNowFormatted() =>
            $" {(string.IsNullOrWhiteSpace(TextLogger.LoggingTimeFormat) ? string.Empty : DateTime.Now.ToString(TextLogger.LoggingTimeFormat) + " ")}";
    }
}
