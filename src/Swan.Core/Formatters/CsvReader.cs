namespace Swan.Formatters
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Represents a text reader, typically a comma-separated set of values
    /// that can be configured to read an parse tabular data with flexible
    /// encoding, field separators and escape characters.
    /// </summary>
    public class CsvReader : CsvReaderBase<IReadOnlyList<string>>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CsvReader"/> class.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="separatorChar">The field separator character.</param>
        /// <param name="escapeChar">The escape character.</param>
        /// <param name="leaveOpen">true to leave the stream open after the stream reader object is disposed; otherwise, false.</param>
        /// <param name="trimsValues">True to trim field values as they are read and parsed.</param>
        public CsvReader(Stream stream,
            Encoding? encoding = default,
            char separatorChar = Csv.DefaultSeparatorChar,
            char escapeChar = Csv.DefaultEscapeChar,
            bool leaveOpen = default,
            bool trimsValues = true)
        : base(stream, encoding, separatorChar, escapeChar, leaveOpen, trimsValues)
        {
        }

        /// <inheritdoc />
        public override IReadOnlyList<string> Current =>
            Values ?? throw new InvalidOperationException("Reader is not in a valid state.");
    }
}
