using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Swan.Formatters
{
    /// <summary>
    /// Provides a <see cref="CsvReader"/> that is schema-aware
    /// and is able to map records into a <see cref="Dictionary{TKey, TValue}"/>
    /// </summary>
    public class CsvDictionaryReader : CsvRecordReader<CsvDictionaryReader>, ICsvEnumerable<Dictionary<string, string?>>
    {
        private readonly Dictionary<string, CsvMapping<CsvDictionaryReader, IDictionary<string, string?>>> TargetMap = new(64);

        /// <summary>
        /// Creates a new instance of the <see cref="CsvDictionaryReader"/> class.
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
        /// Creates a new instance of the <see cref="CsvRecordReader{T}"/> class.
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

        Dictionary<string, string?> ICsvEnumerable<Dictionary<string, string?>>.Current
        {
            get
            {
                if (!Headings.Any())
                {
                    SetHeadings(Current!.ToArray());
                    TryRead();
                }

                var target = new Dictionary<string, string?>(TargetMap.Count);
                foreach (var mapping in TargetMap.Values)
                    mapping.Apply.Invoke(mapping, target);

                return target;
            }
        }

        /// <summary>
        /// Reads and parses the values from the underlying stream and maps those
        /// values, writing them to a new instance of the target.
        /// </summary>
        /// <param name="trimValues">Determines if values should be trimmed.</param>
        /// <returns>A new instance of the target type with values loaded from the stream.</returns>
        public virtual IDictionary<string, string?> ReadObject(bool trimValues = true)
        {
            var result = new Dictionary<string, string?>(TargetMap.Count);
            return ReadInto(result, trimValues);
        }

        /// <summary>
        /// Reads and parses the values from the underlying stream and maps those
        /// values, writing the corresponding target members.
        /// </summary>
        /// <param name="target">The target instance to read values into.</param>
        /// <param name="trimValues">Determines if values should be trimmed.</param>
        /// <returns>The target instance with values loaded from the stream.</returns>
        public virtual IDictionary<string, string?> ReadInto(IDictionary<string, string?> target, bool trimValues = true)
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            RequireHeadings();

            if (TargetMap is null || TargetMap.Count == 0)
                throw new InvalidOperationException("No schema mappings are available.");

            _ = Read(trimValues);

            foreach (var mapping in TargetMap.Values)
                mapping.Apply.Invoke(mapping, target);

            return target;
        }

        /// <summary>
        /// Maps a heading to the specified target, optionally providing a transform that takes in the original
        /// field value and returning a new value to set. If a mapping for the field already exists, it is overwritten.
        /// </summary>
        /// <param name="heading">The name of the heading for the field value.</param>
        /// <param name="targetName">The name of the key to write the value to.</param>
        /// <param name="valueProvider">The transform function taking in a string and producing another one.</param>
        /// <returns>This instance, in order to enable fluent API.</returns>
        public CsvDictionaryReader AddMapping(string heading, string targetName, Func<string, string>? valueProvider = default)
        {
            if (heading is null)
                throw new ArgumentNullException(nameof(heading));

            if (targetName is null)
                throw new ArgumentNullException(nameof(targetName));

            RequireHeadings();

            if (!Headings.ContainsKey(heading))
                throw new ArgumentException($"Heading name '{heading}' does not exist.");

            valueProvider ??= (s) => s;

            TargetMap[heading] = new(this, heading, targetName, (mapping, target) =>
            {
                target[mapping.TargetName] = mapping.Reader.TryGetValue(mapping.Heading, out var value)
                    ? valueProvider(value)
                    : default;
            });

            return this;
        }

        /// <summary>
        /// Adds a set of mappings between source headings and tarhget keys.
        /// </summary>
        /// <param name="map">The dictionary containing source headings and source dictionary keys.</param>
        /// <returns>This instance, in order to enable fluent API.</returns>
        public CsvDictionaryReader AddMappings(IDictionary<string, string> map)
        {
            if (map is null)
                throw new ArgumentNullException(nameof(map));

            foreach (var kvp in map)
                AddMapping(kvp.Key, kvp.Value);

            return this;
        }

        /// <summary>
        /// Removes a source heading from the field mappings.
        /// </summary>
        /// <param name="heading">The heading to be removed from the mapping.</param>
        /// <returns>This instance, in order to enable fluent API.</returns>
        public CsvDictionaryReader RemoveMapping(string heading)
        {
            RequireHeadings();

            TargetMap.Remove(heading);
            return this;
        }

        /// <inheritdoc />
        protected override void OnHeadingsRead()
        {
            foreach (var heading in Headings!)
                AddMapping(heading.Key, heading.Key);
        }

        /// <inheritdoc />
        public new IEnumerator<Dictionary<string, string?>> GetEnumerator() =>
            new CsvEnumerator<CsvDictionaryReader, Dictionary<string, string?>>(this);

        /// <inheritdoc />
        public new IAsyncEnumerator<Dictionary<string, string?>> GetAsyncEnumerator(
            CancellationToken cancellationToken = default) =>
            new CsvEnumerator<CsvDictionaryReader, Dictionary<string, string?>>(this);

    }

}
