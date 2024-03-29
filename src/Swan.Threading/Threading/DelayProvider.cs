﻿namespace Swan.Threading;

using System.Diagnostics;

/// <summary>
/// Represents logic providing several delay mechanisms.
/// </summary>
/// <example>
///  The following example shows how to implement delay mechanisms.
/// <code>
/// using Swan.Threading;
/// 
/// public class Example
/// {
///     public static void Main()
///     {
///         // using the ThreadSleep strategy
///         using (var delay = new DelayProvider(DelayProvider.DelayStrategy.ThreadSleep))
///         {
///             // retrieve how much time was delayed
///             var time = delay.WaitOne();
///         }
///     }
/// }
/// </code>
/// </example>
public sealed class DelayProvider : IDisposable
{
    private readonly object _syncRoot = new();
    private readonly Stopwatch _delayStopwatch = new();

    private bool _isDisposed;
    private IWaitEvent? _delayEvent;

    /// <summary>
    /// Initializes a new instance of the <see cref="DelayProvider"/> class.
    /// </summary>
    /// <param name="strategy">The strategy.</param>
    public DelayProvider(DelayStrategy strategy = DelayStrategy.TaskDelay)
    {
        Strategy = strategy;
    }

    /// <summary>
    /// Enumerates the different ways of providing delays.
    /// </summary>
    public enum DelayStrategy
    {
        /// <summary>
        /// Using the Thread.Sleep(15) mechanism.
        /// </summary>
        ThreadSleep,

        /// <summary>
        /// Using the Task.Delay(1).Wait mechanism.
        /// </summary>
        TaskDelay,

        /// <summary>
        /// Using a wait event that completes in a background ThreadPool thread.
        /// </summary>
        ThreadPool,
    }

    /// <summary>
    /// Gets the selected delay strategy.
    /// </summary>
    public DelayStrategy Strategy { get; }

    /// <summary>
    /// Creates the smallest possible, synchronous delay based on the selected strategy.
    /// </summary>
    /// <returns>The elapsed time of the delay.</returns>
    public TimeSpan WaitOne()
    {
        lock (_syncRoot)
        {
            if (_isDisposed) return TimeSpan.Zero;

            _delayStopwatch.Restart();

            switch (Strategy)
            {
                case DelayStrategy.ThreadSleep:
                    DelaySleep();
                    break;
                case DelayStrategy.TaskDelay:
                    DelayTask();
                    break;
                case DelayStrategy.ThreadPool:
                    DelayThreadPool();
                    break;
            }

            return _delayStopwatch.Elapsed;
        }
    }

    #region Dispose Pattern

    /// <inheritdoc />
    public void Dispose()
    {
        lock (_syncRoot)
        {
            if (_isDisposed) return;
            _isDisposed = true;

            _delayEvent?.Dispose();
        }
    }

    #endregion

    #region Private Delay Mechanisms

    private static void DelaySleep() => Thread.Sleep(15);

    private static void DelayTask() => Task.Delay(1).Wait();

    private void DelayThreadPool()
    {
        _delayEvent ??= WaitEventFactory.Create(isCompleted: true, useSlim: true);

        _delayEvent.Begin();
        ThreadPool.QueueUserWorkItem(_ =>
        {
            DelaySleep();
            _delayEvent.Complete();
        });

        _delayEvent.Wait();
    }

    #endregion
}
