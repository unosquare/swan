#pragma warning disable CA1031 // Do not catch general exception types
using Swan.Platform;
using Swan.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Swan.Formatters
{
    /// <summary>
    /// Represents a text reader, typically a comma-separated set of values
    /// that can be configured to read an parse tabular data with flexible
    /// encoding, field separators and escape characters.
    /// </summary>
    public class CsvReader : IDisposable
    {
        /// <summary>
        /// Provides a the default separator character.
        /// </summary>
        public const char DefaultSeparatorChar = ',';

        /// <summary>
        /// Provides the default escape character.
        /// </summary>
        public const char DefaultEscapeChar = '"';

        private const int BufferSize = 1024;

        private readonly AtomicBoolean _IsDisposed = new();
        private readonly AtomicInteger _Count = new();
        private readonly StreamReader Reader;

        /// <summary>
        /// Creates a new instance of the <see cref="CsvReader"/> class.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="separatorChar">The field separator character.</param>
        /// <param name="escapeChar">The escape character.</param>
        /// <param name="leaveOpen">true to leave the stream open after the System.IO.StreamReader object is disposed; otherwise, false.</param>
        public CsvReader(Stream stream,
            Encoding? encoding = default,
            char separatorChar = DefaultSeparatorChar,
            char escapeChar = DefaultEscapeChar,
            bool leaveOpen = default)
        {
            var streamEncoding = encoding ?? SwanRuntime.Windows1252Encoding;
            var detectBom = streamEncoding.GetPreamble().Length > 0;
            Reader = new StreamReader(stream, streamEncoding, detectBom, BufferSize, leaveOpen);
            SeparatorChar = separatorChar;
            EscapeChar = escapeChar;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CsvReader"/> class.
        /// </summary>
        /// <param name="path">The file to read from.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="separatorChar">The field separator character.</param>
        /// <param name="escapeChar">The escape character.</param>
        public CsvReader(string path,
            Encoding? encoding = default,
            char separatorChar = DefaultSeparatorChar,
            char escapeChar = DefaultEscapeChar)
            : this(File.OpenRead(path), encoding, separatorChar, escapeChar, false)
        {
            // placeholder
        }

        /// <summary>
        /// Gets the dafault encoding used by the <see cref="CsvReader"/>
        /// whenever an encoding is not specified.
        /// </summary>
        public static Encoding DefaultEncoding { get; } = SwanRuntime.Windows1252Encoding;

        /// <summary>
        /// Gets the escape character.
        /// </summary>
        public char EscapeChar { get; }

        /// <summary>
        /// Gets the separator character.
        /// </summary>
        public char SeparatorChar { get; }

        /// <summary>
        /// Gets a value that indicates whether the current stream position is at the end
        /// of the stream.
        /// </summary>
        public bool EndOfStream => Reader.EndOfStream;

        /// <summary>
        /// Gets the encoding of the underlying <see cref="StreamReader"/>.
        /// </summary>
        public Encoding Encoding => Reader.CurrentEncoding;

        /// <summary>
        /// The number of records that have been read so far, including
        /// headers and empty ones, but excluding calls to the <see cref="SkipAsync"/>
        /// mathod.
        /// </summary>
        public int Count => _Count.Value;

        /// <summary>
        /// Gets the current record consisting of literals parsed from the internal stream reader
        /// as populated by the last successful read operation. This property may return null when
        /// no read operation has been successfully completed.
        /// </summary>
        public IReadOnlyList<string>? Current
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Dispose()"/> method has been called.
        /// </summary>
        public bool IsDisposed
        {
            get => _IsDisposed.Value;
            private set => _IsDisposed.Value = value;
        }

        /// <summary>
        /// Parses a set of literals from the current positions of the underlying
        /// stream just as <see cref="ReadAsync"/> but does not increment the <see cref="Count"/>
        /// and does not set <see cref="Current"/> property.
        /// </summary>
        /// <param name="skipCount">The number of records to skip.</param>
        /// <returns>An awaitable task.</returns>
        public virtual async Task SkipAsync(int skipCount = 1)
        {
            if (skipCount < 1)
                throw new ArgumentOutOfRangeException(nameof(skipCount));

            for (var i = 0; i < skipCount; i++)
                await ReadRecordAsync(true, false);
        }

        /// <summary>
        /// Parses a set of literals from the current positions of the underlying
        /// stream just as <see cref="ReadAsync"/> but does not increment the <see cref="Count"/>
        /// and does not set <see cref="Current"/> property.
        /// </summary>
        /// <param name="skipCount">The number of records to skip.</param>
        public virtual void Skip(int skipCount = 1) =>
            SkipAsync(skipCount).ConfigureAwait(false).GetAwaiter().GetResult();

        /// <summary>
        /// Reads a set of literals parsed from the internal stream reader,
        /// sets the <see cref="Current"/> property to the result and increments
        /// the <see cref="Count"/> by one.
        /// </summary>
        /// <param name="trimValues">Determines if values should be trimmed.</param>
        /// <returns>A set of values representing the parsed record.</returns>
        public virtual async Task<IReadOnlyList<string>> ReadAsync(bool trimValues = true)
        {
            await ReadRecordAsync(false, trimValues);
            return Current!;
        }

        /// <summary>
        /// Reads a set of literals parsed from the internal stream reader,
        /// sets the <see cref="Current"/> property to the result and increments
        /// the <see cref="Count"/> by one.
        /// </summary>
        /// <param name="trimValues">Determines if values should be trimmed.</param>
        /// <returns>A set of values representing the parsed record.</returns>
        public virtual IReadOnlyList<string> Read(bool trimValues = true) =>
            ReadAsync(trimValues).ConfigureAwait(false).GetAwaiter().GetResult();

        /// <summary>
        /// Reads a set of literals parsed from the internal stream reader,
        /// sets the <see cref="Current"/> property to the result and increments
        /// the <see cref="Count"/> by one.
        /// </summary>
        /// <returns>Returns true if the operation was successful; otherwise returns false.</returns>
        public virtual async Task<bool> TryReadAsync(bool trimValues = true)
        {
            if (IsDisposed || EndOfStream)
                return false;

            try
            {
                await ReadAsync(trimValues).ConfigureAwait(false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Reads a set of literals parsed from the internal stream reader,
        /// sets the <see cref="Current"/> property to the result and increments
        /// the <see cref="Count"/> by one.
        /// </summary>
        /// <param name="trimValues">Determines if values should be trimmed.</param>
        /// <returns>True if the read was successful. False otherwise.</returns>
        public virtual bool TryRead(bool trimValues = true)
        {
            if (IsDisposed || EndOfStream)
                return false;

            try
            {
                Read(trimValues);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the string value of a given field index in the <see cref="Current"/> record.
        /// </summary>
        /// <param name="index">The field index in the current</param>
        /// <param name="value"></param>
        /// <returns>True if the value was read successfully.</returns>
        public virtual bool TryGetValue(int index, out string value)
        {
            value = string.Empty;
            if (Current is null || index >= Current.Count || index < 0)
                return false;

            value = Current[index];
            return true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(alsoManaged: true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public override string ToString() =>
            $"{GetType()}: {Count} records read.";

        /// <summary>
        /// Parses a set of literals from the underlying stream, and when the skip parameter is set to false,
        /// increments the <see cref="Count"/> property by one and sets the <see cref="Current"/> property
        /// when the operation succeeds.
        /// </summary>
        /// <param name="isSkipping">True if the <see cref="Count"/> and <see cref="Current"/> properties will not be set.</param>
        /// <param name="trimValues">Determines if values should be trimmed.</param>
        /// <returns>An awaitable task.</returns>
        protected async ValueTask ReadRecordAsync(bool isSkipping, bool trimValues)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(CsvReader));

            if (EndOfStream)
                throw new EndOfStreamException("Unable to read past the end of the stream.");

            var readTask = ReadRecordAsync(Reader, trimValues, EscapeChar, SeparatorChar);

            var result = readTask.IsCompletedSuccessfully
                ? readTask.Result
                : await readTask.ConfigureAwait(false);

            if (isSkipping)
                return;

            Current = result;
            _Count.Increment();
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
        private static async ValueTask<IReadOnlyList<string>> ReadRecordAsync(StreamReader reader, bool trimValues, char escapeChar, char separatorChar)
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
#pragma warning restore CA1031 // Do not catch general exception types