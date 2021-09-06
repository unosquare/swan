#pragma warning disable CA1031 // Do not catch general exception types
using Swan.Platform;
using Swan.Threading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swan.Formatters
{
    /// <summary>
    /// Represents a text reader, typically a comma-separated set of values
    /// that can be configured to read an parse tabular data with flexible
    /// encoding, field separators and escape characters.
    /// </summary>
    public class CommaReader : IDisposable
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
        /// Creates a new instance of the <see cref="CommaReader"/> class.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="separatorChar">The field separator character.</param>
        /// <param name="escapeChar">The escape character.</param>
        /// <param name="leaveOpen">true to leave the stream open after the System.IO.StreamReader object is disposed; otherwise, false.</param>
        public CommaReader(Stream stream,
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
        /// Creates a new instance of the <see cref="CommaReader"/> class.
        /// </summary>
        /// <param name="path">The file to read from.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="separatorChar">The field separator character.</param>
        /// <param name="escapeChar">The escape character.</param>
        public CommaReader(string path,
            Encoding? encoding = default,
            char separatorChar = DefaultSeparatorChar,
            char escapeChar = DefaultEscapeChar)
            : this(File.OpenRead(path), encoding, separatorChar, escapeChar, false)
        {
            // placeholder
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
                throw new ObjectDisposedException(nameof(CommaReader));

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

    /// <summary>
    /// Enables reading schema aware <see cref="CommaReader"/> streams
    /// where headings are used to map to a specific target.
    /// </summary>
    public abstract class CsvStructuredReader<T> : CommaReader
        where T : CommaReader
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CsvStructuredReader"/> class.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="separatorChar">The field separator character.</param>
        /// <param name="escapeChar">The escape character.</param>
        /// <param name="leaveOpen">true to leave the stream open after the System.IO.StreamReader object is disposed; otherwise, false.</param>
        protected CsvStructuredReader(Stream stream,
            Encoding? encoding = default,
            char separatorChar = DefaultSeparatorChar,
            char escapeChar = DefaultEscapeChar,
            bool leaveOpen = default)
            : base(stream, encoding, separatorChar, escapeChar, leaveOpen)
        {
            // placeholder
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CsvStructuredReader"/> class.
        /// </summary>
        /// <param name="path">The file to read from.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="separatorChar">The field separator character.</param>
        /// <param name="escapeChar">The escape character.</param>
        protected CsvStructuredReader(string path,
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
        public IReadOnlyDictionary<string, int>? Headings
        {
            get;
            protected set;
        }

        /// <summary>
        /// Gets the index of the given heading name.
        /// </summary>
        /// <param name="heading">The name of the heading.</param>
        /// <returns>Returns a valid field index or -1 for invalid or not found.</returns>
        public virtual int IndexOf(string heading)
        {
            if (Headings is null || !Headings.TryGetValue(heading, out var index))
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
        public virtual T ReadHeadings(bool trimValues = true)
        {
            if (Headings is not null)
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
        public virtual T SetHeadings(params string[] headings)
        {
            if (Headings is not null)
                throw new InvalidOperationException($"The {nameof(Headings)} have already been set.");

            if (headings is null || headings.Length <= 0)
                throw new ArgumentException($"Headings must contain at least one element.", nameof(headings));

            var result = new Dictionary<string, int>(headings.Length);
            for (var i = 0; i < headings.Length; i++)
                result[headings[i]] = i;

            Headings = result;
            CheckHeadingsValid();
            OnHeadingsRead();
            return (this as T)!;
        }

        /// <summary>
        /// Parses a set of literals from the current positions of the underlying
        /// stream just as <see cref="CommaReader.ReadAsync"/> but does not increment the <see cref="CommaReader.Count"/>
        /// and does not set <see cref="CommaReader.Current"/> property.
        /// </summary>
        /// <param name="skipCount">The number of records to skip.</param>
        /// <returns>This instance for fluent API enablement.</returns>
        public new T Skip(int skipCount = 1)
        {
            base.Skip(skipCount);
            return (this as T)!;
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
        /// Throws if <see cref="Headings"/> are null or have no elements.
        /// </summary>
        protected bool CheckHeadingsValid()
        {
            if (Headings is null || Headings.Count <= 0)
                throw new InvalidOperationException($"{nameof(Headings)} is either null or contains no values. " +
                    $"Make sure you have called either the {nameof(ReadHeadings)} or {nameof(SetHeadings)} methods.");

            return true;
        }
    }

    internal delegate void ApplyMapping<TReader, TTarget>(Mapping<TReader, TTarget> mapping, TTarget instance);

    internal class Mapping<TReader, TTarget>
    {
        public Mapping(TReader reader, string heading, string targetName, ApplyMapping<TReader, TTarget> apply)
        {
            Heading = heading;
            TargetName = targetName;
            Reader = reader;
            Apply = apply;
        }

        public string Heading { get; }

        public string TargetName { get; }

        public TReader Reader { get; }

        public ApplyMapping<TReader, TTarget> Apply { get; }
    }

    /// <summary>
    /// Provides a <see cref="CommaReader"/> that is schema-aware
    /// and is able to map records into a <see cref="Dictionary{TKey, TValue}"/>
    /// </summary>
    public class CsvDictionaryReader : CsvStructuredReader<CsvDictionaryReader>
    {
        private readonly Dictionary<string, Mapping<CsvDictionaryReader, IDictionary<string, string?>>> TargetMap = new(64);

        /// <summary>
        /// Creates a new instance of the <see cref="CsvStructuredReader"/> class.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="separatorChar">The field separator character.</param>
        /// <param name="escapeChar">The escape character.</param>
        /// <param name="leaveOpen">true to leave the stream open after the System.IO.StreamReader object is disposed; otherwise, false.</param>
        public CsvDictionaryReader(Stream stream,
            Encoding? encoding = default,
            char separatorChar = DefaultSeparatorChar,
            char escapeChar = DefaultEscapeChar,
            bool leaveOpen = default)
            : base(stream, encoding, separatorChar, escapeChar, leaveOpen)
        {
            // placeholder
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CsvStructuredReader"/> class.
        /// </summary>
        /// <param name="path">The file to read from.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="separatorChar">The field separator character.</param>
        /// <param name="escapeChar">The escape character.</param>
        public CsvDictionaryReader(string path,
            Encoding? encoding = default,
            char separatorChar = DefaultSeparatorChar,
            char escapeChar = DefaultEscapeChar)
            : base(File.OpenRead(path), encoding, separatorChar, escapeChar, false)
        {
            // placeholder
        }

        /// <summary>
        /// Reads and parses the values from the underlyings stream and maps those
        /// values, writing them to a new instance of the target.
        /// </summary>
        /// <param name="trimValues">Determines if values should be trimmed.</param>
        /// <returns>A new instance of the target type with values loaded from the stream.</returns>
        public IDictionary<string, string?> ReadObject(bool trimValues = true)
        {
            var result = new Dictionary<string, string?>(Headings!.Count);
            return ReadInto(result, trimValues);
        }

        /// <summary>
        /// Reads and parses the values from the underlyings stream and maps those
        /// values, writing the corresponding target members.
        /// </summary>
        /// <param name="target">The target instance to read values into.</param>
        /// <param name="trimValues">Determines if values should be trimmed.</param>
        /// <returns>The target instance with values loaded from the stream.</returns>
        public IDictionary<string, string?> ReadInto(IDictionary<string, string?> target, bool trimValues = true)
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            if (TargetMap is null || TargetMap.Count == 0)
                throw new InvalidOperationException("No schema mappings are available.");

            _ = Read(trimValues);

            foreach (var mapping in TargetMap.Values)
                mapping.Apply.Invoke(mapping, target);

            return target;
        }

        public CsvDictionaryReader AddMapping(string heading, string targetName, Func<string, string> valueProvider)
        {
            CheckHeadingsValid();

            if (heading is null)
                throw new ArgumentNullException(nameof(heading));

            if (targetName is null)
                throw new ArgumentNullException(nameof(targetName));

            if (valueProvider is null)
                throw new ArgumentNullException(nameof(valueProvider));

            if (!Headings!.ContainsKey(heading))
                throw new ArgumentException($"Heading name '{heading}' does not exist.");

            TargetMap[heading] = new(this, heading, targetName, (mapping, target) =>
            {
                target[mapping.TargetName] = mapping.Reader.TryGetValue(mapping.Heading, out var value)
                    ? valueProvider(value)
                    : default;
            });

            return this;
        }

        public CsvDictionaryReader AddMapping(string heading, string targetName) =>
            AddMapping(heading, targetName, (s) => s);

        public CsvDictionaryReader RemoveMapping(string heading)
        {
            TargetMap.Remove(heading);
            return this;
        }

        protected override void OnHeadingsRead()
        {
            foreach (var heading in Headings!)
                AddMapping(heading.Key, heading.Key);
        }
    }

    /*
    public class CsvObjectReader<T> : CsvStructuredReader
    {
        private readonly ITypeProxy Proxy = typeof(T).TypeInfo();
        private readonly Dictionary<string, Mapping<T>> TargetMap = new(64);

        /// <summary>
        /// Creates a new instance of the <see cref="CsvStructuredReader"/> class.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="separatorChar">The field separator character.</param>
        /// <param name="escapeChar">The escape character.</param>
        /// <param name="leaveOpen">true to leave the stream open after the System.IO.StreamReader object is disposed; otherwise, false.</param>
        public CsvObjectReader(Stream stream,
            Encoding? encoding = default,
            char separatorChar = DefaultSeparatorChar,
            char escapeChar = DefaultEscapeChar,
            bool leaveOpen = default)
            : base(stream, encoding, separatorChar, escapeChar, leaveOpen)
        {
            // placeholder
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CsvStructuredReader"/> class.
        /// </summary>
        /// <param name="path">The file to read from.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="separatorChar">The field separator character.</param>
        /// <param name="escapeChar">The escape character.</param>
        public CsvObjectReader(string path,
            Encoding? encoding = default,
            char separatorChar = DefaultSeparatorChar,
            char escapeChar = DefaultEscapeChar)
            : base(File.OpenRead(path), encoding, separatorChar, escapeChar, false)
        {
            // placeholder
        }


        public CsvObjectReader<T> AddMapping<TTargetMember>(string heading,
            Expression<Func<T, TTargetMember>> targetPropertyExpression,
            Func<string, TTargetMember> valueProvider)
        {
            if (targetPropertyExpression is null)
                throw new ArgumentNullException(nameof(targetPropertyExpression));

            if (valueProvider is null)
                throw new ArgumentNullException(nameof(valueProvider));

            var targetProperty = (targetPropertyExpression.Body as MemberExpression)?.Member as PropertyInfo;

            if (targetProperty is null)
                throw new ArgumentException("Invalid target expression", nameof(targetPropertyExpression));

            TargetMap[heading] = new(this, heading, targetProperty.Name, (mapping, target) =>
            {
                if (!mapping.Reader.TryGetValue(mapping.Heading, out var value))
                    return;

                var property = Proxy.Properties[mapping.TargetName];
                if (!property.CanWrite)
                    return;

                property.TrySetValue(target!, valueProvider(value));
            });

            return this;
        }

    }
    */
}
#pragma warning restore CA1031 // Do not catch general exception types