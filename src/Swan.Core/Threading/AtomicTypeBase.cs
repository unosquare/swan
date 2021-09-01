using System;
using System.Threading;

namespace Swan.Threading
{
    /// <summary>
    /// Provides a generic implementation of an Atomic (interlocked) type
    /// 
    /// Idea taken from Memory model and .NET operations in article:
    /// http://igoro.com/archive/volatile-keyword-in-c-memory-model-explained/.
    /// </summary>
    /// <typeparam name="T">The structure type backed by a 64-bit value.</typeparam>
    public abstract class AtomicTypeBase<T> : IComparable, IComparable<T>, IComparable<AtomicTypeBase<T>>, IEquatable<T>, IEquatable<AtomicTypeBase<T>>
        where T : struct, IComparable, IComparable<T>, IEquatable<T>
    {
        private long _backingValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicTypeBase{T}"/> class.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        protected AtomicTypeBase(long initialValue)
        {
            BackingValue = initialValue;
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public T Value
        {
            get => FromLong(BackingValue);
            set => BackingValue = ToLong(value);
        }

        /// <summary>
        /// Gets or sets the backing value.
        /// </summary>
        protected long BackingValue
        {
            get => Interlocked.Read(ref _backingValue);
            set => Interlocked.Exchange(ref _backingValue, value);
        }

        /// <summary>
        /// Implicit conversion operator.
        /// </summary>
        /// <param name="atomic">The atomic object containing the value.</param>
        public static implicit operator T(AtomicTypeBase<T> atomic) => atomic?.Value ?? default;

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(AtomicTypeBase<T>? a, T b) => a is null
            ? b.Equals(default)
            : b.Equals(a.Value);

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(AtomicTypeBase<T>? a, T b) => a is null
            ? !b.Equals(default)
            : !b.Equals(a.Value);

        /// <summary>
        /// Implements the operator &gt;.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator >(AtomicTypeBase<T>? a, T b) => a is null
            ? default(T).CompareTo(b) > 0
            : a.CompareTo(b) > 0;

        /// <summary>
        /// Implements the operator &lt;.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator <(AtomicTypeBase<T>? a, T b) => a is null
            ? default(T).CompareTo(b) < 0
            : a.CompareTo(b) < 0;

        /// <summary>
        /// Implements the operator &gt;=.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator >=(AtomicTypeBase<T>? a, T b) => a is null
            ? default(T).CompareTo(b) >= 0
            : a.CompareTo(b) >= 0;

        /// <summary>
        /// Implements the operator &lt;=.
        /// </summary>
        /// <param name="a">a.</param>
        /// <param name="b">The b.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator <=(AtomicTypeBase<T>? a, T b) => a is null
            ? default(T).CompareTo(b) <= 0
            : a.CompareTo(b) <= 0;

        /// <summary>
        /// Increments the value by one.
        /// </summary>
        /// <returns>The new value incremented by one.</returns>
        public virtual T Increment() => FromLong(Interlocked.Increment(ref _backingValue));

        /// <summary>
        /// Decrements the value by one.
        /// </summary>
        /// <returns>The new value decremented by one.</returns>
        public virtual T Decrement() => FromLong(Interlocked.Decrement(ref _backingValue));

        /// <summary>
        /// Compares the value to the other instance.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns>0 if equal, 1 if this instance is greater, -1 if this instance is less than.</returns>
        /// <exception cref="ArgumentException">When types are incompatible.</exception>
        public int CompareTo(object? other)
        {
            return other switch
            {
                null => 1,
                AtomicTypeBase<T> atomic => BackingValue.CompareTo(atomic.BackingValue),
                T variable => Value.CompareTo(variable),
                _ => throw new ArgumentException("Incompatible comparison types"),
            };
        }

        /// <summary>
        /// Compares the value to the other instance.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns>0 if equal, 1 if this instance is greater, -1 if this instance is less than.</returns>
        public int CompareTo(T other) => Value.CompareTo(other);

        /// <summary>
        /// Compares the value to the other instance.
        /// </summary>
        /// <param name="other">The other instance.</param>
        /// <returns>0 if equal, 1 if this instance is greater, -1 if this instance is less than.</returns>
        public int CompareTo(AtomicTypeBase<T>? other) => BackingValue.CompareTo(other?.BackingValue ?? default);

        /// <summary>
        /// Determines whether the specified <see cref="object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return obj switch
            {
                AtomicTypeBase<T> atomic => Equals(atomic),
                T variable => Equals(variable),
                _ => false,
            };
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.
        /// </returns>
        public override int GetHashCode() => BackingValue.GetHashCode();

        /// <inheritdoc />
        public bool Equals(AtomicTypeBase<T>? other) =>
            BackingValue == (other?.BackingValue ?? default);

        /// <inheritdoc />
        public bool Equals(T other) => Equals(Value, other);

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{Value}";
        }

        /// <summary>
        /// Converts from a long value to the target type.
        /// </summary>
        /// <param name="backingValue">The backing value.</param>
        /// <returns>The value converted form a long value.</returns>
        protected abstract T FromLong(long backingValue);

        /// <summary>
        /// Converts from the target type to a long value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The value converted to a long value.</returns>
        protected abstract long ToLong(T value);
    }
}