namespace Unosquare.Swan
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;

    /// <summary>
    /// A console terminal helper to create nicer output and receive input from the user
    /// This class is thread-safe :)
    /// </summary>
    static public partial class Terminal
    {
        static private readonly object SyncLock = new object();
        private static readonly ConcurrentQueue<OutputContext> OutputQueue = new ConcurrentQueue<OutputContext>();
        private static readonly Task OutputTask;

        /// <summary>
        /// Represents an asynchronous output context
        /// </summary>
        private class OutputContext
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="OutputContext"/> class.
            /// </summary>
            public OutputContext()
            {
                OriginalColor = Console.ForegroundColor;
            }

            public ConsoleColor OriginalColor { get; set; }
            public ConsoleColor OutputColor { get; set; }
            public char[] OutputText { get; set; }
        }

        /// <summary>
        /// Initializes the <see cref="Terminal"/> class.
        /// </summary>
        static Terminal()
        {
            OutputTask = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    if (OutputQueue.Count <= 0)
                        await Task.Delay(1);

                    while (OutputQueue.Count > 0)
                    {
                        OutputContext context;
                        if (OutputQueue.TryDequeue(out context) == false)
                            continue;

                        Console.ForegroundColor = context.OutputColor;
                        Console.Write(context.OutputText);
                        Console.ResetColor();
                        Console.ForegroundColor = context.OriginalColor;
                    }
                }
            });
        }

        /// <summary>
        /// Prints all characters in the current code page.
        /// </summary>
        static public void PrintCurrentCodePage()
        {
            lock (SyncLock)
            {
                $"Output Encoding: {Console.OutputEncoding.ToString()}".WriteLine();
                for (byte b = 0; b < byte.MaxValue; b++)
                {
                    char c = Console.OutputEncoding.GetChars(new byte[] { b })[0];
                    switch (b)
                    {
                        case 8: // Backspace
                        case 9: // Tab
                        case 10: // Line feed
                        case 13: // Carriage return
                            c = '.';
                            break;
                    }

                    string.Format("{0:000} {1}   ", b, c).Write();

                    // 7 is a beep -- Console.Beep() also works
                    if (b == 7) " ".Write();

                    if ((b + 1) % 8 == 0)
                        Terminal.WriteLine();
                }
                Terminal.WriteLine();
            }
            
        }

        static public class Settings
        {
            // TODO: add color and other settings such as callbacks and stuff
        }

    }
}