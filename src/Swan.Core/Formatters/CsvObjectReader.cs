namespace Swan.Formatters
{
    using Swan.Reflection;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// Provides a <see cref="CsvReader"/> that is schema-aware
    /// and is able to map records into the specified target type.
    /// </summary>
    public class CsvObjectReader<T> : CsvReader<CsvObjectReader<T>, T>
        where T : class, new()
    {
        private readonly ITypeInfo _typeInfo = typeof(T).TypeInfo();
        private readonly Dictionary<string, CsvMapping<CsvObjectReader<T>, T>> _targetMap = new(64);

        /// <summary>
        /// Creates a new instance of the <see cref="CsvObjectReader{T}"/> class.
        /// </summary>
        /// <param name="stream">The stream to read.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="separatorChar">The field separator character.</param>
        /// <param name="escapeChar">The escape character.</param>
        /// <param name="leaveOpen">true to leave the stream open after the System.IO.StreamReader object is disposed; otherwise, false.</param>
        /// <param name="trimsValues">True to trim field values as they are read and parsed.</param>
        /// <param name="trimsHeadings">True to trim heading values as they are read and parsed.</param>
        public CsvObjectReader(Stream stream,
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
        public override T Current
        {
            get
            {
                RequireHeadings(true);
                var target = new T();
                foreach (var mapping in _targetMap.Values)
                    mapping.Apply.Invoke(mapping, target);

                return target;
            }
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

            if (!Headings.ContainsKey(heading))
                throw new ArgumentException($"Heading name '{heading}' does not exist.");

            if (targetPropertyExpression is null)
                throw new ArgumentNullException(nameof(targetPropertyExpression));

            if ((targetPropertyExpression.Body as MemberExpression)?.Member is not PropertyInfo targetPropertyInfo)
                throw new ArgumentException("Invalid target expression", nameof(targetPropertyExpression));

            if (!_typeInfo.TryFindProperty(targetPropertyInfo.Name, out var targetProperty))
                throw new ArgumentException(
                    $"Property '{_typeInfo.FullName}.{targetPropertyInfo.Name}' was not found.",
                    nameof(targetPropertyExpression));

            if (!targetProperty.CanWrite)
                throw new ArgumentException(
                    $"Property '{_typeInfo.FullName}.{targetPropertyInfo.Name}' cannot be written to.",
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
            RequireHeadings(false);

            _targetMap.Remove(heading);
            return this;
        }

        /// <inheritdoc />
        protected override void OnHeadingsRead(IReadOnlyList<string> headings)
        {
            if (headings is null)
                throw new ArgumentNullException(nameof(headings));

            foreach (var heading in headings)
            {
                if (!_typeInfo.TryFindProperty(heading, out var property))
                    continue;

                if (!property.CanWrite)
                    continue;

                AddMapping(heading, property, (s) => s);
            }
        }

        private void AddMapping(string heading, IPropertyProxy property, Func<string, object?> valueProvider)
        {
            _targetMap[heading] = new(this, heading, property.PropertyName, (mapping, target) =>
            {
                if (!mapping.Container.TryGetValue(mapping.Heading, out var value))
                    return;

                if (!property.CanWrite)
                    return;

                property.TryWrite(target, valueProvider(value));
            });
        }
    }
}
