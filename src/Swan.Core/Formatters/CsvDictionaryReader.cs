using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Swan.Formatters
{
    /// <summary>
    /// Provides a <see cref="CsvReader"/> that is schema-aware
    /// and is able to map records into a <see cref="Dictionary{TKey, TValue}"/>
    /// </summary>
    public class CsvDictionaryReader : CsvReader<CsvDictionaryReader, Dictionary<string, string?>>
    {
        private readonly Dictionary<string, CsvMapping<CsvDictionaryReader, IDictionary<string, string?>>> _targetMap = new(64);

        /// <summary>
        /// Creates a new instance of the <see cref="CsvDictionaryReader"/> class.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="separatorChar">The field separator character.</param>
        /// <param name="escapeChar">The escape character.</param>
        /// <param name="leaveOpen">true to leave the stream open after the stream reader object is disposed; otherwise, false.</param>
        /// <param name="trimsValues">True to trim field values as they are read and parsed.</param>
        /// <param name="trimsHeadings">True to trim heading values as they are read and parsed.</param>
        public CsvDictionaryReader(Stream stream,
            Encoding? encoding = default,
            char separatorChar = Csv.DefaultSeparatorChar,
            char escapeChar = Csv.DefaultEscapeChar,
            bool leaveOpen = default,
            bool trimsValues = true,
            bool trimsHeadings = true)
            : base(stream, encoding, separatorChar, escapeChar, leaveOpen, trimsValues, trimsHeadings)
        {
            // placeholder
        }

        /// <inheritdoc />
        public override Dictionary<string, string?> Current
        {
            get
            {
                RequireHeadings(true);
                var target = new Dictionary<string, string?>(_targetMap.Count);
                foreach (var mapping in _targetMap.Values)
                    mapping.Apply.Invoke(mapping, target);

                return target;
            }
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

            if (!Headings.ContainsKey(heading))
                throw new ArgumentException($"Heading name '{heading}' does not exist.");

            valueProvider ??= (s) => s;

            _targetMap[heading] = new(this, heading, targetName, (mapping, target) =>
            {
                target[mapping.TargetName] = mapping.Container.TryGetValue(mapping.Heading, out var value)
                    ? valueProvider(value)
                    : default;
            });

            return this;
        }

        /// <summary>
        /// Adds a set of mappings between source headings and target keys.
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
            RequireHeadings(false);

            _targetMap.Remove(heading);
            return this;
        }

        /// <inheritdoc />
        protected override void OnHeadingsRead(IReadOnlyList<string> headings)
        {
            foreach (var heading in headings)
                AddMapping(heading, heading);
        }
    }

}
