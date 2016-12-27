namespace Unosquare.Swan
{
    using System;
    using System.IO;
    using System.Linq;

    partial class Terminal
    {
        #region Write Methods

        /// <summary>
        /// Writes a character a number of times, optionally adding a new line at the end
        /// </summary>
        /// <param name="charCode">The character code.</param>
        /// <param name="color">The color.</param>
        /// <param name="count">The count.</param>
        /// <param name="newLine">if set to <c>true</c> [new line].</param>
        /// <param name="writer">The writer.</param>
        internal static void Write(this byte charCode, ConsoleColor color, int count, bool newLine, TextWriter writer)
        {
            if (IsConsolePresent == false) return;

            lock (SyncLock)
            {
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
                    OutputColor = color,
                    OutputText = buffer,
                    OutputWriter = writer
                };

                EnqueueOutput(context);
            }
        }

        /// <summary>
        /// Writes a character a number of times, optionally adding a new line at the end
        /// </summary>
        /// <param name="charCode">The character code.</param>
        /// <param name="color">The color.</param>
        /// <param name="count">The count.</param>
        /// <param name="newLine">if set to <c>true</c> [new line].</param>
        public static void Write(this byte charCode, ConsoleColor color, int count, bool newLine)
        {
            Write(charCode, color, count, newLine, Console.Out);
        }

        /// <summary>
        /// Writes a character a number of times, optionally adding a new line at the end
        /// it outputs to the console's standard error
        /// </summary>
        /// <param name="charCode">The character code.</param>
        /// <param name="color">The color.</param>
        /// <param name="count">The count.</param>
        /// <param name="newLine">if set to <c>true</c> [new line].</param>
        public static void WriteError(this byte charCode, ConsoleColor color, int count, bool newLine)
        {
            Write(charCode, color, count, newLine, Console.Error);
        }

        /// <summary>
        /// Writes the specified character in the default color.
        /// </summary>
        /// <param name="charCode">The character code.</param>
        /// <param name="writer">The writer.</param>
        internal static void Write(this char charCode, TextWriter writer = null)
        {
            if (writer == null && IsConsolePresent == false) return;

            lock (SyncLock)
            {

                var buffer = new[] { charCode };
                var context = new OutputContext()
                {
                    OutputColor = Settings.DefaultColor,
                    OutputText = buffer,
                    OutputWriter = writer ?? Console.Out
                };

                EnqueueOutput(context);
            }
        }

        /// <summary>
        /// Writes the specified character in the default color to the standard error
        /// </summary>
        /// <param name="charCode">The character code.</param>
        public static void WriteError(this char charCode)
        {
            Write(charCode, Console.Error);
        }

        /// <summary>
        /// Writes the specified text in the given color
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        /// <param name="writer">The writer.</param>
        internal static void Write(this string text, ConsoleColor color, TextWriter writer)
        {
            if (IsConsolePresent == false) return;
            if (text == null) return;

            lock (SyncLock)
            {
                var buffer = OutputEncoding.GetBytes(text);
                var context = new OutputContext()
                {
                    OutputColor = color,
                    OutputText = OutputEncoding.GetChars(buffer),
                    OutputWriter = writer
                };
                EnqueueOutput(context);
            }
        }

        /// <summary>
        /// Writes the specified text in the given color to the standard output
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        public static void Write(this string text, ConsoleColor color)
        {
            Write(text, color, Console.Out);
        }

        /// <summary>
        /// Writes the specified text in the given color to the standard error
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        public static void WriteError(this string text, ConsoleColor color)
        {
            Write(text, color, Console.Error);
        }

        /// <summary>
        /// Writes the specified text in the current console's foreground color.
        /// </summary>
        /// <param name="text">The text.</param>
        public static void Write(this string text)
        {
            text?.Write(Settings.DefaultColor);
        }

        #endregion

        #region WriteLine Methods

        /// <summary>
        /// Writes a New Line Sequence to the standard output
        /// </summary>
        /// <param name="writer">The writer.</param>
        public static void WriteLine(TextWriter writer = null)
        {
            Environment.NewLine.Write(Settings.DefaultColor, writer ?? Console.Out);
        }

        /// <summary>
        /// Writes a New Line Sequence to the standard error
        /// </summary>
        /// <param name="writer">The writer.</param>
        public static void WriteLineError(TextWriter writer = null)
        {
            Environment.NewLine.Write(Settings.DefaultColor, writer ?? Console.Error);
        }

        /// <summary>
        /// Writes a line of text in the current console foreground color
        /// to the standard output
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="writer">The writer.</param>
        public static void WriteLine(this string text, TextWriter writer = null)
        {
            text?.WriteLine(Settings.DefaultColor, writer ?? Console.Out);
        }

        /// <summary>
        /// Writes a line of text in the current console foreground color
        /// to the standard error as opposed to the standard output.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="writer">The writer.</param>
        public static void WriteLineError(this string text, TextWriter writer = null)
        {
            text?.WriteLineError(Settings.DefaultColor, writer ?? Console.Error);
        }

        /// <summary>
        /// Writes a line of text using the given color to the standard output
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        /// <param name="writer">The writer.</param>
        public static void WriteLine(this string text, ConsoleColor color, TextWriter writer = null)
        {
            if (text == null) return;
            $"{text}{Environment.NewLine}".Write(color, writer ?? Console.Out);
        }

        /// <summary>
        /// Writes a line of text using the given color to the standard error
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        /// <param name="writer">The writer.</param>
        public static void WriteLineError(this string text, ConsoleColor color, TextWriter writer = null)
        {
            if (text == null) return;
            $"{text}{Environment.NewLine}".Write(color, writer ?? Console.Error);
        }

        #endregion
    }
}