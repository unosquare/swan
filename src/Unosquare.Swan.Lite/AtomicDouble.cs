﻿namespace Unosquare.Swan
{
    using System.Threading;

    /// <summary>
    /// Fast, atomioc double combining interlocked to write value and volatile to read values
    /// Idea taken from Memory model and .NET operations in article:
    /// http://igoro.com/archive/volatile-keyword-in-c-memory-model-explained/
    /// </summary>
    public sealed class AtomicDouble
    {
        private double _value = default(double);

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicDouble"/> class.
        /// </summary>
        public AtomicDouble()
        {
            Value = default(double);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AtomicDouble"/> class.
        /// </summary>
        /// <param name="initialValue">The initial value.</param>
        public AtomicDouble(double initialValue)
        {
            Value = initialValue;
        }

        /// <summary>
        /// Gets or sets the latest value written by any of the processors in the machine
        /// </summary>
        public double Value
        {
            get => Volatile.Read(ref _value);
            set => Interlocked.Exchange(ref _value, value);
        }
    }
}
