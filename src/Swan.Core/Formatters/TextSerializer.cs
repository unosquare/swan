using Swan.Reflection;
using System;
using System.Collections;
using System.Text;
using System.Text.Json;

namespace Swan.Formatters
{
    /// <summary>
    /// Represents a generic text serializer. 
    /// </summary>
    public static class TextSerializer
    {
        /// <summary>
        /// Converts the object into a flexible text representation as determined by
        /// the <see cref="TextSerializerOptions"/>.
        /// </summary>
        /// <param name="instance">The object instance to serialize.</param>
        /// <param name="options">The text serializer options.</param>
        /// <returns></returns>
        public static string Serialize(object? instance, TextSerializerOptions? options = null)
        {
            options ??= TextSerializerOptions.JsonPrettyPrint;
            return Serialize(instance, options, 0, 1).ToString();
        }

        private static ReadOnlySpan<char> Serialize(object? instance, TextSerializerOptions options, int stackDepth, int indentDepth)
        {
            if (instance is null)
                return options.NullLiteral;

            var proxy = instance.GetType().TypeInfo();

            if (instance is Type typeValue)
                return WriteAsString(options, $"{typeValue}");

            if (instance is bool boolValue)
                return boolValue ? options.TrueLiteral : options.FalseLiteral;

            if (instance is string stringValue)
                return WriteAsString(options, stringValue);

            if (proxy.IsNumeric)
                return proxy.ToStringInvariant(instance);

            if (proxy.IsBasicType)
                return WriteAsString(options, proxy.ToStringInvariant(instance));

            if (instance is JsonElement element)
                return WriteJsonElement(element, options, stackDepth, indentDepth);

            var builder = new StringBuilder(256);

            if (TryWriteAsDictionary(builder, proxy, instance, options, stackDepth, indentDepth))
                return builder.ToString();

            if (TryWriteAsArray(builder, instance, options, stackDepth, indentDepth))
                return builder.ToString();

            return WriteAsObject(builder, proxy, instance, options, stackDepth, indentDepth);
        }

        private static ReadOnlySpan<char> WriteAsString(TextSerializerOptions options, ReadOnlySpan<char> value)
        {
            var encodedValue = Encoding.UTF8.GetString(JsonEncodedText.Encode(value).EncodedUtf8Bytes);
            return $"{options.StringQuotation}{encodedValue}{options.StringQuotation}";
        }

        private static ReadOnlySpan<char> WriteJsonElement(JsonElement element, TextSerializerOptions options, int stackDepth, int indentDepth) =>
             element.ValueKind switch
             {
                 JsonValueKind.Null => options.NullLiteral,
                 JsonValueKind.False => options.FalseLiteral,
                 JsonValueKind.True => options.TrueLiteral,
                 JsonValueKind.Undefined => options.NullLiteral,
                 JsonValueKind.Number => element.ToString() ?? "0",
                 JsonValueKind.String => WriteAsString(options, element.GetString()),
                 JsonValueKind.Array => Serialize(
                     new JsonDynamicObject(element).Materialize(), options, stackDepth + 1, indentDepth),
                 JsonValueKind.Object => Serialize(
                     new JsonDynamicObject(element).Materialize(), options, stackDepth + 1, indentDepth),
                 _ => element.ToString() ?? options.NullLiteral,
             };

        private static bool TryWriteAsDictionary(StringBuilder builder, ITypeProxy proxy, object instance, TextSerializerOptions options, int stackDepth, int indentDepth)
        {
            var dictionaryType = proxy.GenericDictionaryType;
            if (dictionaryType is null || !dictionaryType.GenericTypeArguments[0].IsBasicType)
                return false;

            // obtain keys and values
            var keys = dictionaryType.Properties[nameof(IDictionary.Keys)].GetValue(instance) as IEnumerable;
            var values = dictionaryType.Properties[nameof(IDictionary.Values)].GetValue(instance) as IEnumerable;

            // check that keys and values are in fact available.
            if (keys is null || values is null)
                return false;

            var valuesEnumerator = values.GetEnumerator();
            var keyType = dictionaryType.GenericTypeArguments[0];

            var isFirst = true;
            BeginObject(options, builder);
            foreach (var key in keys)
            {
                valuesEnumerator.MoveNext();
                var value = valuesEnumerator.Current;

                if (!isFirst)
                {
                    builder.Append(options.ItemSeparator);
                    if (options.WriteIndented)
                        builder.AppendLine();
                }

                builder
                    .Append(IndentString(options, indentDepth))
                    .Append(WriteAsString(options, keyType.ToStringInvariant(key)))
                    .Append(options.KeyValueSeparator);

                if (options.WriteIndented)
                    builder.Append(' ');

                builder.Append(Serialize(value, options, stackDepth + 1, indentDepth + 1));

                isFirst = false;
            }

            EndObject(options, builder, indentDepth);
            return true;
        }

        private static bool TryWriteAsArray(StringBuilder builder, object instance, TextSerializerOptions options, int stackDepth, int indentDepth)
        {
            if (instance is not IEnumerable collection)
                return false;

            var isFirst = true;
            builder.Append(options.ArrayOpener);
            foreach (var item in collection)
            {
                if (!isFirst)
                {
                    builder.Append(options.ItemSeparator);
                    if (options.WriteIndented)
                        builder.Append(' ');
                }

                builder.Append(Serialize(item, options, stackDepth + 1, indentDepth));
                isFirst = false;
            }

            builder.Append(options.ArrayCloser);
            return true;
        }

        private static string WriteAsObject(StringBuilder builder, ITypeProxy proxy, object instance, TextSerializerOptions options, int stackDepth, int indentDepth)
        {
            var isFirst = true;
            BeginObject(options, builder);
            foreach (var property in proxy.Properties.Values)
            {
                if (!property.CanRead)
                    continue;

                if (!isFirst)
                {
                    builder.Append(options.ItemSeparator);
                    if (options.WriteIndented)
                        builder.AppendLine();
                }

                if (property.TryGetValue(instance, out var value))
                    builder
                        .Append(IndentString(options, indentDepth))
                        .Append(WriteAsString(options, property.PropertyName))
                        .Append(options.KeyValueSeparator);

                if (options.WriteIndented)
                    builder.Append(' ');

                builder.Append(Serialize(value, options, stackDepth + 1, indentDepth + 1));

                isFirst = false;
            }

            EndObject(options, builder, indentDepth);
            return builder.ToString();
        }

        private static ReadOnlySpan<char> IndentString(TextSerializerOptions options, int indentDepth) => options.WriteIndented
            ? new(' ', indentDepth * options.IndentSpaces)
            : string.Empty;

        private static void BeginObject(TextSerializerOptions options, StringBuilder builder)
        {
            builder.Append(options.ObjectOpener.AsSpan());

            if (options.WriteIndented)
                builder.AppendLine();
        }

        private static void EndObject(TextSerializerOptions options, StringBuilder builder, int indentDepth)
        {
            indentDepth = indentDepth > 0
                ? indentDepth - 1
                : 0;

            if (options.WriteIndented)
                builder
                    .AppendLine()
                    .Append(IndentString(options, indentDepth));

            builder.Append(options.ObjectCloser);
        }
    }
}
