using System;
using System.Threading;

namespace Swan.Threading
{
    /// <summary>
    /// A threading <see cref="_backingTimer"/> implementation that executes at most one cycle at a time
    /// in a <see cref="ThreadPool"/> thread. Callback execution is NOT guaranteed to be carried out
    /// on the same <see cref="ThreadPool"/> thread every time the timer fires.
    /// </summary>
    public sealed class ExclusiveTimer : IDisposable
    {
        private readonly object _syncLock = new object();
        private readonly ManualResetEventSlim _cycleDoneEvent = new ManualResetEventSlim(true);
        private readonly Timer _backingTimer;
        private readonly TimerCallback _userCallback;
        private readonly AtomicBoolean _isDisposing = new AtomicBoolean();
        private readonly AtomicBoolean _isDisposed = new AtomicBoolean();
        private int _period;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExclusiveTimer"/> class.
        /// </summary>
        /// <param name="timerCallback">The timer callback.</param>
        /// <param name="state">The state.</param>
        /// <param name="dueTime">The due time.</param>
        /// <param name="period">The period.</param>
        public ExclusiveTimer(TimerCallback timerCallback, object? state, int dueTime, int period)
        {
            _period = period;
            _userCallback = timerCallback;
            _backingTimer = new Timer(InternalCallback, state ?? this, dueTime, Timeout.Infinite);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExclusiveTimer"/> class.
        /// </summary>
        /// <param name="timerCallback">The timer callback.</param>
        /// <param name="state">The state.</param>
        /// <param name="dueTime">The due time.</param>
        /// <param name="period">The period.</param>
        public ExclusiveTimer(TimerCallback timerCallback, object? state, TimeSpan dueTime, TimeSpan period)
            : this(timerCallback, state, Convert.ToInt32(dueTime.TotalMilliseconds), Convert.ToInt32(period.TotalMilliseconds))
        {
            // placeholder
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExclusiveTimer"/> class.
        /// </summary>
        /// <param name="timerCallback">The timer callback.</param>
        public ExclusiveTimer(TimerCallback timerCallback)
            : this(timerCallback, null, Timeout.Infinite, Timeout.Infinite)
        {
            // placeholder
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExclusiveTimer"/> class.
        /// </summary>
        /// <param name="timerCallback">The timer callback.</param>
        /// <param name="dueTime">The due time.</param>
        /// <param name="period">The period.</param>
        public ExclusiveTimer(Action timerCallback, int dueTime, int period)
            : this(s => { timerCallback?.Invoke(); }, null, dueTime, period)
        {
            // placeholder
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExclusiveTimer"/> class.
        /// </summary>
        /// <param name="timerCallback">The timer callback.</param>
        /// <param name="dueTime">The due time.</param>
        /// <param name="period">The period.</param>
        public ExclusiveTimer(Action timerCallback, TimeSpan dueTime, TimeSpan period)
            : this(s => { timerCallback?.Invoke(); }, null, dueTime, period)
        {
            // placeholder
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExclusiveTimer"/> class.
        /// </summary>
        /// <param name="timerCallback">The timer callback.</param>
        public ExclusiveTimer(Action timerCallback)
            : this(timerCallback, Timeout.Infinite, Timeout.Infinite)
        {
            // placeholder
        }

        /// <summary>
        /// Gets a value indicating whether this instance is disposing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is disposing; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposing => _isDisposing.Value;

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed => _isDisposed.Value;
        
        /// <summary>
        /// Waits until the time is elapsed.
        /// </summary>
        /// <param name="untilDate">The until date.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static void WaitUntil(DateTime untilDate, CancellationToken cancellationToken = default)
        {
            static void Callback(IWaitEvent waitEvent)
            {
                try
                {
                    waitEvent.Complete();
                    waitEvent.Begin();
                }
                catch
                {
                    // ignore
                }
            }

            using var delayLock = WaitEventFactory.Create(true);
            using var timer = new ExclusiveTimer(() => Callback(delayLock), 0, 15);
            while (!cancellationToken.IsCancellationRequested && DateTime.UtcNow < untilDate)
                delayLock.Wait();
        }

        /// <summary>
        /// Waits the specified wait time.
        /// </summary>
        /// <param name="waitTime">The wait time.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        public static void Wait(TimeSpan waitTime, CancellationToken cancellationToken = default) =>
            WaitUntil(DateTime.UtcNow.Add(waitTime), cancellationToken);

        /// <summary>
        /// Changes the start time and the interval between method invocations for the internal timer.
        /// </summary>
        /// <param name="dueTime">The due time.</param>
        /// <param name="period">The period.</param>
        public void Change(int dueTime, int period)
        {
            _period = period;

            _backingTimer.Change(dueTime, Timeout.Infinite);
        }

        /// <summary>
        /// Changes the start time and the interval between method invocations for the internal timer.
        /// </summary>
        /// <param name="dueTime">The due time.</param>
        /// <param name="period">The period.</param>
        public void Change(TimeSpan dueTime, TimeSpan period)
            => Change(Convert.ToInt32(dueTime.TotalMilliseconds), Convert.ToInt32(period.TotalMilliseconds));

        /// <summary>
        /// Changes the interval between method invocations for the internal timer.
        /// </summary>
        /// <param name="period">The period.</param>
        public void Resume(int period) => Change(0, period);

        /// <summary>
        /// Changes the interval between method invocations for the internal timer.
        /// </summary>
        /// <param name="period">The period.</param>
        public void Resume(TimeSpan period) => Change(TimeSpan.Zero, period);

        /// <summary>
        /// Pauses this instance.
        /// </summary>
        public void Pause() => Change(Timeout.Infinite, Timeout.Infinite);

        /// <inheritdoc />
        public void Dispose()
        {
            lock (_syncLock)
            {
                if (_isDisposed == true || _isDisposing == true)
                    return;

                _isDisposing.Value = true;
            }

            try
            {
                _cycleDoneEvent.Wait();
                _cycleDoneEvent.Dispose();
                Pause();
                _backingTimer.Dispose();
            }
            finally
            {
                _isDisposed.Value = true;
                _isDisposing.Value = false;
            }
        }

        /// <summary>
        /// Logic that runs every time the timer hits the due time.
        /// </summary>
        /// <param name="state">The state.</param>
        private void InternalCallback(object state)
        {
            lock (_syncLock)
            {
                if (IsDisposed || IsDisposing)
                    return;
            }

            if (_cycleDoneEvent.IsSet == false)
                return;

            _cycleDoneEvent.Reset();

            try
            {
                _userCallback(state);
            }
            finally
            {
                _cycleDoneEvent?.Set();
                _backingTimer?.Change(_period, Timeout.Infinite);
            }
        }
    }
}
