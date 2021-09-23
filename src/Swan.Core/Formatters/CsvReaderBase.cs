using Swan.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Swan.Formatters
{
    /// <summary>
    /// Represents a base class for all CSV readers, which typically read
    /// a comma-separated set of values that can be configured to read an parse tabular data with flexible
    /// encoding, field separators and escape characters.
    /// </summary>
    public abstract class CsvReaderBase<TLine> : ICsvReader<TLine>
    {
        private const int BufferSize = 4096;
        private readonly AtomicBoolean _IsDisposed = new();
        private readonly AtomicInteger _Count = new();
        private readonly StreamReader Reader;

        /// <summary>
        /// Creates a new instance of the <see cref="CsvReaderBase{TLine}"/> class.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="separatorChar">The field separator character.</param>
        /// <param name="escapeChar">The escape character.</param>
        /// <param name="leaveOpen">true to leave the stream open after the stream reader object is disposed; otherwise, false.</param>
        /// <param name="trimsValues">True to trim field values as they are read and parsed.</param>
        protected CsvReaderBase(Stream stream,
            Encoding? encoding,
            char separatorChar,
            char escapeChar,
            bool leaveOpen,
            bool trimsValues)
        {
            var streamEncoding = encoding ?? Csv.DefaultEncoding;
            var detectBom = streamEncoding.GetPreamble().Length > 0;
            Reader = new(stream, streamEncoding, detectBom, BufferSize, leaveOpen);
            SeparatorChar = separatorChar;
            EscapeChar = escapeChar;
            TrimsValues = trimsValues;
        }

        /// <summary>
        /// Gets the escape character.
        /// </summary>
        public char EscapeChar { get; }

        /// <summary>
        /// Gets the separator character.
        /// </summary>
        public char SeparatorChar { get; }

        /// <summary>
        /// Gets a value indicating whether the reader trims or removes
        /// whitespace from the records it reads.
        /// </summary>
        public bool TrimsValues { get; }

        /// <inheritdoc />
        public bool EndOfStream => Reader.EndOfStream;

        /// <summary>
        /// Gets the encoding of the underlying <see cref="StreamReader"/>.
        /// </summary>
        public Encoding Encoding => Reader.CurrentEncoding;

        /// <inheridoc />
        public int Count => _Count.Value;

        /// <inheridoc />
        public IReadOnlyList<string>? Values
        {
            get;
            private set;
        }

        /// <inheridoc />
        public bool IsDisposed
        {
            get => _IsDisposed.Value;
            private set => _IsDisposed.Value = value;
        }

        /// <inheridoc />
        public abstract TLine Current { get; }

        /// <inheridoc />
        object IEnumerator.Current =>
            Current ??
            throw new InvalidOperationException("The reader is not in a valid state.");

        /// <inheridoc />
        public async ValueTask SkipAsync(int skipCount = 1)
        {
            if (skipCount < 1)
                throw new ArgumentOutOfRangeException(nameof(skipCount));

            for (var i = 0; i < skipCount; i++)
                await ReadValuesAsync(true, false).ConfigureAwait(false);
        }

        /// <inheridoc />
        public void Skip(int skipCount = 1)
        {
            if (skipCount < 1)
                throw new ArgumentOutOfRangeException(nameof(skipCount));

            for (var i = 0; i < skipCount; i++)
                ReadValues(true, false);
        }

        /// <inheritdoc />
        public bool MoveNext(bool trimValues)
        {
            if (IsDisposed || EndOfStream)
                return false;

            try
            {
                ReadValues(false, trimValues);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool MoveNext() => MoveNext(TrimsValues);

        /// <inheritdoc />
        public async ValueTask<bool> MoveNextAsync() => await MoveNextAsync(TrimsValues);

        /// <inheritdoc />
        public async ValueTask<bool> MoveNextAsync(bool trimValues)
        {
            if (IsDisposed || EndOfStream)
                return false;

            try
            {
                await ReadValuesAsync(false, trimValues).ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public bool TryGetValue(int index, out string value)
        {
            value = string.Empty;
            if (Values is null || index >= Values.Count || index < 0)
                return false;

            value = Values[index];
            return true;
        }

        /// <inheritdoc />
        public void Reset() => throw new NotSupportedException("Reset is not supported by CSV readers.");

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(alsoManaged: true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }

        /// <inheritdoc />
        public override string ToString() =>
            $"{GetType()}: {Count} records read.";

        /// <inheritdoc />
        public IAsyncEnumerator<TLine> GetAsyncEnumerator(CancellationToken cancellationToken = default) => this;

        /// <inheritdoc />
        public IEnumerator<TLine> GetEnumerator() => this;

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => this;

        /// <summary>
        /// Parses a set of literals from the underlying stream, and when the skip parameter is set to false,
        /// increments the <see cref="Count"/> property by one and sets the <see cref="Values"/> property
        /// when the operation succeeds.
        /// </summary>
        /// <param name="isSkipping">True if the <see cref="Count"/> and <see cref="Values"/> properties will not be set.</param>
        /// <param name="trimValues">Determines if values should be trimmed.</param>
        /// <returns>An awaitable task.</returns>
        protected async ValueTask ReadValuesAsync(bool isSkipping, bool trimValues)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(CsvReader));

            if (EndOfStream)
                throw new EndOfStreamException("Unable to read past the end of the stream.");

            var result = await ReadValuesAsync(Reader, trimValues, EscapeChar, SeparatorChar)
                .ConfigureAwait(false);

            if (isSkipping)
                return;

            Values = result;
            _Count.Increment();
        }

        /// <summary>
        /// Synchronously calls the <see cref="ReadValuesAsync(bool,bool)"/> method.
        /// </summary>
        /// <param name="isSkipping">True if the <see cref="Count"/> and <see cref="Values"/> properties will not be set.</param>
        /// <param name="trimValues">Determines if values should be trimmed.</param>
        protected void ReadValues(bool isSkipping, bool trimValues)
        {
            var readTask = ReadValuesAsync(isSkipping, trimValues);
            if (readTask.IsCompletedSuccessfully)
                return;

            readTask.AsTask().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Disposes this instance optionally disposing
        /// of managed objects.
        /// </summary>
        /// <param name="alsoManaged">If managed objects should also be disposed of.</param>
        protected virtual void Dispose(bool alsoManaged)
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            if (alsoManaged)
                Reader.Dispose();
        }

        /// <summary>
        /// Parses a line of standard CSV text into an array of strings.
        /// Note that quoted values might have new line sequences in them. Field values will contain such sequences.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="trimValues">Determines if values should be trimmed.</param>
        /// <param name="escapeChar">The escape character.</param>
        /// <param name="separatorChar">The separator character.</param>
        /// <returns>An array of the specified element type containing copies of the elements of the ArrayList.</returns>
        private static async ValueTask<IReadOnlyList<string>> ReadValuesAsync(TextReader reader, bool trimValues, char escapeChar, char separatorChar)
        {
            var values = new List<string>(64);
            var currentValue = new StringBuilder(256);
            var currentState = ReadState.WaitingForNewField;
            string? line;

            while ((line = await reader.ReadLineAsync()) is not null)
            {
                for (var charIndex = 0; charIndex < line.Length; charIndex++)
                {
                    // Get the current and next character
                    var currentChar = line[charIndex];
                    var nextChar = charIndex < line.Length - 1
                        ? line[charIndex + 1]
                        : default(char?);

                    // Perform logic based on state and decide on next state
                    switch (currentState)
                    {
                        case ReadState.WaitingForNewField:
                            currentValue.Clear();

                            if (currentChar == escapeChar)
                            {
                                currentState = ReadState.PushingQuoted;
                                continue;
                            }

                            if (currentChar == separatorChar)
                            {
                                values.Add(trimValues ? currentValue.ToString().Trim() : currentValue.ToString());
                                currentState = ReadState.WaitingForNewField;
                                continue;
                            }

                            currentValue.Append(currentChar);
                            currentState = ReadState.PushingNormal;
                            continue;

                        case ReadState.PushingNormal:
                            // Handle field content delimiter separator char
                            if (currentChar == separatorChar)
                            {
                                currentState = ReadState.WaitingForNewField;
                                values.Add(trimValues ? currentValue.ToString().Trim() : currentValue.ToString());
                                currentValue.Clear();
                                continue;
                            }

                            // Handle double quote escaping
                            if (currentChar == escapeChar && nextChar.HasValue && nextChar == escapeChar)
                            {
                                // advance 1 character now. The loop will advance one more.
                                currentValue.Append(currentChar);
                                charIndex++;
                                continue;
                            }

                            currentValue.Append(currentChar);
                            break;

                        case ReadState.PushingQuoted:
                            // Handle field content delimiter by ending double quotes
                            if (currentChar == escapeChar && (nextChar.HasValue == false || nextChar != escapeChar))
                            {
                                currentState = ReadState.PushingNormal;
                                continue;
                            }

                            // Handle double quote escaping
                            if (currentChar == escapeChar && nextChar.HasValue && nextChar == escapeChar)
                            {
                                // advance 1 character now. The loop will advance one more.
                                currentValue.Append(currentChar);
                                charIndex++;
                                continue;
                            }

                            currentValue.Append(currentChar);
                            break;
                    }
                }

                // determine if we need to continue reading a new line if it is part of the quoted
                // field value
                if (currentState == ReadState.PushingQuoted)
                {
                    // we need to add the new line sequence to the output of the field
                    // because we were pushing a quoted value
                    currentValue.Append(Environment.NewLine);
                }
                else
                {
                    // push anything that has not been pushed (flush) into a last value
                    values.Add(trimValues ? currentValue.ToString().Trim() : currentValue.ToString());
                    currentValue.Clear();

                    // stop reading more lines we have reached the end of the CSV record
                    break;
                }
            }

            // If we ended up pushing quoted and no closing quotes we might
            // have additional text in it
            if (currentValue.Length > 0)
                values.Add(trimValues ? currentValue.ToString().Trim() : currentValue.ToString());

            return values;
        }

        /// <summary>
        /// Defines the 3 different read states
        /// for the parsing state machine.
        /// </summary>
        private enum ReadState
        {
            WaitingForNewField,
            PushingNormal,
            PushingQuoted,
        }
    }
}
