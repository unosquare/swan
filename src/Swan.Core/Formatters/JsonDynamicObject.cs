namespace Swan.Formatters;

using System.Dynamic;
using System.Text.Json;

/// <summary>
/// When deserializing JSON without a specific type (i.e. object)
/// the default JSON deserializer returns <see cref="JsonElement"/> values.
/// This class materializes these elements by converting them to actual
/// properties with values.
/// </summary>
internal class JsonDynamicObject : DynamicObject
{
    private readonly Func<JsonElement, object?> ValueParser;
    private readonly JsonElement Element;

    /// <summary>
    /// Creates a new instance of <see cref="JsonDynamicObject"/>.
    /// </summary>
    /// <param name="element">The JSON element to read from.</param>
    /// <param name="valueParser">A custom value parser that converts JSON data types into CLR types.</param>
    public JsonDynamicObject(JsonElement element, Func<JsonElement, object?>? valueParser = default)
    {
        Element = element;
        ValueParser = valueParser ?? ParseJsonElement;
    }

    /// <summary>
    /// Materializes a JsonElement by traversing all of its nodes.
    /// </summary>
    /// <returns>A dynamic object with materialized values.</returns>
    public object? Materialize() =>
        Materialize(Element, ValueParser);

    /// <inheritdoc />
    public override IEnumerable<string> GetDynamicMemberNames()
    {
        if (Element.ValueKind != JsonValueKind.Object)
            throw new InvalidOperationException($"Element does not represent an object because its value kind is {Element.ValueKind}");

        foreach (var kvp in Element.EnumerateObject())
            yield return kvp.Name;
    }

    /// <inheritdoc />
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        result = null;

        if (binder == null)
            throw new ArgumentNullException(nameof(binder));

        if (!Element.TryGetProperty(binder.Name, out var jsonEl))
            return false;

        try
        {
            result = ValueParser(jsonEl);
        }
        catch
        {
            return false;
        }

        return true;
    }

    private object? ParseJsonElement(JsonElement jsonEl) =>
        jsonEl.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.False => false,
            JsonValueKind.True => true,
            JsonValueKind.Undefined => null,
            JsonValueKind.Number => ParseNumber(jsonEl),
            JsonValueKind.String => ParseString(jsonEl),
            JsonValueKind.Object => ParseObject(jsonEl),
            JsonValueKind.Array => ParseArray(jsonEl),
            _ => null,
        };

    private static object? ParseString(JsonElement element) =>
        element.TryGetDateTime(out var dtValue)
            ? dtValue
            : element.TryGetGuid(out var guidValue)
                ? guidValue
                : element.GetString();

    private static object ParseNumber(JsonElement element)
    {
        var elementString = element.GetRawText().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(elementString))
            return default(int);

        // Check if it's a floating point type
        return elementString.Contains('E', StringComparison.Ordinal)
            ? element.GetDouble()
            : elementString.Contains('.', StringComparison.Ordinal)
            ? element.TryGetDecimal(out var value)
                ? value
                : element.GetDouble()
            : element.TryGetInt32(out var intValue)
            ? intValue
            : element.TryGetInt64(out var longValue)
            ? longValue
            : element.TryGetUInt64(out var ulongValue)
            ? ulongValue
            : element.TryGetDecimal(out var decimalValue)
            ? decimalValue
            : element.GetDouble();
    }

    private JsonDynamicObject ParseObject(JsonElement element) =>
        new(element, ValueParser);

    private JsonDynamicObject[] ParseArray(JsonElement element) =>
        element.EnumerateArray()
            .Select(o => new JsonDynamicObject(o, ValueParser))
            .ToArray();

    private static object? Materialize(JsonElement element, Func<JsonElement, object?> valueParser)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                {
                    var result = new ExpandoObject();
                    foreach (var kvp in element.EnumerateObject())
                        result.TryAdd(kvp.Name, Materialize(kvp.Value, valueParser));

                    return result;
                }
            case JsonValueKind.Array:
                {
                    return element.EnumerateArray()
                        .Select(arrayElement => Materialize(arrayElement, valueParser))
                        .ToArray();
                }
            default:
                return valueParser.Invoke(element);
        }
    }
}

