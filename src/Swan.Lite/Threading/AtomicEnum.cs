using System;
using System.Threading;

namespace Swan.Threading
{
    /// <summary>
    /// Defines an atomic generic Enum.
    /// </summary>
    /// <typeparam name="T">The type of enum.</typeparam>
   public sealed class AtomicEnum<T>
        where T : struct, IConvertible
    {
        private long _backingValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicEnum{T}"/> class.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        /// <exception cref="ArgumentException">T must be an enumerated type.</exception>
        public AtomicEnum(T initialValue)
        {
            if (!Enum.IsDefined(typeof(T), initialValue))
                throw new ArgumentException("T must be an enumerated type");

            Value = initialValue;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public T Value
        {
            get => (T)Enum.ToObject(typeof(T), BackingValue);
            set => BackingValue = Convert.ToInt64(value);
        }

        private long BackingValue
        {
            get => Interlocked.Read(ref _backingValue);
            set => Interlocked.Exchange(ref _backingValue, value);
        }
    }
}