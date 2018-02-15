namespace Unosquare.Swan
{
    using System.Threading;

    /// <summary>
    /// Fast, atomic double combining interlocked to write value and volatile to read values
    /// Idea taken from Memory model and .NET operations in article:
    /// http://igoro.com/archive/volatile-keyword-in-c-memory-model-explained/
    /// </summary>
    public sealed class AtomicDouble
    {
        private double _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicDouble"/> class.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        public AtomicDouble(double initialValue = default) => Value = initialValue;

        /// <summary>
        /// Gets or sets the latest value written by any of the processors in the machine
        /// </summary>
        public double Value
        {
            get => Interlocked.CompareExchange(ref _value, 0d, 0d);
            set => Interlocked.Exchange(ref _value, value);
        }
    }
}
