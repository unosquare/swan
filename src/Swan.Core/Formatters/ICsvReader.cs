namespace Swan.Formatters
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides properties and methods common to all CSV readers.
    /// </summary>
    /// <typeparam name="TLine">The type of record the reader outputs.</typeparam>
    public interface ICsvReader<out TLine> : IEnumerable<TLine>, IEnumerator<TLine>
    {
        /// <summary>
        /// Gets the current transformed record for schema-aware CSV readers and
        /// as populated by the last successful read operation. This property may return null when
        /// no read operation has been successfully completed.
        /// </summary>
        new TLine Current { get; }

        /// <summary>
        /// Gets the current record consisting of literals parsed from the internal stream reader
        /// as populated by the last successful read operation. This property may return null when
        /// no read operation has been successfully completed.
        /// </summary>
        IReadOnlyList<string?>? Values { get; }

        /// <summary>
        /// Gets a value that indicates whether the current stream position is at the end
        /// of the stream.
        /// </summary>
        bool EndOfStream { get; }

        /// <summary>
        /// The number of records that have been read so far, including
        /// headers and empty ones, but excluding calls to the <see cref="Skip"/>
        /// or <see cref="Skip"/> methods.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="IDisposable.Dispose()"/> method has been called.
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// Reads a set of literals parsed from the internal stream reader,
        /// sets the <see cref="Values"/> property to the result and increments
        /// the <see cref="Count"/> by one.
        /// </summary>
        /// <param name="trimValues">Determines if values should be trimmed.</param>
        /// <returns>True if the read was successful. False otherwise.</returns>
        bool MoveNext(bool trimValues);

        /// <summary>
        /// Parses a set of literals from the current positions of the underlying
        /// stream just as <see cref="MoveNext"/> but does not increment the <see cref="Count"/>
        /// and does not set <see cref="Values"/> property.
        /// </summary>
        /// <param name="skipCount">The number of records to skip.</param>
        void Skip(int skipCount = 1);

        /// <summary>
        /// Gets the string value of a given field index in the <see cref="Values"/> list.
        /// </summary>
        /// <param name="index">The field index in the current.</param>
        /// <param name="value">The output value.</param>
        /// <returns>True if the value was read successfully.</returns>
        bool TryGetValue(int index, out string value);
    }
}
