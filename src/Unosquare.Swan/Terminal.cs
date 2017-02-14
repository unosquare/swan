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
    public static partial class Terminal
    {
        #region Private Declarations

        private static readonly object SyncLock = new object();
        private static readonly ConcurrentQueue<OutputContext> OutputQueue = new ConcurrentQueue<OutputContext>();

        private static readonly ManualResetEventSlim OutputDone = new ManualResetEventSlim(false);
        private static readonly ManualResetEventSlim InputDone = new ManualResetEventSlim(true);

        private static bool? m_IsConsolePresent;

        private static readonly Task DequeueOutputTask;

        #endregion

        #region Output Context

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
                OutputWriters = IsConsolePresent ? TerminalWriters.StandardOutput :
                    IsDebuggerAttached ? TerminalWriters.Diagnostics
                        : TerminalWriters.None;
            }

            public ConsoleColor OriginalColor { get; }
            public ConsoleColor OutputColor { get; set; }
            public char[] OutputText { get; set; }
            public TerminalWriters OutputWriters { get; set; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the <see cref="Terminal"/> class.
        /// </summary>
        static Terminal()
        {
            lock (SyncLock)
            {
                if (DequeueOutputTask != null)
                    return;

                if (IsDebuggerAttached)
                {
                    Settings.DisplayLoggingMessageType =
                        LogMessageType.Debug |
                        LogMessageType.Error |
                        LogMessageType.Info |
                        LogMessageType.Trace |
                        LogMessageType.Warning;
                }
                else
                {
                    Settings.DisplayLoggingMessageType =
                        LogMessageType.Error |
                        LogMessageType.Info |
                        LogMessageType.Warning;
                }

                if (IsConsolePresent)
                {
#if !NET452
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
#endif
                    Console.CursorVisible = false;
                }
                    

                // Here we start the output task, fire-and-forget
                DequeueOutputTask = DequeueOutputAsync();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Enqueues the output to be written to the console
        /// This is the only method that should enqueue to the output
        /// Please note that if AvailableWriters is None, then no output will be enqueued
        /// </summary>
        /// <param name="context">The context.</param>
        private static void EnqueueOutput(OutputContext context)
        {
            lock (SyncLock)
            {
                var availableWriters = AvailableWriters;

                if (availableWriters == TerminalWriters.None || context.OutputWriters == TerminalWriters.None)
                {
                    OutputDone.Set();
                    return;
                }

                if ((context.OutputWriters & availableWriters) == TerminalWriters.None)
                    return;

                OutputDone.Reset();
                OutputQueue.Enqueue(context);
            }
        }

        /// <summary>
        /// Dequeues the output asynchronously.
        /// </summary>
        /// <returns></returns>
        private static async Task DequeueOutputAsync()
        {
            if (AvailableWriters == TerminalWriters.None)
            {
                OutputDone.Set();
                return;
            }

            while (true)
            {
                InputDone.Wait();

                if (OutputQueue.Count <= 0)
                {
                    OutputDone.Set();
                    await Task.Delay(1);
                    continue;
                }
                else
                {
                    OutputDone.Reset();
                }

                while (OutputQueue.Count > 0)
                {
                    OutputContext context;
                    if (OutputQueue.TryDequeue(out context) == false)
                        continue;

                    // Process Console output and Skip over stuff we can't display so we don't stress the output too much.
                    if (IsConsolePresent && OutputQueue.Count <= Console.BufferHeight)
                    {
                        // Output to the sandard output
                        if (context.OutputWriters.HasFlag(TerminalWriters.StandardOutput))
                        {
                            Console.ForegroundColor = context.OutputColor;
                            Console.Out.Write(context.OutputText);
                            Console.ResetColor();
                            Console.ForegroundColor = context.OriginalColor;
                        }

                        // output to the standard error
                        if (context.OutputWriters.HasFlag(TerminalWriters.StandardError))
                        {
                            Console.ForegroundColor = context.OutputColor;
                            Console.Error.Write(context.OutputText);
                            Console.ResetColor();
                            Console.ForegroundColor = context.OriginalColor;
                        }
                    }

                    // Process Debugger output
                    if (IsDebuggerAttached && context.OutputWriters.HasFlag(TerminalWriters.Diagnostics))
                    {
                        System.Diagnostics.Debug.Write(new string(context.OutputText));
                    }
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }

        /// <summary>
        /// Waits for all of the queued output messages to be written out to the console.
        /// Call this method if it is important to display console text before
        /// quitting the application such as showing usage or help.
        /// Set the timeout to null or TimeSpan.Zero to wait indefinitely.
        /// </summary>
        /// <param name="timeout">The timeout. Set the amount of time to black before this method exits.</param>
        public static void Flush(TimeSpan? timeout = null)
        {
            if (OutputDone.IsSet) return;
            if (timeout == null) timeout = TimeSpan.Zero;
            var startTime = DateTime.UtcNow;

            while (true)
            {
                if (OutputDone.Wait(1))
                    break;

                if (timeout.Value == TimeSpan.Zero)
                    continue;

                if (DateTime.UtcNow.Subtract(startTime) >= timeout.Value)
                    break;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the Console is present
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is console present; otherwise, <c>false</c>.
        /// </value>
        public static bool IsConsolePresent
        {
            get
            {
                if (m_IsConsolePresent == null)
                {
                    m_IsConsolePresent = true;
                    try
                    {
                        var windowHeight = Console.WindowHeight;
                        m_IsConsolePresent = windowHeight >= 0;
                    }
                    catch { m_IsConsolePresent = false; }
                }

                return m_IsConsolePresent.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether a debugger is attached.
        /// </summary>
        public static bool IsDebuggerAttached => System.Diagnostics.Debugger.IsAttached;

        /// <summary>
        /// Gets the available output writers in a bitwise mask.
        /// </summary>
        public static TerminalWriters AvailableWriters
        {
            get
            {
                var writers = TerminalWriters.None;
                if (IsConsolePresent)
                    writers = TerminalWriters.StandardError | TerminalWriters.StandardOutput;

                if (IsDebuggerAttached)
                    writers = writers | TerminalWriters.Diagnostics;

                return writers;
            }
        }

        /// <summary>
        /// Gets or sets the output encoding for the current console.
        /// </summary>
        /// <value>
        /// The output encoding.
        /// </value>
        public static Encoding OutputEncoding
        {
            get { return Console.OutputEncoding; }
            set { Console.OutputEncoding = value; }
        }

        #endregion

        #region Synchronized Cursor Movement

        /// <summary>
        /// Gets or sets the cursor left position.
        /// </summary>
        /// <value>
        /// The cursor left.
        /// </value>
        public static int CursorLeft
        {
            get
            {
                if (IsConsolePresent == false) return -1;
                lock (SyncLock)
                {
                    Flush();
                    return Console.CursorLeft;
                }

            }
            set
            {
                if (IsConsolePresent == false) return;
                lock (SyncLock)
                {
                    Flush();
                    Console.CursorLeft = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the cursor top position.
        /// </summary>
        /// <value>
        /// The cursor top.
        /// </value>
        public static int CursorTop
        {
            get
            {
                if (IsConsolePresent == false) return -1;
                lock (SyncLock)
                {
                    Flush();
                    return Console.CursorTop;
                }
            }
            set
            {
                if (IsConsolePresent == false) return;

                lock (SyncLock)
                {
                    Flush();
                    Console.CursorTop = value;
                }
            }
        }

        /// <summary>
        /// Sets the cursor position.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="top">The top.</param>
        public static void SetCursorPosition(int left, int top)
        {
            if (IsConsolePresent == false) return;

            if (left < 0) left = 0;
            if (top < 0) top = 0;

            lock (SyncLock)
            {
                Flush();
                Console.SetCursorPosition(left, top);
            }
        }

        /// <summary>
        /// Moves the output cursor one line up starting at left position 0
        /// Please note that backlining the cursor does not clear the contents of the 
        /// previous line so you might need to clear it by writing an empty string the 
        /// length of the console width
        /// </summary>
        public static void BacklineCursor()
        {
            SetCursorPosition(0, CursorTop - 1);
        }

        #endregion

    }
}