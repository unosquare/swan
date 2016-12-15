namespace Unosquare.Swan
{
    using System;
    using System.Collections.Generic;

    partial class Terminal
    {
        /// <summary>
        /// Reads a key from the terminal preventing the key from being echoed.
        /// </summary>
        /// <param name="prompt">The prompt.</param>
        /// <returns></returns>
        public static ConsoleKeyInfo ReadKey(this string prompt)
        {
            if (IsConsolePresent == false) return new ConsoleKeyInfo();

            return prompt.ReadKey(true);
        }

        /// <summary>
        /// Reads a key from the Terminal
        /// </summary>
        /// <param name="prompt">The prompt.</param>
        /// <param name="preventEcho">if set to <c>true</c> [prevent echo].</param>
        /// <returns></returns>
        public static ConsoleKeyInfo ReadKey(this string prompt, bool preventEcho)
        {
            if (IsConsolePresent == false) return new ConsoleKeyInfo();

            if (prompt != null)
                $" {DateTime.Now:HH:mm:ss} USR << {prompt} ".Write(ConsoleColor.White);

            var input = ReadKey(true);
            var echo = preventEcho ? string.Empty : input.Key.ToString();
            echo.WriteLine();
            return input;
        }

        /// <summary>
        /// Reads a number from the input. If unable to parse, it returns the default number
        /// </summary>
        /// <param name="prompt">The prompt.</param>
        /// <param name="defaultNumber">The default number.</param>
        /// <returns></returns>
        public static int ReadNumber(this string prompt, int defaultNumber)
        {
            if (IsConsolePresent == false) return defaultNumber;

            $" {DateTime.Now:HH:mm:ss} USR << {prompt} (default is {defaultNumber}): ".Write(ConsoleColor.White);
            var input = ReadLine();
            var parsedInt = defaultNumber;
            if (int.TryParse(input, out parsedInt) == false)
            {
                parsedInt = defaultNumber;
            }

            return parsedInt;
        }

        /// <summary>
        /// Reads the prompt.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="options">The options.</param>
        /// <param name="anyKeyOption">Any key option.</param>
        /// <returns></returns>
        public static ConsoleKeyInfo ReadPrompt(this string title, Dictionary<ConsoleKey, string> options, string anyKeyOption)
        {
            if (IsConsolePresent == false) return new ConsoleKeyInfo();

            var inputLeft = 0;
            var inputTop = 0;

            var textColor = ConsoleColor.White;
            var lineLength = Console.BufferWidth;
            var lineAlign = -(lineLength - 2);
            var textFormat = "{0," + lineAlign.ToString() + "}";

            lock (SyncLock) // lock the output as an atomic operation
            {
                { // Top border
                    Table.TopLeft();
                    Table.Horizontal(-lineAlign);
                    Table.TopRight();
                }

                { // Title
                    Table.Vertical();
                    var titleText = string.Format(textFormat,
                        string.IsNullOrWhiteSpace(title) ?
                            $" Select an option from the list below." :
                            $" {title}");
                    titleText.Write(textColor); //, titleText);
                    Table.Vertical();
                }

                { // Title Bottom
                    Table.LeftTee();
                    Table.Horizontal(lineLength - 2);
                    Table.RightTee();
                }

                // Options
                foreach (var kvp in options)
                {
                    Table.Vertical();
                    string.Format(textFormat,
                        $"    {"[ " + kvp.Key.ToString() + " ]",-10}  {kvp.Value}").Write(textColor);
                    Table.Vertical();
                }

                // Any Key Options
                if (string.IsNullOrWhiteSpace(anyKeyOption) == false)
                {
                    Table.Vertical();
                    string.Format(textFormat, " ").Write(ConsoleColor.Gray);
                    Table.Vertical();

                    Table.Vertical();
                    string.Format(textFormat,
                        $"    {" ",-10}  {anyKeyOption}").Write(ConsoleColor.Gray);
                    Table.Vertical();
                }

                inputLeft = 12;
                inputTop = CursorTop - 1;

                { // Input
                    Table.LeftTee();
                    Table.Horizontal(lineLength - 2);
                    Table.RightTee();

                    Table.Vertical();
                    string.Format(textFormat,
                        $" Option: ").Write(ConsoleColor.Green);
                    inputTop = CursorTop;
                    Table.Vertical();

                    Table.BottomLeft();
                    Table.Horizontal(lineLength - 2);
                    Table.BottomRight();
                }

            }

            var currentTop = CursorTop;
            var currentLeft = CursorLeft;

            SetCursorPosition(inputLeft, inputTop);
            var userInput = ReadKey(true);
            userInput.Key.ToString().Write(ConsoleColor.Gray);

            SetCursorPosition(currentLeft, currentTop);
            return userInput;
        }
    }
}
