namespace Unosquare.Swan.Components
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;

    /// <summary>
    /// Represents logic providing several delay mechanisms.
    /// </summary>
    /// <example>
    ///  The following example shows how to implement delay mechanisms.
    /// <code>
    /// using Unosquare.Swan.Components;
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
        private readonly object _syncRoot = new object();
        private bool _isDisposed;
        private IWaitEvent _delayEvent;
        private readonly Stopwatch _delayStopwatch = new Stopwatch();

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
            /// Using the Thread.Sleep(1) mechanism.
            /// </summary>
            ThreadSleep,

            /// <summary>
            /// Using the Task.Delay(1).Wait mechanism.
            /// </summary>
            TaskDelay,

#if !UWP
            /// <summary>
            /// Using a wait event that completes in a background threadpool thread.
            /// </summary>
            ThreadPool,
#endif
        }

        /// <summary>
        /// Gets the selected delay strategy.
        /// </summary>
        public DelayStrategy Strategy { get; }

        /// <summary>
        /// Creates the smallest possible, synchronous delay based on the selected strategy.
        /// </summary>
        /// <returns>The elamped time of the delay.</returns>
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
#if !NETSTANDARD1_3 && !UWP
                    case DelayStrategy.ThreadPool:
                        DelayThreadPool();
                        break;
#endif
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

#if !NETSTANDARD1_3 && !UWP
        private void DelayThreadPool()
        {
            if (_delayEvent == null)
                _delayEvent = WaitEventFactory.Create(isCompleted: true, useSlim: true);

            _delayEvent.Begin();
            ThreadPool.QueueUserWorkItem((s) =>
            {
                DelaySleep();
                _delayEvent.Complete();
            });

            _delayEvent.Wait();
        }
#endif
        #endregion
    }
}