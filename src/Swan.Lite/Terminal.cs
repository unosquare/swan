using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using Swan.Threading;

namespace Swan
{
    /// <summary>
    /// A console terminal helper to create nicer output and receive input from the user. 
    /// This class is thread-safe :).
    /// </summary>
    public static partial class Terminal
    {
        #region Private Declarations

        private const int OutputFlushInterval = 15;
        private static readonly ExclusiveTimer DequeueOutputTimer;
        private static readonly object SyncLock = new object();
        private static readonly ConcurrentQueue<OutputContext> OutputQueue = new ConcurrentQueue<OutputContext>();

        private static readonly ManualResetEventSlim OutputDone = new ManualResetEventSlim(false);
        private static readonly ManualResetEventSlim InputDone = new ManualResetEventSlim(true);

        private static bool? _isConsolePresent;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes static members of the <see cref="Terminal"/> class.
        /// </summary>
        static Terminal()
        {
            lock (SyncLock)
            {
                if (DequeueOutputTimer != null) return;

                if (IsConsolePresent)
                {
                    Console.CursorVisible = false;
                }

                // Here we start the output task, fire-and-forget
                DequeueOutputTimer = new ExclusiveTimer(DequeueOutputCycle);
                DequeueOutputTimer.Resume(OutputFlushInterval);
            }
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

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the Console is present.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is console present; otherwise, <c>false</c>.
        /// </value>
        public static bool IsConsolePresent
        {
            get
            {
                if (_isConsolePresent == null)
                {
                    _isConsolePresent = true;
                    try
                    {
                        var windowHeight = Console.WindowHeight;
                        _isConsolePresent = windowHeight >= 0;
                    }
                    catch
                    {
                        _isConsolePresent = false;
                    }
                }

                return _isConsolePresent.Value;
            }
        }

        /// <summary>
        /// Gets the available output writers in a bitwise mask.
        /// </summary>
        /// <value>
        /// The available writers.
        /// </value>
        public static TerminalWriters AvailableWriters =>
            IsConsolePresent
                ? TerminalWriters.StandardError | TerminalWriters.StandardOutput
                : TerminalWriters.None;

        /// <summary>
        /// Gets or sets the output encoding for the current console.
        /// </summary>
        /// <value>
        /// The output encoding.
        /// </value>
        public static Encoding OutputEncoding
        {
            get => Console.OutputEncoding;
            set => Console.OutputEncoding = value;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Waits for all of the queued output messages to be written out to the console.
        /// Call this method if it is important to display console text before
        /// quitting the application such as showing usage or help.
        /// Set the timeout to null or TimeSpan.Zero to wait indefinitely.
        /// </summary>
        /// <param name="timeout">The timeout. Set the amount of time to black before this method exits.</param>
        public static void Flush(TimeSpan? timeout = null)
        {
            if (timeout == null) timeout = TimeSpan.Zero;
            var startTime = DateTime.UtcNow;

            while (OutputQueue.Count > 0)
            {
                // Manually trigger a timer cycle to run immediately
                DequeueOutputTimer.Change(0, OutputFlushInterval);

                // Wait for the output to finish
                if (OutputDone.Wait(OutputFlushInterval))
                    break;

                // infinite timeout
                if (timeout.Value == TimeSpan.Zero)
                    continue;

                // break if we have reached a timeout condition
                if (DateTime.UtcNow.Subtract(startTime) >= timeout.Value)
                    break;
            }
        }

        /// <summary>
        /// Sets the cursor position.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="top">The top.</param>
        public static void SetCursorPosition(int left, int top)
        {
            if (!IsConsolePresent) return;

            lock (SyncLock)
            {
                Flush();
                Console.SetCursorPosition(left.Clamp(0, left), top.Clamp(0, top));
            }
        }

        /// <summary>
        /// Moves the output cursor one line up starting at left position 0
        /// Please note that backlining the cursor does not clear the contents of the 
        /// previous line so you might need to clear it by writing an empty string the 
        /// length of the console width.
        /// </summary>
        public static void BacklineCursor() => SetCursorPosition(0, CursorTop - 1);

        /// <summary>
        /// Writes a standard banner to the standard output
        /// containing the company name, product name, assembly version and trademark.
        /// </summary>
        /// <param name="color">The color.</param>
        public static void WriteWelcomeBanner(ConsoleColor color = ConsoleColor.Gray)
        {
            WriteLine($"{SwanRuntime.CompanyName} {SwanRuntime.ProductName} [Version {SwanRuntime.EntryAssemblyVersion}]", color);
            WriteLine($"{SwanRuntime.ProductTrademark}", color);
        }

        /// <summary>
        /// Enqueues the output to be written to the console
        /// This is the only method that should enqueue to the output
        /// Please note that if AvailableWriters is None, then no output will be enqueued.
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
        /// Runs a Terminal I/O cycle in the <see cref="ThreadPool"/> thread.
        /// </summary>
        private static void DequeueOutputCycle()
        {
            if (AvailableWriters == TerminalWriters.None)
            {
                OutputDone.Set();
                return;
            }

            InputDone.Wait();

            if (OutputQueue.Count <= 0)
            {
                OutputDone.Set();
                return;
            }

            OutputDone.Reset();

            while (OutputQueue.Count > 0)
            {
                if (!OutputQueue.TryDequeue(out var context)) continue;

                // Process Console output and Skip over stuff we can't display so we don't stress the output too much.
                if (!IsConsolePresent) continue;

                Console.ForegroundColor = context.OutputColor;

                // Output to the standard output
                if (context.OutputWriters.HasFlag(TerminalWriters.StandardOutput))
                {
                    Console.Out.Write(context.OutputText);
                }

                // output to the standard error
                if (context.OutputWriters.HasFlag(TerminalWriters.StandardError))
                {
                    Console.Error.Write(context.OutputText);
                }

                Console.ResetColor();
                Console.ForegroundColor = context.OriginalColor;
            }
        }

        #endregion

        #region Output Context

        /// <summary>
        /// Represents an asynchronous output context.
        /// </summary>
        private sealed class OutputContext
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="OutputContext"/> class.
            /// </summary>
            public OutputContext()
            {
                OriginalColor = Settings.DefaultColor;
                OutputWriters = IsConsolePresent
                    ? TerminalWriters.StandardOutput
                    : TerminalWriters.None;
            }

            public ConsoleColor OriginalColor { get; }
            public ConsoleColor OutputColor { get; set; }
            public char[] OutputText { get; set; }
            public TerminalWriters OutputWriters { get; set; }
        }

        #endregion
    }
}
