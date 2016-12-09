namespace Unosquare.Swan
{
    using System;
    using System.Collections.Concurrent;
    using System.Text;
    using System.Threading;
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

        static private readonly ManualResetEventSlim OutputDone = new ManualResetEventSlim(false);
        static private readonly ManualResetEventSlim InputDone = new ManualResetEventSlim(true);

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
                OriginalColor = Settings.DefaultColor;
            }

            public ConsoleColor OriginalColor { get; set; }
            public ConsoleColor OutputColor { get; set; }
            public char[] OutputText { get; set; }
        }

        /// <summary>
        /// Enqueues the output to be written to the console
        /// </summary>
        /// <param name="context">The context.</param>
        static private void EnqueueOutput(OutputContext context)
        {
            OutputDone.Reset();
            OutputQueue.Enqueue(context);
        }

        /// <summary>
        /// Initializes the <see cref="Terminal"/> class.
        /// </summary>
        static Terminal()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                Settings.ConsoleOptions =
                    LoggingMessageType.Debug |
                    LoggingMessageType.Error |
                    LoggingMessageType.Info |
                    LoggingMessageType.Trace |
                    LoggingMessageType.Warning;
            }
            else
            {
                Settings.ConsoleOptions = LoggingMessageType.None;
            }

            OutputTask = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    InputDone.Wait();

                    if (OutputQueue.Count <= 0)
                    {
                        OutputDone.Set();
                        await Task.Delay(1);
                        continue;
                    }

                    while (OutputQueue.Count > 0)
                    {
                        OutputContext context;
                        if (OutputQueue.TryDequeue(out context) == false)
                            continue;

                        // TODO: connect logging here

                        // Skip over stuff we can't display so we don't setress the output too much.
                        if (OutputQueue.Count > Console.BufferHeight)
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
                $"Output Encoding: {OutputEncoding.ToString()}".WriteLine();
                for (byte b = 0; b < byte.MaxValue; b++)
                {
                    char c = OutputEncoding.GetChars(new byte[] { b })[0];
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

        /// <summary>
        /// Gets or sets the output encoding for the current console.
        /// </summary>
        /// <value>
        /// The output encoding.
        /// </value>
        static public Encoding OutputEncoding
        {
            get { return Console.OutputEncoding; }
            set { Console.OutputEncoding = value; }
        }

        /// <summary>
        /// Gets or sets the cursor left position.
        /// </summary>
        /// <value>
        /// The cursor left.
        /// </value>
        static public int CursorLeft
        {
            get
            {

                OutputDone.Wait();
                InputDone.Reset();
                try { return Console.CursorLeft; } finally { InputDone.Set(); }
            }
            set
            {
                OutputDone.Wait();
                InputDone.Reset();
                try { Console.CursorLeft = value; } finally { InputDone.Set(); }
            }
        }

        /// <summary>
        /// Gets or sets the cursor top position.
        /// </summary>
        /// <value>
        /// The cursor top.
        /// </value>
        static public int CursorTop
        {
            get
            {

                OutputDone.Wait();
                InputDone.Reset();
                try { return Console.CursorTop; } finally { InputDone.Set(); }
            }
            set
            {
                OutputDone.Wait();
                InputDone.Reset();
                try { Console.CursorTop = value; } finally { InputDone.Set(); }
            }
        }

        /// <summary>
        /// Sets the cursor position.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="top">The top.</param>
        static public void SetCursorPosition(int left, int top)
        {
            OutputDone.Wait();
            InputDone.Reset();
            try { Console.SetCursorPosition(left, top); } finally { InputDone.Set(); }
        }

        /// <summary>
        /// Reads a key from the console.
        /// </summary>
        /// <param name="intercept">if set to <c>true</c> [intercept].</param>
        /// <returns></returns>
        static public ConsoleKeyInfo ReadKey(bool intercept)
        {
            OutputDone.Wait();
            InputDone.Reset();
            try { return Console.ReadKey(intercept); } finally { InputDone.Set(); }
        }

        /// <summary>
        /// Reads a line of thext from the console
        /// </summary>
        /// <returns></returns>
        static public string ReadLine()
        {
            OutputDone.Wait();
            InputDone.Reset();
            try { return Console.ReadLine(); } finally { InputDone.Set(); }
        }

    }
}