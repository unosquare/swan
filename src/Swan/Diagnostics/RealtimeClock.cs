namespace Swan.Diagnostics
{
    using System;
    using System.Diagnostics;
    using Threading;

    /// <summary>
    /// A time measurement artifact.
    /// </summary>
    public sealed class RealTimeClock : IDisposable
    {
        private readonly Stopwatch _chrono = new Stopwatch();
        private ISyncLocker? _locker = SyncLockerFactory.Create(useSlim: true);
        private long _offsetTicks;
        private double _speedRatio = 1.0d;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="RealTimeClock"/> class.
        /// The clock starts paused and at the 0 position.
        /// </summary>
        public RealTimeClock()
        {
            Reset();
        }

        /// <summary>
        /// Gets or sets the clock position.
        /// </summary>
        public TimeSpan Position
        {
            get
            {
                using (_locker?.AcquireReaderLock())
                {
                    return TimeSpan.FromTicks(
                        _offsetTicks + Convert.ToInt64(_chrono.Elapsed.Ticks * SpeedRatio));
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the clock is running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                using (_locker?.AcquireReaderLock())
                {
                    return _chrono.IsRunning;
                }
            }
        }

        /// <summary>
        /// Gets or sets the speed ratio at which the clock runs.
        /// </summary>
        public double SpeedRatio
        {
            get
            {
                using (_locker?.AcquireReaderLock())
                {
                    return _speedRatio;
                }
            }
            set
            {
                using (_locker?.AcquireWriterLock())
                {
                    if (value < 0d) value = 0d;

                    // Capture the initial position se we set it even after the Speed Ratio has changed
                    // this ensures a smooth position transition
                    var initialPosition = Position;
                    _speedRatio = value;
                    Update(initialPosition);
                }
            }
        }

        /// <summary>
        /// Sets a new position value atomically.
        /// </summary>
        /// <param name="value">The new value that the position property will hold.</param>
        public void Update(TimeSpan value)
        {
            using (_locker?.AcquireWriterLock())
            {
                var resume = _chrono.IsRunning;
                _chrono.Reset();
                _offsetTicks = value.Ticks;
                if (resume) _chrono.Start();
            }
        }

        /// <summary>
        /// Starts or resumes the clock.
        /// </summary>
        public void Play()
        {
            using (_locker?.AcquireWriterLock())
            {
                if (_chrono.IsRunning) return;
                _chrono.Start();
            }
        }

        /// <summary>
        /// Pauses the clock.
        /// </summary>
        public void Pause()
        {
            using (_locker?.AcquireWriterLock())
            {
                _chrono.Stop();
            }
        }

        /// <summary>
        /// Sets the clock position to 0 and stops it.
        /// The speed ratio is not modified.
        /// </summary>
        public void Reset()
        {
            using (_locker?.AcquireWriterLock())
            {
                _offsetTicks = 0;
                _chrono.Reset();
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (_isDisposed) return;
            _isDisposed = true;
            _locker?.Dispose();
            _locker = null;
        }
    }
}
