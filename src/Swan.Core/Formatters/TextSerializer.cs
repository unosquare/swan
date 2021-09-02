using Swan.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private readonly TextSerializerOptions Options;

        private TextSerializer(ITypeProxy proxy, int stackDepth, object? instance, TextSerializerOptions options)
        {
            Proxy = proxy;
            StackDepth = stackDepth;
            Instance = instance;
            Options = options;
        }

        public static string Serialize(object? instance, TextSerializerOptions? options = null)
        {
            return Serialize(instance, 0, options);
        }

        private static string Serialize(object? instance, int stackDepth, TextSerializerOptions? options)
        {
            options ??= TextSerializerOptions.JsonPrettyPrint;

            if (instance is null) return options.NullLiteral;
            var serializer = new TextSerializer(instance.GetType().TypeInfo(), stackDepth, instance, options);
            return serializer.WriteObject();
        }

        private bool CanWriteAsDictionary(out ITypeProxy? keyType, out ITypeProxy? valueType, out ITypeProxy? constructedType)
        {
            keyType = null;
            valueType = null;
            constructedType = null;

            var genericDictionary = Proxy.Interfaces
                .FirstOrDefault(c => c.IsGenericType && c.GetGenericTypeDefinition() == typeof(IDictionary<,>));

            if (genericDictionary is null || genericDictionary.GenericTypeArguments.Length < 2)
                return false;

            var preKeyType = genericDictionary.GenericTypeArguments[0].TypeInfo();
            if (!preKeyType.IsBasicType)
                return false;

            keyType = preKeyType;
            valueType = genericDictionary.GenericTypeArguments[1].TypeInfo();

            constructedType = typeof(IDictionary<,>).MakeGenericType(keyType.ProxiedType, valueType.ProxiedType).TypeInfo();

            return true;
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
                    _ => element.ToString() ?? Options.NullLiteral,
                };
            }

            var builder = new StringBuilder();

            if (CanWriteAsDictionary(out var keyType, out _, out var dictionaryType))
            {
                var keys = dictionaryType!.Properties[nameof(IDictionary.Keys)].GetValue(Instance) as IEnumerable;
                var values = dictionaryType.Properties[nameof(IDictionary.Values)].GetValue(Instance) as IEnumerable;
                var valuesEnumerator = values!.GetEnumerator();

                var isFirst = true;
                Options.OpenObject(builder);
                foreach (var key in keys!)
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
                        .Append($"{Options.IndentString(StackDepth)}{QuotedJsonString(keyType!.ToStringInvariant(key))}")
                        .Append(Options.PropertySeparation)
                        .Append($"{Serialize(value, StackDepth + 1, Options)}");

                    isFirst = false;
                }

                Options.CloseObject(builder, StackDepth);

                return builder.ToString();
            }

            if (Instance is IEnumerable collection)
            {
                var isFirst = true;

                builder.Append(Options.OpenArraySequence);
                foreach (var item in collection)
                {
                    if (!isFirst)
                    {
                        builder.Append(Options.ItemSeparator);
                        if (Options.WriteIndented)
                            builder.Append(' ');
                    }

                    builder.Append(Serialize(item, StackDepth + 1, Options));
                    isFirst = false;
                }

                builder.Append(Options.CloseArraySequence);
                return builder.ToString();
            }

            {
                var isFirst = true;
                Options.OpenObject(builder);
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
                        builder.Append($"{Options.IndentString(StackDepth)}{QuotedJsonString(property.PropertyName)}")
                            .Append(Options.PropertySeparation)
                            .Append(Serialize(value, StackDepth + 1, Options));

                    isFirst = false;
                }

                Options.CloseObject(builder, StackDepth);
            }

            return builder.ToString();
        }

        private static string QuotedJsonString(ReadOnlySpan<char> str)
        {
            return $"\"{JsonEncode(str)}\"";
        }

        private static string JsonEncode(ReadOnlySpan<char> value) =>
            Encoding.UTF8.GetString(JsonEncodedText.Encode(value).EncodedUtf8Bytes);

    }
}
