using System;

namespace Swan.Threading
{
    /// <summary>
    /// Fast, atomic double combining interlocked to write value and volatile to read values.
    /// </summary>
    public sealed class AtomicDouble : AtomicTypeBase<double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicDouble"/> class.
        /// </summary>
        /// <param name="initialValue">if set to <c>true</c> [initial value].</param>
        public AtomicDouble(double initialValue = default)
            : base(BitConverter.DoubleToInt64Bits(initialValue))
        {
            // placeholder
        }

        /// <inheritdoc/>
        protected override double FromLong(long backingValue) =>
            BitConverter.Int64BitsToDouble(backingValue);

        /// <inheritdoc/>
        protected override long ToLong(double value) =>
            BitConverter.DoubleToInt64Bits(value);
    }
}