﻿namespace Swan.Formatters;

using Collections;
using Extensions;
using Reflection;
using System.Text.Json;

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
        var stackTable = new StackTable();
        return Serialize(instance, options, stackTable, 0, 1);
    }

    private static string Serialize(object? instance,
        TextSerializerOptions options,
        StackTable stackTable,
        int stackDepth,
        int indentDepth)
    {
        if (instance is null)
            return options.NullLiteral;

        var proxy = instance.GetType().TypeInfo();

        switch (instance)
        {
            case Type typeValue:
                return WriteAsString(options, $"{typeValue}", false);
            case bool boolValue:
                return boolValue ? options.TrueLiteral : options.FalseLiteral;
            case string stringValue:
                return WriteAsString(options, stringValue, false);
        }

        if (proxy.IsNumeric)
            return proxy.ToStringInvariant(instance);

        if (proxy.IsBasicType)
            return WriteAsString(options, proxy.ToStringInvariant(instance), false);

        if (instance is JsonElement element)
            return WriteJsonElement(element, options, stackTable, stackDepth, indentDepth);

        var builder = new StringBuilder(256);

        return TryWriteAsDictionary(builder, proxy, instance, options, stackTable, stackDepth, indentDepth)
            ? builder.ToString()
            : TryWriteAsArray(builder, instance, options, stackTable, stackDepth, indentDepth)
                ? builder.ToString()
                : WriteAsObject(builder, proxy, instance, options, stackTable, stackDepth, indentDepth);
    }

    private static bool WillIncrementStack(object? value)
    {
        if (value is null or Type or string)
            return false;

        var proxy = value.GetType().TypeInfo();

        if (proxy.IsNumeric || proxy.IsBasicType)
            return false;

        if (value is JsonElement element)
            return element.ValueKind switch
            {
                JsonValueKind.Object or JsonValueKind.Array => true,
                _ => false
            };

        return true;
    }

    private static string WriteAsString(TextSerializerOptions options, string value, bool isKeyName)
    {
        var stringValue = isKeyName
            ? FormatKeyName(value, options)
            : value;

        var encodedValue = Encoding.UTF8.GetString(JsonEncodedText.Encode(stringValue).EncodedUtf8Bytes);
        var quotedValue = !isKeyName || options.QuotePropertyNames
            ? $"{options.StringQuotation}{encodedValue.Truncate(options.ValueMaxLength)}{options.StringQuotation}"
            : encodedValue;

        return !isKeyName || !options.KeyValuePadding.HasValue
            ? quotedValue
            : options.KeyValuePadding > 0
                ? quotedValue.PadLeft(options.KeyValuePadding.Value)
                : options.KeyValuePadding < 0
                    ? quotedValue.PadRight(Math.Abs(options.KeyValuePadding.Value))
                    : quotedValue;
    }

    private static string WriteJsonElement(JsonElement element, TextSerializerOptions options, StackTable stackTable,
        int stackDepth, int indentDepth) =>
        element.ValueKind switch
        {
            JsonValueKind.Null => options.NullLiteral,
            JsonValueKind.False => options.FalseLiteral,
            JsonValueKind.True => options.TrueLiteral,
            JsonValueKind.Undefined => options.NullLiteral,
            JsonValueKind.Number => element.ToString() ?? "0",
            JsonValueKind.String => WriteAsString(options, element.GetString()!, false),
            JsonValueKind.Array => Serialize(
                new JsonDynamicObject(element).Materialize(), options, stackTable, stackDepth + 1, indentDepth),
            JsonValueKind.Object => Serialize(
                new JsonDynamicObject(element).Materialize(), options, stackTable, stackDepth + 1, indentDepth),
            _ => element.ToString() ?? options.NullLiteral,
        };

    private static bool TryWriteAsDictionary(StringBuilder builder, ITypeInfo proxy, object instance,
        TextSerializerOptions options, StackTable stackTable, int stackDepth, int indentDepth)
    {
        if (!CollectionProxy.TryCreate(instance, out var dictionary) || dictionary.IsDictionary == false)
            return false;

        var isFirst = true;
        stackTable.AddReference(instance);

        BeginObject(options, $"({proxy.FullName})", builder);

        foreach (dynamic kvp in dictionary!)
        {
            if (stackDepth >= options.MaxStackDepth && WillIncrementStack(kvp.Value))
                continue;

            if (options.IgnoreRepeatedReferences && stackTable.HasReference(kvp.Value))
                continue;

            if (!isFirst)
            {
                builder.Append(options.PropertySeparator);
                if (options.WriteIndented)
                    builder.AppendLine();
            }

            builder
                .Append(IndentString(options, indentDepth))
                .Append(WriteAsString(options, dictionary.KeysType.ToStringInvariant(kvp.Key), true))
                .Append(options.KeyValueSeparator);

            if (options.WriteIndented)
                builder.Append(' ');

            builder.Append(Serialize(kvp.Value, options, stackTable, stackDepth + 1, indentDepth + 1));

            isFirst = false;
        }

        EndObject(options, builder, indentDepth);

        return true;
    }

    private static bool TryWriteAsArray(StringBuilder builder, object instance, TextSerializerOptions options,
        StackTable stackTable, int stackDepth, int indentDepth)
    {
        if (instance is not IEnumerable collection)
            return false;

        var isFirst = true;
        stackTable.AddReference(instance);

        builder.Append(options.ArrayOpener);
        foreach (var value in collection)
        {
            if (stackDepth >= options.MaxStackDepth && WillIncrementStack(value))
                continue;

            if (options.IgnoreRepeatedReferences && stackTable.HasReference(value))
                continue;

            if (!isFirst)
            {
                builder.Append(options.ArrayItemSeparator);
                if (options.WriteIndented)
                    builder.Append(' ');
            }

            builder.Append(Serialize(value, options, stackTable, stackDepth + 1, indentDepth));
            isFirst = false;
        }

        builder.Append(options.ArrayCloser);
        return true;
    }

    private static string WriteAsObject(StringBuilder builder, ITypeInfo proxy, object instance,
        TextSerializerOptions options, StackTable stackTable, int stackDepth, int indentDepth)
    {
        var isFirst = true;
        stackTable.AddReference(instance);

        BeginObject(options, $"({proxy.FullName})", builder);

        foreach (var property in proxy.Properties.Values)
        {
            if (!property.CanRead)
                continue;

            if (!property.TryRead(instance, out var value))
                continue;

            if (stackDepth >= options.MaxStackDepth && WillIncrementStack(value))
                continue;

            if (options.IgnoreRepeatedReferences && stackTable.HasReference(value))
                continue;

            if (!isFirst)
            {
                builder.Append(options.PropertySeparator);
                if (options.WriteIndented)
                    builder.AppendLine();
            }

            builder
                .Append(IndentString(options, indentDepth))
                .Append(WriteAsString(options, property.PropertyName, true))
                .Append(options.KeyValueSeparator);

            if (options.WriteIndented)
                builder.Append(' ');

            builder.Append(Serialize(value, options, stackTable, stackDepth + 1, indentDepth + 1));

            isFirst = false;
        }

        EndObject(options, builder, indentDepth);
        return builder.ToString();
    }

    private static string IndentString(TextSerializerOptions options, int indentDepth) => options.WriteIndented
        ? new(' ', indentDepth * options.IndentSpaces)
        : string.Empty;

    private static string FormatKeyName(string name, TextSerializerOptions options) =>
        string.IsNullOrEmpty(name)
            ? name
            : options.UseCamelCase && char.IsLetter(name[0]) && !char.IsLower(name[0])
                ? char.ToLowerInvariant(name[0]) + new string(name.Length > 1 ? name[1..] : string.Empty)
                : name;

    private static void BeginObject(TextSerializerOptions options, string typeName, StringBuilder builder)
    {
        builder.Append(options.ObjectOpener);

        if (options.OutputTypeNames && !string.IsNullOrEmpty(typeName))
            builder.Append(typeName);

        if (options.WriteIndented)
            builder.AppendLine();
    }

    private static void EndObject(TextSerializerOptions options, StringBuilder builder, int indentDepth)
    {
        if (options?.ObjectCloser is null || options.ObjectCloser.Length <= 0)
            return;

        indentDepth = indentDepth > 0
            ? indentDepth - 1
            : 0;

        if (options.WriteIndented)
            builder
                .AppendLine()
                .Append(IndentString(options, indentDepth));

        builder.Append(options.ObjectCloser);
    }

    private class StackTable : Dictionary<object, int>
    {
        public StackTable()
            : base(256)
        {
            // placeholder
        }

        public void AddReference(object? instance)
        {
            if (instance is null)
                return;

            if (!ContainsKey(instance))
                this[instance] = 0;

            this[instance] += 1;
        }

        public bool HasReference(object? instance) =>
            instance is not null && TryGetValue(instance, out var count) && count > 0;
    }
}
