using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Swan.Formatters
{
    public static partial class JsonFormatter
    {
        /// <summary>
        /// Deserializes a JSON string into a dynamic object.
        /// </summary>
        /// <param name="this">The string to parse. Has to be a valid JSON document.</param>
        /// <param name="options">Optional JSON document options.</param>
        /// <param name="valueParser">Optional JSON element value parser.</param>
        /// <returns>The deserialized dynamic object.</returns>
        public static dynamic? JsonDeserialize(this string @this, JsonDocumentOptions options = default, Func<JsonElement, object?>? valueParser = default)
        {
            if (string.IsNullOrWhiteSpace(@this))
                return null;

            using var jsonDocument = JsonDocument.Parse(@this, options);
            var jsonObject = new JsonDynamicObject(jsonDocument.RootElement, valueParser);
            return jsonObject.Materialize();
        }

        /// <summary>
        /// Deserializes a JSON stream of UTF8 bytes into a dynamic object.
        /// </summary>
        /// <param name="this">The stream of bytes in UTF8.</param>
        /// <param name="options">Optional JSON document options.</param>
        /// <param name="valueParser">Optional JSON element value parser.</param>
        /// <returns>The deserialized dynamic object.</returns>
        public static async Task<dynamic?> JsonDeserializeAsync(this Stream @this, JsonDocumentOptions options = default, Func<JsonElement, object?>? valueParser = default)
        {
            if (@this is null)
                return null;

            using var jsonDocument = await JsonDocument
                .ParseAsync(@this, options)
                .ConfigureAwait(false);

            var jsonObject = new JsonDynamicObject(jsonDocument.RootElement, valueParser);
            return jsonObject.Materialize();
        }
    }
}
