using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text.Json;

namespace Swan.Formatters
{
    /// <summary>
    /// A dynamic object 
    /// </summary>
    internal class JsonDynamicObject : DynamicObject
    {
        private readonly Func<JsonElement, object?> _valueParser;
        private readonly JsonElement _element;

        /// <summary>
        /// Creates a new instance of <see cref="JsonDynamicObject"/>.
        /// </summary>
        /// <param name="element">The JSON element to read from.</param>
        /// <param name="valueParser">A custom value parser that converts JSON data types into CLR types.</param>
        public JsonDynamicObject(JsonElement element, Func<JsonElement, object?>? valueParser = default)
        {
            _element = element;
            _valueParser = valueParser ?? ParseJsonElement;
        }

        /// <summary>
        /// Materializes a JsonElement by traversing all of its nodes.
        /// </summary>
        /// <returns>A dynamic object with materialized values.</returns>
        public object? Materialize() =>
            Materialize(_element, _valueParser);

        /// <inheritdoc />
        public override IEnumerable<string> GetDynamicMemberNames()
        {
            if (_element.ValueKind != JsonValueKind.Object)
                throw new InvalidOperationException($"Element does not represent an object because its value kind is {_element.ValueKind}");

            foreach (var kvp in _element.EnumerateObject())
                yield return kvp.Name;
        }

        /// <inheritdoc />
        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            result = null;

            if (binder == null)
                throw new ArgumentNullException(nameof(binder));

            if (!_element.TryGetProperty(binder.Name, out var jsonEl))
                return false;

            try
            {
                result = _valueParser(jsonEl);
            }
            catch
            {
                return false;
            }

            return true;
        }

        private object? ParseJsonElement(JsonElement jsonEl)
        {
            return jsonEl.ValueKind switch
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
        }

        private static object? ParseString(JsonElement element)
        {
            return element.TryGetDateTime(out var dtValue)
                ? dtValue
                : element.TryGetGuid(out var guidValue)
                ? guidValue
                : element.GetString();
        }

        private static object ParseNumber(JsonElement element)
        {
            var elementString = element.GetRawText().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(elementString))
                return default(int);

            // Check if it's a floating point type
            if (elementString.Contains("E", StringComparison.Ordinal))
                return element.GetDouble();

            if (elementString.Contains(".", StringComparison.Ordinal))
            {
                return element.TryGetDecimal(out var value)
                    ? value
                    : element.GetDouble();
            }

            return
                element.TryGetInt32(out var intValue)
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
            new(element, _valueParser);

        private JsonDynamicObject[] ParseArray(JsonElement element) =>
            element.EnumerateArray()
                .Select(o => new JsonDynamicObject(o, _valueParser))
                .ToArray();

        private static object? Materialize(JsonElement element, Func<JsonElement, object?> valueParser)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                var result = new ExpandoObject();
                foreach (var kvp in element.EnumerateObject())
                    result.TryAdd(kvp.Name, Materialize(kvp.Value, valueParser));

                return result;
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                var result = new List<object?>();
                foreach (var arrayElement in element.EnumerateArray())
                    result.Add(Materialize(arrayElement, valueParser));

                return result.ToArray();
            }

            return valueParser.Invoke(element);
        }
    }
}
