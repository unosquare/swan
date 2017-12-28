namespace Unosquare.Swan
{
    using System;
    using System.Linq;

    /// <summary>
    /// A console terminal helper to create nicer output and receive input from the user
    /// This class is thread-safe :)
    /// </summary>
    public partial class Terminal
    {
        #region Helper Methods

        /// <summary>
        /// Prints all characters in the current code page.
        /// This is provided for debugging purposes only.
        /// </summary>
        public static void PrintCurrentCodePage()
        {
            if (IsConsolePresent == false) return;

            lock (SyncLock)
            {
                $"Output Encoding: {OutputEncoding}".WriteLine();
                for (byte byteValue = 0; byteValue < byte.MaxValue; byteValue++)
                {
                    var charValue = OutputEncoding.GetChars(new[] { byteValue })[0];
                    switch (byteValue)
                    {
                        case 8: // Backspace
                        case 9: // Tab
                        case 10: // Line feed
                        case 13: // Carriage return
                            charValue = '.';
                            break;
                    }

                    $"{byteValue:000} {charValue}   ".Write();

                    // 7 is a beep -- Console.Beep() also works
                    if (byteValue == 7) " ".Write();

                    if ((byteValue + 1) % 8 == 0)
                        WriteLine();
                }

                WriteLine();
            }
        }

        #endregion

        #region Write Methods

        /// <summary>
        /// Writes a character a number of times, optionally adding a new line at the end
        /// </summary>
        /// <param name="charCode">The character code.</param>
        /// <param name="color">The color.</param>
        /// <param name="count">The count.</param>
        /// <param name="newLine">if set to <c>true</c> [new line].</param>
        /// <param name="writerFlags">The writer flags.</param>
        public static void Write(this byte charCode, ConsoleColor? color = null, int count = 1, bool newLine = false, TerminalWriters writerFlags = TerminalWriters.StandardOutput)
        {
            lock (SyncLock)
            {
                if (color == null) color = Settings.DefaultColor;
                var bytes = new byte[count];
                for (var i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = charCode;
                }

                if (newLine)
                {
                    var newLineBytes = OutputEncoding.GetBytes(Environment.NewLine);
                    bytes = bytes.Union(newLineBytes).ToArray();
                }

                var buffer = OutputEncoding.GetChars(bytes);
                var context = new OutputContext()
                {
                    OutputColor = color.Value,
                    OutputText = buffer,
                    OutputWriters = writerFlags
                };

                EnqueueOutput(context);
            }
        }

        /// <summary>
        /// Writes the specified character in the default color.
        /// </summary>
        /// <param name="charCode">The character code.</param>
        /// <param name="color">The color.</param>
        /// <param name="writerFlags">The writer flags.</param>
        public static void Write(this char charCode, ConsoleColor? color = null, TerminalWriters writerFlags = TerminalWriters.StandardOutput)
        {
            lock (SyncLock)
            {
                if (color == null) color = Settings.DefaultColor;
                var buffer = new[] { charCode };
                var context = new OutputContext()
                {
                    OutputColor = color.Value,
                    OutputText = buffer,
                    OutputWriters = writerFlags
                };

                EnqueueOutput(context);
            }
        }

        /// <summary>
        /// Writes the specified text in the given color
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        /// <param name="writerFlags">The writer flags.</param>
        public static void Write(this string text, ConsoleColor? color = null, TerminalWriters writerFlags = TerminalWriters.StandardOutput)
        {
            if (text == null) return;
            if (color == null) color = Settings.DefaultColor;

            lock (SyncLock)
            {
                var buffer = OutputEncoding.GetBytes(text);
                var context = new OutputContext()
                {
                    OutputColor = color.Value,
                    OutputText = OutputEncoding.GetChars(buffer),
                    OutputWriters = writerFlags
                };

                EnqueueOutput(context);
            }
        }

        #endregion

        #region WriteLine Methods

        /// <summary>
        /// Writes a New Line Sequence to the standard output
        /// </summary>
        /// <param name="writerFlags">The writer flags.</param>
        public static void WriteLine(TerminalWriters writerFlags = TerminalWriters.StandardOutput)
        {
            Environment.NewLine.Write(Settings.DefaultColor, writerFlags);
        }

        /// <summary>
        /// Writes a line of text in the current console foreground color
        /// to the standard output
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        /// <param name="writerFlags">The writer flags.</param>
        public static void WriteLine(this string text, ConsoleColor? color = null, TerminalWriters writerFlags = TerminalWriters.StandardOutput)
        {
            if (text == null) text = string.Empty;
            $"{text}{Environment.NewLine}".Write(color, writerFlags);
        }

        /// <summary>
        /// As opposed to WriteLine methods, it prepends a Carriage Return character to the text
        /// so that the console moves the cursor one position up after the text has been written out.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        /// <param name="writerFlags">The writer flags.</param>
        public static void OverwriteLine(this string text, ConsoleColor? color = null, TerminalWriters writerFlags = TerminalWriters.StandardOutput)
        {
            if (text == null) text = string.Empty;
            $"\r{text}".Write(color, writerFlags);
            Flush();
            CursorLeft = 0;
        }

        #endregion
    }
}