namespace Unosquare.Swan.Lite.Abstractions
{
    using System;
    using System.Threading;

    /// <summary>
    /// A threading <see cref="BackingTimer"/> implementation that executes at most one cycle at a time
    /// in a <see cref="ThreadPool"/> thread. Callback execution is NOT guaranteed to be carried out
    /// on the same <see cref="ThreadPool"/> thread every time the timer fires.
    /// </summary>
    public sealed class ExclusiveTimer
    {
        private readonly object SyncLock = new object();
        private readonly ManualResetEventSlim CycleDoneEvent = new ManualResetEventSlim(true);
        private readonly Timer BackingTimer;
        private readonly TimerCallback UserCallback;
        private readonly AtomicBoolean m_IsDisposing = new AtomicBoolean();
        private readonly AtomicBoolean m_IsDisposed = new AtomicBoolean();

        /// <summary>
        /// Initializes a new instance of the <see cref="ExclusiveTimer"/> class.
        /// </summary>
        /// <param name="timerCallback">The timer callback.</param>
        /// <param name="state">The state.</param>
        /// <param name="dueTime">The due time.</param>
        /// <param name="period">The period.</param>
        public ExclusiveTimer(TimerCallback timerCallback, object state, int dueTime, int period)
        {
            UserCallback = timerCallback;
            BackingTimer = new Timer(InternalCallback, state ?? this, dueTime, period);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExclusiveTimer"/> class.
        /// </summary>
        /// <param name="timerCallback">The timer callback.</param>
        /// <param name="state">The state.</param>
        /// <param name="dueTime">The due time.</param>
        /// <param name="period">The period.</param>
        public ExclusiveTimer(TimerCallback timerCallback, object state, TimeSpan dueTime, TimeSpan period)
            : this(timerCallback, state, Convert.ToInt32(dueTime.TotalMilliseconds), Convert.ToInt32(dueTime.TotalMilliseconds))
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
            // placholder
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExclusiveTimer"/> class.
        /// </summary>
        /// <param name="timerCallback">The timer callback.</param>
        /// <param name="dueTime">The due time.</param>
        /// <param name="period">The period.</param>
        public ExclusiveTimer(Action timerCallback, int dueTime, int period)
            : this(new TimerCallback((object s) => { timerCallback?.Invoke(); }), null, dueTime, period)
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
            : this(new TimerCallback((object s) => { timerCallback?.Invoke(); }), null, dueTime, period)
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
        public bool IsDisposing => m_IsDisposing.Value;

        /// <summary>
        /// Gets a value indicating whether this instance is disposed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is disposed; otherwise, <c>false</c>.
        /// </value>
        public bool IsDisposed => m_IsDisposed.Value;

        /// <summary>
        /// Changes the start time and the interval between method invocations for the internal timer.
        /// </summary>
        /// <param name="dueTime">The due time.</param>
        /// <param name="period">The period.</param>
        public void Change(int dueTime, int period) => BackingTimer.Change(dueTime, period);

        /// <summary>
        /// Changes the start time and the interval between method invocations for the internal timer.
        /// </summary>
        /// <param name="dueTime">The due time.</param>
        /// <param name="period">The period.</param>
        public void Change(TimeSpan dueTime, TimeSpan period) => BackingTimer.Change(dueTime, period);

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

        /// <summary>
        /// Releases resources held by this class.
        /// </summary>
        public void Dispose()
        {
            lock (SyncLock)
            {
                if (m_IsDisposed == true || m_IsDisposed == true)
                    return;

                m_IsDisposing.Value = true;
            }

            try
            {
                BackingTimer.Dispose();
                CycleDoneEvent.Wait();
                CycleDoneEvent.Dispose();
            }
            finally
            {
                m_IsDisposed.Value = true;
                m_IsDisposing.Value = false;
            }
        }

        /// <summary>
        /// Logic that runs every time the timer hits the due time.
        /// </summary>
        /// <param name="state">The state.</param>
        private void InternalCallback(object state)
        {
            lock (SyncLock)
            {
                if (IsDisposed || IsDisposing)
                    return;
            }

            if (CycleDoneEvent.IsSet == false)
                return;

            CycleDoneEvent.Reset();

            try
            {
                UserCallback(state);
            }
            finally
            {
                CycleDoneEvent.Set();
            }
        }
    }
}
