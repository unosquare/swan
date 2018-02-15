namespace Unosquare.Swan
{
    using System.Threading;

    /// <summary>
    /// Fast, atomic boolean combining interlocked to write value and volatile to read values
    /// Idea taken from Memory model and .NET operations in article:
    /// http://igoro.com/archive/volatile-keyword-in-c-memory-model-explained/
    /// </summary>
    public sealed class AtomicBoolean
    {
        private int _value;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicBoolean"/> class.
        /// </summary>
        /// <param name="initialValue">if set to <c>true</c> [initial value].</param>
        public AtomicBoolean(bool initialValue = default)
        {
            Value = initialValue;
        }

        /// <summary>
        /// Gets the latest value written by any of the processors in the machine
        /// Setting
        /// </summary>
        public bool Value
        {
            get => Interlocked.CompareExchange(ref _value, 0, 0) != 0;
            set => Interlocked.Exchange(ref _value, value ? 1 : 0);
        }
    }
}