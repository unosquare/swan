namespace Swan.Threading
{
    /// <summary>
    /// Fast, atomic long combining interlocked to write value and volatile to read values.
    /// </summary>
    public sealed class AtomicLong : AtomicTypeBase<long>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicLong"/> class.
        /// </summary>
        /// <param name="initialValue">if set to <c>true</c> [initial value].</param>
        public AtomicLong(long initialValue = default)
            : base(initialValue)
        {
            // placeholder
        }

        /// <inheritdoc />
        protected override long FromLong(long backingValue) => backingValue;

        /// <inheritdoc />
        protected override long ToLong(long value) => value;
    }
}
