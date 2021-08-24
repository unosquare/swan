using Swan.Types;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Swan.Extensions
{
    public static class JsonExtensions
    {
        private static readonly JsonSerializerOptions ToJsonIndentedOptions = new(JsonSerializerDefaults.General)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.Preserve
        };

        private static readonly JsonSerializerOptions ToJsonFlatOptions = new(JsonSerializerDefaults.General)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.Preserve
        };

        /// <summary>
        /// Outputs JSON string representing this object.
        /// </summary>
        /// <param name="this">The object.</param>
        /// <param name="indent">if set to <c>true</c> format the output.</param>
        /// <returns>A <see cref="string" /> that represents the current object.</returns>
        public static string JsonSerialize(this object? @this, bool indent = false) =>
            @this == null ? string.Empty : JsonSerializer.Serialize(@this, @this.GetType(), indent ? ToJsonIndentedOptions : ToJsonFlatOptions);

        /// <summary>
        /// Outputs JSON string representing this object.
        /// </summary>
        /// <param name="this">The object.</param>
        /// <param name="options">Specify custom serializer options.</param>
        /// <returns>A <see cref="string" /> that represents the current object.</returns>
        public static string JsonSerialize(this object? @this, JsonSerializerOptions options) =>
            @this == null ? string.Empty : JsonSerializer.Serialize(@this, @this.GetType(), options);

        /// <summary>
        /// Deserializes a JSON string into a dynamic object.
        /// </summary>
        /// <param name="this">The string to parse. Has to be a valid JSON document.</param>
        /// <param name="options">Option JSON document options.</param>
        /// <param name="valueParser">Optional JSON element value parser.</param>
        /// <returns></returns>
        public static dynamic? JsonDeserialize(this string @this, JsonDocumentOptions options = default, Func<JsonElement, object?>? valueParser = default)
        {
            if (string.IsNullOrWhiteSpace(@this))
                return null;

            var jsonDocument = JsonDocument.Parse(@this, options);
            return new JsonDynamicObject(jsonDocument.RootElement, valueParser);
        }

        public static async Task<dynamic?> JsonDeserializeAsync(this Stream @this, JsonDocumentOptions options = default, Func<JsonElement, object?>? valueParser = default)
        {
            if (@this is null)
                return null;

            var jsonDocument = await JsonDocument
                .ParseAsync(@this, options)
                .ConfigureAwait(false);
            
            return new JsonDynamicObject(jsonDocument.RootElement, valueParser);
        }

        public static object? JsonDeserialize(this string @this, Type type, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Deserialize(@this, type, options);
        }

        public static async Task<object?> JsonDeserializeAsync(this Stream @this, Type type, JsonSerializerOptions? options = null)
        {
            return await JsonSerializer
                .DeserializeAsync(@this, type, options)
                .ConfigureAwait(false);
        }

        public static T? JsonDeserialize<T>(this string @this, JsonSerializerOptions? options = null)
        {
            return JsonSerializer.Deserialize<T>(@this, options);
        }

        public static async Task<T?> JsonDeserializeAsync<T>(this Stream @this, Type type, JsonSerializerOptions? options = null)
        {
            return await JsonSerializer
                .DeserializeAsync<T>(@this, options)
                .ConfigureAwait(false);
        }
    }
}
