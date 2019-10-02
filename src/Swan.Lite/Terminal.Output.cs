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
        /// Writes a character a number of times, optionally adding a new line at the end.
        /// </summary>
        /// <param name="charCode">The character code.</param>
        /// <param name="color">The color.</param>
        /// <param name="count">The count.</param>
        /// <param name="newLine">if set to <c>true</c> [new line].</param>
        /// <param name="writerFlags">The writer flags.</param>
        public static void Write(char charCode, ConsoleColor? color = null, int count = 1, bool newLine = false, TerminalWriters writerFlags = TerminalWriters.StandardOutput)
        {
            lock (SyncLock)
            {
                var text = new string(charCode, count);

                if (newLine)
                {
                    text += Environment.NewLine;
                }

                var buffer = OutputEncoding.GetBytes(text);
                var context = new OutputContext
                {
                    OutputColor = color ?? Settings.DefaultColor,
                    OutputText = OutputEncoding.GetChars(buffer),
                    OutputWriters = writerFlags,
                };

                EnqueueOutput(context);
            }
        }

        /// <summary>
        /// Writes the specified text in the given color.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        /// <param name="writerFlags">The writer flags.</param>
        public static void Write(string? text, ConsoleColor? color = null, TerminalWriters writerFlags = TerminalWriters.StandardOutput)
        {
            if (text == null) return;

            lock (SyncLock)
            {
                var buffer = OutputEncoding.GetBytes(text);
                var context = new OutputContext
                {
                    OutputColor = color ?? Settings.DefaultColor,
                    OutputText = OutputEncoding.GetChars(buffer),
                    OutputWriters = writerFlags,
                };

                EnqueueOutput(context);
            }
        }

        /// <summary>
        /// Writes a New Line Sequence to the standard output.
        /// </summary>
        /// <param name="writerFlags">The writer flags.</param>
        public static void WriteLine(TerminalWriters writerFlags = TerminalWriters.StandardOutput)
            => Write(Environment.NewLine, Settings.DefaultColor, writerFlags);

        /// <summary>
        /// Writes a line of text in the current console foreground color
        /// to the standard output.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        /// <param name="writerFlags">The writer flags.</param>
        public static void WriteLine(string text, ConsoleColor? color = null, TerminalWriters writerFlags = TerminalWriters.StandardOutput)
            => Write($"{text ?? string.Empty}{Environment.NewLine}", color, writerFlags);

        /// <summary>
        /// As opposed to WriteLine methods, it prepends a Carriage Return character to the text
        /// so that the console moves the cursor one position up after the text has been written out.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        /// <param name="writerFlags">The writer flags.</param>
        public static void OverwriteLine(string text, ConsoleColor? color = null, TerminalWriters writerFlags = TerminalWriters.StandardOutput)
        {
            Write($"\r{text ?? string.Empty}", color, writerFlags);
            Flush();
            CursorLeft = 0;
        }
    }
}
