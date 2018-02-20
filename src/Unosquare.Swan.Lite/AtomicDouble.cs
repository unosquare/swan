namespace Unosquare.Swan
{
    using System;
    using Unosquare.Swan.Lite.Abstractions;

    /// <summary>
    /// Fast, atomic double combining interlocked to write value and volatile to read values
    /// Idea taken from Memory model and .NET operations in article:
    /// http://igoro.com/archive/volatile-keyword-in-c-memory-model-explained/
    /// </summary>
    public sealed class AtomicDouble : AtomicTypeBase<double>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicDouble"/> class.
        /// </summary>
        /// <param name="initialValue">if set to <c>true</c> [initial value].</param>
        public AtomicDouble(double initialValue)
            : base(BitConverter.DoubleToInt64Bits(initialValue))
        {
            // placeholder
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicDouble"/> class.
        /// </summary>
        public AtomicDouble()
            : base(BitConverter.DoubleToInt64Bits(0))
        {
            // placeholder
        }

        /// <summary>
        /// Converts from a long value to the target type.
        /// </summary>
        /// <param name="backingValue">The backing value.</param>
        /// <returns>
        /// The value converted form a long value
        /// </returns>
        protected override double FromLong(long backingValue)
        {
            return BitConverter.Int64BitsToDouble(backingValue);
        }

        /// <summary>
        /// Converts from the target type to a long value
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The value converted to a long value
        /// </returns>
        protected override long ToLong(double value)
        {
            return BitConverter.DoubleToInt64Bits(value);
        }
    }
}
