namespace Swan.Platform
{
    using System;
    using System.Text;

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
        public static void Write(char charCode, ConsoleColor? color = null, int count = 1, bool newLine = false, TerminalWriterFlags writerFlags = TerminalWriterFlags.StandardOutput)
        {
            lock (SyncLock)
            {
                var builder = new StringBuilder(count + Environment.NewLine.Length);
                builder.Append(charCode, count);
                if (newLine)
                    builder.Append(Environment.NewLine);

                var context = new OutputContext(
                    builder.ToString(), writerFlags, color ?? Settings.DefaultColor, Settings.DefaultColor);
                EnqueueOutput(context);
            }
        }

        /// <summary>
        /// Writes the specified text in the given color.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        /// <param name="writerFlags">The writer flags.</param>
        public static void Write(string? text, ConsoleColor? color = null, TerminalWriterFlags writerFlags = TerminalWriterFlags.StandardOutput)
        {
            if (text == null) return;

            lock (SyncLock)
            {
                var context = new OutputContext(
                    text, writerFlags, color ?? Settings.DefaultColor, Settings.DefaultColor);
                var buffer = OutputEncoding.GetBytes(text);
                EnqueueOutput(context);
            }
        }

        /// <summary>
        /// Writes a New Line Sequence to the standard output.
        /// </summary>
        /// <param name="writerFlags">The writer flags.</param>
        public static void WriteLine(TerminalWriterFlags writerFlags = TerminalWriterFlags.StandardOutput)
            => Write(Environment.NewLine, Settings.DefaultColor, writerFlags);

        /// <summary>
        /// Writes a line of text in the current console foreground color
        /// to the standard output.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        /// <param name="writerFlags">The writer flags.</param>
        public static void WriteLine(string text, ConsoleColor? color = null, TerminalWriterFlags writerFlags = TerminalWriterFlags.StandardOutput)
        {
            var outputText = text ?? string.Empty;
            var builder = new StringBuilder(outputText.Length + Environment.NewLine.Length);
            builder
                .Append(outputText)
                .Append(Environment.NewLine);

            Write(builder.ToString(), color, writerFlags);
        }
            

        /// <summary>
        /// As opposed to WriteLine methods, it prepends a Carriage Return character to the text
        /// so that the console moves the cursor one position up after the text has been written out.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        /// <param name="writerFlags">The writer flags.</param>
        public static void OverwriteLine(string text, ConsoleColor? color = null, TerminalWriterFlags writerFlags = TerminalWriterFlags.StandardOutput)
        {
            Write($"\r{text ?? string.Empty}", color, writerFlags);
            Flush();
            CursorLeft = 0;
        }
    }
}
