using System;
using System.Dynamic;
using System.Linq;
using System.Text.Json;

namespace Swan.Types
{
    /// <summary>
    /// A dynamic object 
    /// </summary>
    public class JsonDynamicObject : DynamicObject
    {
        private readonly Func<JsonElement, object?> ValueParser;

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
        /// Gets the backing json element.
        /// </summary>
        public JsonElement Element { get; }

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
                JsonValueKind.Object => new JsonDynamicObject(jsonEl, ValueParser),
                JsonValueKind.Array => jsonEl.EnumerateArray()
                    .Select(o => new JsonDynamicObject(o, ValueParser))
                    .ToArray(),
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
    }
}
