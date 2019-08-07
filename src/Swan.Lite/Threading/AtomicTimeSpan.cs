using System;

namespace Swan.Threading
{
    /// <summary>
    /// Represents an atomic TimeSpan type.
    /// </summary>
    public sealed class AtomicTimeSpan : AtomicTypeBase<TimeSpan>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicTimeSpan" /> class.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        public AtomicTimeSpan(TimeSpan initialValue)
            : base(initialValue.Ticks)
        {
            // placeholder
        }

        /// <inheritdoc />
        protected override TimeSpan FromLong(long backingValue) => TimeSpan.FromTicks(backingValue);

        /// <inheritdoc />
        protected override long ToLong(TimeSpan value) => value.Ticks < 0 ? 0 : value.Ticks;
    }
}
