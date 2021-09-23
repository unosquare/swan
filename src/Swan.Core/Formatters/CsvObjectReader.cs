using Swan.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Swan.Formatters
{
    /// <summary>
    /// Provides a <see cref="CsvReader"/> that is schema-aware
    /// and is able to map records into the specified target type.
    /// </summary>
    public class CsvObjectReader<T> : CsvRecordReader<CsvObjectReader<T>>, ICsvEnumerable<T>
        where T : class
    {
        private readonly ITypeInfo TypeInfo = typeof(T).TypeInfo();
        private readonly Dictionary<string, CsvMapping<CsvObjectReader<T>, T>> TargetMap = new(64);

        /// <summary>
        /// Creates a new instance of the <see cref="CsvObjectReader{T}"/> class.
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
        /// Creates a new instance of the <see cref="CsvObjectReader{T}"/> class.
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

        T? ICsvEnumerable<T>.Current
        {
            get
            {
                if (!Headings.Any())
                {
                    SetHeadings(Current!.ToArray());
                    TryRead();
                }

                var target = typeof(T).TypeInfo().CreateInstance() as T;
                foreach (var mapping in TargetMap.Values)
                    mapping.Apply.Invoke(mapping, target!);

                return target;
            }
        }

        /// <summary>
        /// Reads and parses the values from the underlying stream and maps those
        /// values, writing them to a new instance of the target.
        /// </summary>
        /// <param name="trimValues">Determines if values should be trimmed.</param>
        /// <returns>A new instance of the target type with values loaded from the stream.</returns>
        public virtual T ReadObject(bool trimValues = true)
        {
            if (!TypeInfo.CanCreateInstance)
                throw new InvalidOperationException($"The type {TypeInfo.FullName} does not have a default constructor.");

            if (TypeInfo.CreateInstance() is not T target)
                throw new InvalidCastException($"Unable to create a compatible instance of {typeof(T)}");

            return ReadInto(target, trimValues);
        }

        /// <summary>
        /// Reads and parses the values from the underlying stream and maps those
        /// values, writing the corresponding target members.
        /// </summary>
        /// <param name="target">The target instance to read values into.</param>
        /// <param name="trimValues">Determines if values should be trimmed.</param>
        /// <returns>The target instance with record values loaded from the stream.</returns>
        public virtual T ReadInto(T target, bool trimValues = true)
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
        /// Adds or replaces a mapping between a heading and a target property using expressions and
        /// optionally, a transform function that converts the value from a string and into a type acceptable
        /// to the target. If no transform function is provided, one will be automatically provided using basic type
        /// conversion.
        /// </summary>
        /// <typeparam name="TTargetMember">The type of the target property.</typeparam>
        /// <param name="heading">The name of the heading to map from.</param>
        /// <param name="targetPropertyExpression">The expression containing the target property name.</param>
        /// <param name="valueProvider">The optional value provider method to transform the source string value into the appropriate target type.</param>
        /// <returns>This instance, in order to enable fluent API.</returns>
        public CsvObjectReader<T> AddMapping<TTargetMember>(string heading,
            Expression<Func<T, TTargetMember>> targetPropertyExpression,
            Func<string, TTargetMember>? valueProvider = default)
        {
            if (heading is null)
                throw new ArgumentNullException(nameof(heading));

            RequireHeadings();

            if (!Headings.ContainsKey(heading))
                throw new ArgumentException($"Heading name '{heading}' does not exist.");

            if (targetPropertyExpression is null)
                throw new ArgumentNullException(nameof(targetPropertyExpression));

            if ((targetPropertyExpression.Body as MemberExpression)?.Member is not PropertyInfo targetPropertyInfo)
                throw new ArgumentException("Invalid target expression", nameof(targetPropertyExpression));

            if (!TypeInfo.TryFindProperty(targetPropertyInfo.Name, out var targetProperty))
                throw new ArgumentException(
                    $"Property '{TypeInfo.FullName}.{targetPropertyInfo.Name}' was not found.",
                    nameof(targetPropertyExpression));

            if (!targetProperty.CanWrite)
                throw new ArgumentException(
                    $"Property '{TypeInfo.FullName}.{targetPropertyInfo.Name}' cannot be written to.",
                    nameof(targetPropertyExpression));

            AddMapping(heading, targetProperty, valueProvider is null ? (s) => s : (s) => valueProvider(s));

            return this;
        }

        /// <summary>
        /// Removes a source heading from the field mappings.
        /// </summary>
        /// <param name="heading">The heading to be removed from the mapping.</param>
        /// <returns>This instance, in order to enable fluent API.</returns>
        public CsvObjectReader<T> RemoveMapping(string heading)
        {
            RequireHeadings();

            TargetMap.Remove(heading);
            return this;
        }

        /// <inheritdoc />
        public new IEnumerator<T> GetEnumerator() =>
            new CsvEnumerator<CsvObjectReader<T>, T>(this);

        /// <inheritdoc />
        public new IAsyncEnumerator<T> GetAsyncEnumerator(
            CancellationToken cancellationToken = default) =>
            new CsvEnumerator<CsvObjectReader<T>, T>(this);

        /// <inheritdoc />
        protected override void OnHeadingsRead()
        {
            foreach (var heading in Headings.Keys)
            {
                if (!TypeInfo.TryFindProperty(heading, out var property))
                    continue;

                if (!property.CanWrite)
                    continue;

                AddMapping(heading, property, (s) => s);
            }
        }

        private void AddMapping(string heading, IPropertyProxy property, Func<string, object?> valueProvider)
        {
            TargetMap[heading] = new(this, heading, property.PropertyName, (mapping, target) =>
            {
                if (!mapping.Reader.TryGetValue(mapping.Heading, out var value))
                    return;

                if (!property.CanWrite)
                    return;

                property.TryWrite(target, valueProvider(value));
            });
        }
    }
}
