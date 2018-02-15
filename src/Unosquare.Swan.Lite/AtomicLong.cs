namespace Unosquare.Swan
{
    using System.Threading;

    /// <summary>
    /// Fast, atomioc long combining interlocked to write value and volatile to read values
    /// Idea taken from Memory model and .NET operations in article:
    /// http://igoro.com/archive/volatile-keyword-in-c-memory-model-explained/
    /// </summary>
    public sealed class AtomicLong
    {
        private long _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicLong" /> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public AtomicLong(long value = default) => Value = value;

        /// <summary>
        /// Gets or sets the latest value written by any of the processors in the machine
        /// </summary>
        public long Value
        {
            get => Interlocked.Read(ref _value);
            set => Interlocked.Exchange(ref _value, value);
        }

        /// <summary>
        /// Implements the operator ++.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static AtomicLong operator ++(AtomicLong instance)
        {
            instance.IncreaseOne();
            return instance;
        }

        /// <summary>
        /// Implements the operator --.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static AtomicLong operator --(AtomicLong instance)
        {
            instance.DecreaseOne();
            return instance;
        }

        internal void IncreaseOne() => Interlocked.Increment(ref _value);

        internal void DecreaseOne() => Interlocked.Decrement(ref _value);
    }
}
