using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Swan.Formatters
{
    /// <summary>
    /// PRovides a base class for reading schema aware <see cref="CsvReader"/> streams
    /// where headings are used to map and transform specific target members.
    /// </summary>
    public abstract class CsvRecordReader<TReader> : CsvReader
        where TReader : CsvReader
    {
        private readonly Dictionary<string, int> _Headings = new(64);

        /// <summary>
        /// Creates a new instance of the <see cref="CsvRecordReader{T}"/> class.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="separatorChar">The field separator character.</param>
        /// <param name="escapeChar">The escape character.</param>
        /// <param name="leaveOpen">true to leave the stream open after the System.IO.StreamReader object is disposed; otherwise, false.</param>
        protected CsvRecordReader(Stream stream,
            Encoding? encoding = default,
            char separatorChar = DefaultSeparatorChar,
            char escapeChar = DefaultEscapeChar,
            bool leaveOpen = default)
            : base(stream, encoding, separatorChar, escapeChar, leaveOpen)
        {
            // placeholder
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CsvRecordReader{T}"/> class.
        /// </summary>
        /// <param name="path">The file to read from.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="separatorChar">The field separator character.</param>
        /// <param name="escapeChar">The escape character.</param>
        protected CsvRecordReader(string path,
            Encoding? encoding = default,
            char separatorChar = DefaultSeparatorChar,
            char escapeChar = DefaultEscapeChar)
            : base(File.OpenRead(path), encoding, separatorChar, escapeChar, false)
        {
            // placeholder
        }

        /// <summary>
        /// Gets a dictionary of hadings and their corresponding indices. This property
        /// is valid only after successfully calling the <see cref="ReadHeadings"/>
        /// method.
        /// </summary>
        public IReadOnlyDictionary<string, int> Headings => _Headings;

        /// <summary>
        /// Gets the index of the given heading name.
        /// </summary>
        /// <param name="heading">The name of the heading.</param>
        /// <returns>Returns a valid field index or -1 for invalid or not found.</returns>
        public virtual int IndexOf(string heading)
        {
            RequireHeadings();

            if (!Headings.TryGetValue(heading, out var index))
                return -1;

            return index;
        }

        /// <summary>
        /// Gets the string value of a given field name.
        /// </summary>
        /// <param name="heading">The field name (heading) to retrieve the data from.</param>
        /// <param name="value">The return value.</param>
        /// <returns></returns>
        public virtual bool TryGetValue(string heading, out string value) =>
            TryGetValue(IndexOf(heading), out value);

        /// <summary>
        /// Reads from the underlying stream ingests the parsed set of literals as headings
        /// or column names to be used as a map to read the file.
        /// </summary>
        /// <param name="trimValues">Determines if values should be trimmed.</param>
        /// <returns>This instance for fluent API enablement.</returns>
        public TReader ReadHeadings(bool trimValues = true)
        {
            if (Headings.Any())
                throw new InvalidOperationException($"The {nameof(Headings)} have already been set.");

            if (Count != 0)
                throw new InvalidOperationException("Reading headings is only supported as the first read operation.");

            var values = base.Read(trimValues);
            return SetHeadings(values.ToArray());
        }

        /// <summary>
        /// When the underlying stream has no headings, call this method for the
        /// current reader to become schema-aware.
        /// </summary>
        /// <param name="headings">The heading names.</param>
        /// <returns>This instance for fluent API enablement.</returns>
        public TReader SetHeadings(params string[] headings)
        {
            if (Headings.Any())
                throw new InvalidOperationException($"The {nameof(Headings)} have already been set.");

            if (headings is null || headings.Length <= 0)
                throw new ArgumentException($"Headings must contain at least one element.", nameof(headings));

            for (var i = 0; i < headings.Length; i++)
                _Headings[headings[i]] = i;

            OnHeadingsRead();
            return (this as TReader)!;
        }

        /// <summary>
        /// Parses a set of literals from the current positions of the underlying
        /// stream just as <see cref="CsvReader.ReadAsync"/> but does not increment the <see cref="CsvReader.Count"/>
        /// and does not set <see cref="CsvReader.Current"/> property.
        /// </summary>
        /// <param name="skipCount">The number of records to skip.</param>
        /// <returns>This instance for fluent API enablement.</returns>
        public new TReader Skip(int skipCount = 1)
        {
            base.Skip(skipCount);
            return (this as TReader)!;
        }

        /// <summary>
        /// This method gets called after the user calls the <see cref="ReadHeadings"/> method
        /// and it may be used to build an initial map of properties for a structured reader.
        /// </summary>
        protected virtual void OnHeadingsRead()
        {
            // placeholder
        }

        /// <summary>
        /// If no headings have been set, this method automatically calls the <see cref="ReadHeadings(bool)"/>
        /// method to make those headings available. The method throws if proper conditions are not met.
        /// </summary>
        /// <param name="trimValues">Optionally trim the values (recommended).</param>
        protected void RequireHeadings(bool trimValues = true)
        {
            if (Headings.Any())
                return;

            ReadHeadings(trimValues);
        }
    }

}
