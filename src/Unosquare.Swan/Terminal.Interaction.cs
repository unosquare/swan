namespace Unosquare.Swan
{
    using System;
    using System.Collections.Generic;

    partial class Terminal
    {

        /// <summary>
        /// Reads a key from the Terminal
        /// </summary>
        /// <param name="prompt">The prompt.</param>
        /// <param name="preventEcho">if set to <c>true</c> [prevent echo].</param>
        /// <returns></returns>
        static public ConsoleKeyInfo ReadKey(this string prompt, bool preventEcho)
        {
            $" {DateTime.Now:HH:mm:ss} USR << {prompt} ".Write(ConsoleColor.White);
            var input = Console.ReadKey(true);
            var echo = preventEcho ? string.Empty : input.Key.ToString();
            Console.WriteLine(echo);
            return input;
        }

        /// <summary>
        /// Reads a number from the input. If unable to parse, it returns the default number
        /// </summary>
        /// <param name="prompt">The prompt.</param>
        /// <param name="defaultNumber">The default number.</param>
        /// <returns></returns>
        static public int ReadNumber(this string prompt, int defaultNumber)
        {
            $" {DateTime.Now:HH:mm:ss} USR << {prompt} (default is {defaultNumber}): ".Write(ConsoleColor.White);
            var input = Console.ReadLine();
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
        static public ConsoleKeyInfo ReadPrompt(string title, Dictionary<ConsoleKey, string> options, string anyKeyOption)
        {
            lock (SyncLock)
            {
                var textColor = ConsoleColor.White;
                var lineLength = Console.BufferWidth;
                var lineAlign = -(lineLength - 2);
                var textFormat = "{0," + lineAlign.ToString() + "}";

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
                    titleText.Write(textColor);
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

                var inputLeft = 12;
                var inputTop = Console.CursorTop - 1;

                { // Input
                    Table.LeftTee();
                    Table.Horizontal(lineLength - 2);
                    Table.RightTee();

                    Table.Vertical();
                    string.Format(textFormat,
                        $" Option: ").Write(ConsoleColor.Green);
                    inputTop = Console.CursorTop;
                    Table.Vertical();

                    Table.BottomLeft();
                    Table.Horizontal(lineLength - 2);
                    Table.BottomRight();
                }

                var currentTop = Console.CursorTop;
                var currentLeft = Console.CursorLeft;

                Console.SetCursorPosition(inputLeft, inputTop);
                var userInput = Console.ReadKey(true);
                userInput.Key.ToString().Write(ConsoleColor.Gray);

                Console.SetCursorPosition(currentLeft, currentTop);
                return userInput;
            }

        }
    }
}
