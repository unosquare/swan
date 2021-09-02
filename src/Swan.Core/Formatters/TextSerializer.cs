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
    public class TextSerializer
    {
        private readonly ITypeProxy Proxy;
        private readonly object Instance;
        private readonly int StackDepth;
        private readonly int IndentDepth;
        private readonly TextSerializerOptions Options;

        private TextSerializer(ITypeProxy proxy, object? instance, TextSerializerOptions options, int stackDepth, int indentDepth)
        {
            Proxy = proxy;
            StackDepth = stackDepth;
            IndentDepth = indentDepth;
            Instance = instance;
            Options = options;
        }

        public static string Serialize(object? instance, TextSerializerOptions? options = null)
        {
            return Serialize(instance, options, 0, 1);
        }

        private static string Serialize(object? instance, TextSerializerOptions? options, int stackDepth, int indentDepth)
        {
            options ??= TextSerializerOptions.JsonPrettyPrint;

            if (instance is null) return options.NullLiteral;
            var serializer = new TextSerializer(
                instance.GetType().TypeInfo(), instance, options, stackDepth, indentDepth);

            return serializer.WriteObject();
        }

        private bool TryWriteAsDictionary(StringBuilder builder)
        {
            var dictionaryType = Proxy.GenericDictionaryType;
            if (dictionaryType is null || !dictionaryType.GenericTypeArguments[0].IsBasicType)
                return false;

            // obtain keys and values
            var keys = dictionaryType.Properties[nameof(IDictionary.Keys)].GetValue(Instance) as IEnumerable;
            var values = dictionaryType.Properties[nameof(IDictionary.Values)].GetValue(Instance) as IEnumerable;

            // check that keys and values are in fact available.
            if (keys is null || values is null)
                return false;

            var valuesEnumerator = values.GetEnumerator();
            var keyType = dictionaryType.GenericTypeArguments[0];

            var isFirst = true;
            BeginObject(builder);
            foreach (var key in keys)
            {
                valuesEnumerator.MoveNext();
                var value = valuesEnumerator.Current;

                if (!isFirst)
                {
                    builder.Append(Options.ItemSeparator);
                    if (Options.WriteIndented)
                        builder.AppendLine();
                }

                builder
                    .Append($"{IndentString(IndentDepth)}{QuotedJsonString(keyType.ToStringInvariant(key))}")
                    .Append(KeyValueSeparation)
                    .Append($"{Serialize(value, Options, StackDepth + 1, IndentDepth + 1)}");

                isFirst = false;
            }

            EndObject(builder, IndentDepth);
            return true;
        }

        private bool TryWriteAsArray(StringBuilder builder)
        {
            if (Instance is not IEnumerable collection)
                return false;

            var isFirst = true;
            builder.Append(Options.ArrayOpener);
            foreach (var item in collection)
            {
                if (!isFirst)
                {
                    builder.Append(Options.ItemSeparator);
                    if (Options.WriteIndented)
                        builder.Append(' ');
                }

                builder.Append(Serialize(item, Options, StackDepth + 1, IndentDepth));
                isFirst = false;
            }

            builder.Append(Options.ArrayCloser);
            return true;
        }

        private void WriteAsObject(StringBuilder builder)
        {
            var isFirst = true;
            BeginObject(builder);
            foreach (var property in Proxy.Properties.Values)
            {
                if (!property.CanRead)
                    continue;

                if (!isFirst)
                {
                    builder.Append(Options.ItemSeparator);
                    if (Options.WriteIndented)
                        builder.AppendLine();
                }

                if (property.TryGetValue(Instance, out var value))
                    builder.Append($"{IndentString(IndentDepth)}{QuotedJsonString(property.PropertyName)}")
                        .Append(KeyValueSeparation)
                        .Append(Serialize(value, Options, StackDepth + 1, IndentDepth + 1));

                isFirst = false;
            }

            EndObject(builder, IndentDepth);
        }

        private string WriteObject()
        {
            if (Instance is null)
                return Options.NullLiteral;

            if (Instance is Type typeValue)
                return QuotedJsonString($"{typeValue}");

            if (Instance is bool boolValue)
                return boolValue ? Options.TrueLiteral : Options.FalseLiteral;

            if (Instance is string stringValue)
                return QuotedJsonString(stringValue);

            if (Proxy.IsNumeric)
                return Proxy.ToStringInvariant(Instance);

            if (Proxy.IsBasicType)
                return QuotedJsonString(Proxy.ToStringInvariant(Instance));

            if (Instance is JsonElement element)
            {
                return element.ValueKind switch
                {
                    JsonValueKind.Null => Options.NullLiteral,
                    JsonValueKind.False => Options.FalseLiteral,
                    JsonValueKind.True => Options.TrueLiteral,
                    JsonValueKind.Undefined => Options.NullLiteral,
                    JsonValueKind.Number => element.ToString() ?? "0",
                    JsonValueKind.String => QuotedJsonString(element.GetString()),
                    JsonValueKind.Array => Serialize(
                        new JsonDynamicObject(element).Materialize(), Options, StackDepth + 1, IndentDepth),
                    JsonValueKind.Object => Serialize(
                        new JsonDynamicObject(element).Materialize(), Options, StackDepth + 1, IndentDepth),
                    _ => element.ToString() ?? Options.NullLiteral,
                };
            }

            var builder = new StringBuilder();

            if (TryWriteAsDictionary(builder))
                return builder.ToString();

            if (TryWriteAsArray(builder))
                return builder.ToString();

            WriteAsObject(builder);
            return builder.ToString();
        }


        private void BeginObject(StringBuilder builder)
        {
            builder.Append(Options.ObjectOpener);

            if (Options.WriteIndented)
                builder.AppendLine();
        }

        private void EndObject(StringBuilder builder, int indentDepth)
        {
            indentDepth = indentDepth > 0
                ? indentDepth - 1
                : 0;

            if (Options.WriteIndented)
                builder.AppendLine().Append(IndentString(indentDepth));

            builder.Append(Options.ObjectCloser);
        }

        private string KeyValueSeparation => Options.WriteIndented
            ? $"{Options.KeyValueSeparator} "
            : Options.KeyValueSeparator;

        private string IndentString(int indentDepth) => Options.WriteIndented
            ? new(' ', indentDepth * Options.IndentSpaces)
            : string.Empty;

        private static string QuotedJsonString(ReadOnlySpan<char> str)
        {
            return $"\"{JsonEncode(str)}\"";
        }

        private static string JsonEncode(ReadOnlySpan<char> value) =>
            Encoding.UTF8.GetString(JsonEncodedText.Encode(value).EncodedUtf8Bytes);

    }
}
