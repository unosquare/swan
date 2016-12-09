namespace Unosquare.Swan
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

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
        static public void Write(this byte charCode, ConsoleColor color, int count, bool newLine)
        {
            lock (SyncLock)
            {
                var bytes = new byte[count];
                for (var i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = charCode;
                }

                if (newLine)
                {
                    var newLineBytes = Console.OutputEncoding.GetBytes(Environment.NewLine);
                    bytes = bytes.Union(newLineBytes).ToArray();
                }

                var buffer = Console.OutputEncoding.GetChars(bytes);
                var context = new OutputContext()
                {
                    OutputColor = color,
                    OutputText = buffer
                };

                OutputQueue.Enqueue(context);
            }

        }

        /// <summary>
        /// Writes the specified text in the given color
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        static public void Write(this string text, ConsoleColor color)
        {
            if (text == null) return;

            lock (SyncLock)
            {
                var buffer = Console.OutputEncoding.GetBytes(text);
                var context = new OutputContext()
                {
                    OutputColor = color,
                    OutputText = Console.OutputEncoding.GetChars(buffer)
                };
                OutputQueue.Enqueue(context);
            }
        }

        /// <summary>
        /// Writes the specified text in the current consoloe's foreground color.
        /// </summary>
        /// <param name="text">The text.</param>
        static public void Write(this string text)
        {
            text?.Write(Console.ForegroundColor);
        }

        #endregion

        #region WriteLine Methods

        /// <summary>
        /// Writes a New Line Sequence
        /// </summary>
        static public void WriteLine()
        {
            Environment.NewLine.Write();
        }

        /// <summary>
        /// Writes a line of text in the current console foreground color
        /// </summary>
        /// <param name="text">The text.</param>
        static public void WriteLine(this string text)
        {
            text?.WriteLine(Console.ForegroundColor);
        }

        /// <summary>
        /// Writes a line of text using the given color
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="color">The color.</param>
        static public void WriteLine(this string text, ConsoleColor color)
        {
            if (text == null) return;
            $"{text}{Environment.NewLine}".Write(color);
        }

        #endregion
    }
}
