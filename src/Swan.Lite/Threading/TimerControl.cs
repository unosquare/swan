using System;
using System.Threading;

namespace Swan.Threading
{
    /// <summary>
    /// Use this singleton to wait for a specific <c>TimeSpan</c> or time.
    ///
    /// Internally this class will use a <c>Timer</c> and a <c>ManualResetEvent</c> to block until
    /// the time condition is satisfied.
    /// </summary>
    /// <seealso cref="SingletonBase{TimerControl}" />
    public class TimerControl : SingletonBase<TimerControl>
    {
        private readonly Timer _innerTimer;
        private readonly IWaitEvent _delayLock = WaitEventFactory.Create(true);

        /// <summary>
        /// Initializes a new instance of the <see cref="TimerControl"/> class.
        /// </summary>
        protected TimerControl()
        {
            _innerTimer = new Timer(
                x =>
                {
                    try
                    {
                        _delayLock.Complete();
                        _delayLock.Begin();
                    }
                    catch
                    {
                        // ignore
                    }
                },
                null,
                0,
                15);
        }

        /// <summary>
        /// Waits until the time is elapsed.
        /// </summary>
        /// <param name="untilDate">The until date.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public void WaitUntil(DateTime untilDate, CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested && DateTime.UtcNow < untilDate)
                _delayLock.Wait();
        }

        /// <summary>
        /// Waits the specified wait time.
        /// </summary>
        /// <param name="waitTime">The wait time.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public void Wait(TimeSpan waitTime, CancellationToken cancellationToken = default) =>
            WaitUntil(DateTime.UtcNow.Add(waitTime), cancellationToken);
    }
}