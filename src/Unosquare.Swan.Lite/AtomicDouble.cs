﻿namespace Unosquare.Swan
{
    using System;
    using System.Threading;

    /// <summary>
    /// Fast, atomic double combining interlocked to write value and volatile to read values
    /// Idea taken from Memory model and .NET operations in article:
    /// http://igoro.com/archive/volatile-keyword-in-c-memory-model-explained/
    /// </summary>
    public sealed class AtomicDouble
    {
        private long _value;

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
            get => BitConverter.Int64BitsToDouble(Interlocked.Read(ref _value));
            set => Interlocked.Exchange(ref _value, BitConverter.DoubleToInt64Bits(value));
        }

        /// <summary>
        /// Implements the operator ++.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static AtomicDouble operator ++(AtomicDouble instance)
        {
            instance.Value++;
            return instance;
        }

        /// <summary>
        /// Implements the operator --.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static AtomicDouble operator --(AtomicDouble instance)
        {
            instance.Value--;
            return instance;
        }
    }
}
