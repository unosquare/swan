namespace Swan.Platform;

using Extensions;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using Threading;

/// <summary>
/// A console terminal helper to create nicer output and receive input from the user. 
/// This class is thread-safe :).
/// </summary>
public static partial class Terminal
{
    #region Private Declarations

    private const int OutputFlushInterval = 15;
    private static readonly ExclusiveTimer DequeueOutputTimer;
    private static readonly object SyncLock = new();
    private static readonly ConcurrentQueue<OutputContext> OutputQueue = new();

    private static readonly ManualResetEventSlim OutputDone = new(false);
    private static readonly ManualResetEventSlim InputDone = new(true);

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
            DequeueOutputTimer = new(DequeueOutputCycle);
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
    public static TerminalWriterFlags AvailableWriters =>
        IsConsolePresent
            ? TerminalWriterFlags.StandardError | TerminalWriterFlags.StandardOutput
            : TerminalWriterFlags.None;

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
        timeout ??= TimeSpan.Zero;
        var startTime = DateTime.UtcNow;

        while (!OutputQueue.IsEmpty)
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
    /// Sets the cursor position withing the console buffer.
    /// </summary>
    /// <param name="left">The left.</param>
    /// <param name="top">The top.</param>
    public static void SetCursorPosition(int left, int top)
    {
        if (!IsConsolePresent) return;

        lock (SyncLock)
        {
            Flush();
            Console.SetCursorPosition(left.Clamp(0, Console.BufferWidth - 1), top.Clamp(0, Console.BufferHeight - 1));
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

            if (availableWriters == TerminalWriterFlags.None || context.OutputWriters == TerminalWriterFlags.None)
            {
                OutputDone.Set();
                return;
            }

            if ((context.OutputWriters & availableWriters) == TerminalWriterFlags.None)
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
        if (AvailableWriters == TerminalWriterFlags.None)
        {
            OutputDone.Set();
            return;
        }

        InputDone.Wait();

        if (OutputQueue.IsEmpty)
        {
            OutputDone.Set();
            return;
        }

        OutputDone.Reset();

        while (!OutputQueue.IsEmpty)
        {
            if (!OutputQueue.TryDequeue(out var context)) continue;

            // Process Console output and Skip over stuff we can't display so we don't stress the output too much.
            if (!IsConsolePresent) continue;

            Console.ForegroundColor = context.OutputColor;
            var buffer = OutputEncoding == Encoding.Default
                ? context.OutputText.ToCharArray().AsSpan()
                : OutputEncoding.GetChars(OutputEncoding.GetBytes(context.OutputText));

            // Output to the standard output
            if (context.OutputWriters.HasFlag(TerminalWriterFlags.StandardOutput))
                Console.Out.Write(buffer);

            // output to the standard error
            if (context.OutputWriters.HasFlag(TerminalWriterFlags.StandardError))
                Console.Error.Write(buffer);

            Console.ResetColor();
            Console.ForegroundColor = context.OriginalColor;
        }
    }

    #endregion

    #region Output Context

    /// <summary>
    /// Represents an asynchronous output context.
    /// </summary>
    private sealed record OutputContext(
        string OutputText,
        TerminalWriterFlags OutputWriters,
        ConsoleColor OutputColor,
        ConsoleColor OriginalColor)
    {
        // placeholder
    }

    #endregion
}
