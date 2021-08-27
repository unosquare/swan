using Swan.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Swan.Formatters
{
    /// <summary>
    /// Provides extension methods for JSON serialization and deserialization.
    /// </summary>
    public static class JsonFormatter
    {
        private static readonly Type DefaultDeserializationType = typeof(Dictionary<string, object>);

        private static readonly JsonSerializerOptions ToJsonIndentedOptions = new(JsonSerializerDefaults.General)
        {
            PropertyNamingPolicy = null, // Pascal case
            WriteIndented = true,
            ReferenceHandler = null,
            IgnoreReadOnlyFields = false,
            IgnoreReadOnlyProperties = false,
        };

        private static readonly JsonSerializerOptions ToJsonFlatOptions = new(JsonSerializerDefaults.General)
        {
            PropertyNamingPolicy = null, // Pascal case
            WriteIndented = false,
            ReferenceHandler = null,
            IgnoreReadOnlyFields = false,
            IgnoreReadOnlyProperties = false,
        };

        /// <summary>
        /// Outputs JSON string representing this object.
        /// </summary>
        /// <param name="this">The object.</param>
        /// <param name="indent">if set to <c>true</c> format the output.</param>
        /// <returns>A <see cref="string" /> that represents the current object.</returns>
        public static string JsonSerialize(this object? @this, bool indent = default) =>
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
        /// <param name="options">Optional JSON document options.</param>
        /// <param name="valueParser">Optional JSON element value parser.</param>
        /// <returns>The deserialized dynamic object.</returns>
        public static dynamic? JsonDynamicDeserialize(this string @this, JsonDocumentOptions options = default, Func<JsonElement, object?>? valueParser = default)
        {
            if (string.IsNullOrWhiteSpace(@this))
                return null;

            var jsonDocument = JsonDocument.Parse(@this, options);
            return new JsonDynamicObject(jsonDocument.RootElement, valueParser);
        }

        /// <summary>
        /// Deserializes a JSON stream of UTF8 bytes into a dynamic object.
        /// </summary>
        /// <param name="this">The stream of bytes in UTF8.</param>
        /// <param name="options">Optional JSON document options.</param>
        /// <param name="valueParser">Optional JSON element value parser.</param>
        /// <returns>The deserialized dynamic object.</returns>
        public static async Task<dynamic?> JsonDynamicDeserializeAsync(this Stream @this, JsonDocumentOptions options = default, Func<JsonElement, object?>? valueParser = default)
        {
            if (@this is null)
                return null;

            var jsonDocument = await JsonDocument
                .ParseAsync(@this, options)
                .ConfigureAwait(false);
            
            return new JsonDynamicObject(jsonDocument.RootElement, valueParser);
        }

        /// <summary>
        /// Deserializes a JSON string into an object of the given type.
        /// </summary>
        /// <param name="this">The string containing the JSON.</param>
        /// <param name="type">The type to deserialize into.</param>
        /// <param name="options">The optional serializer options.</param>
        /// <returns>The deserialized object.</returns>
        public static object? JsonDeserialize(this string @this, Type type, JsonSerializerOptions? options = default) =>
            type == null || !type.TypeInfo().CanCreateInstance
                ? throw new ArgumentException("The provided type must not be null and needs a parameterless constructor.", nameof(type))
                : JsonSerializer.Deserialize(@this, type ?? DefaultDeserializationType, options);

        /// <summary>
        /// Deserializes a JSON stream of UTF8 bytes into a dynamic object.
        /// </summary>
        /// <param name="this">The stream of bytes in UTF8.</param>
        /// <param name="type">The type to deserialize into.</param>
        /// <param name="options">Optional JSON serializer options.</param>
        /// <returns>The deserialized object.</returns>
        public static async Task<object?> JsonDeserializeAsync(this Stream @this, Type type, JsonSerializerOptions? options = null)
        {
            if (type == null || !type.TypeInfo().CanCreateInstance)
                throw new ArgumentException("The provided type must not be null and needs a parameterless constructor.", nameof(type));

            return await JsonSerializer
                .DeserializeAsync(@this, type ?? DefaultDeserializationType, options)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Deserializes a JSON string into an object dictionary.
        /// </summary>
        /// <param name="this">The string containing the JSON.</param>
        /// <param name="options">The optional serializer options.</param>
        /// <returns>The deserialized object.</returns>
        public static IDictionary<string, object>? JsonDeserialize(this string @this, JsonSerializerOptions? options = default) =>
            JsonSerializer.Deserialize(@this, DefaultDeserializationType, options) as IDictionary<string, object>;

        /// <summary>
        /// Deserializes a JSON stream of UTF8 bytes into an object dictionary.
        /// </summary>
        /// <param name="this">The stream of bytes in UTF8.</param>
        /// <param name="options">Optional JSON serializer options.</param>
        /// <returns>The deserialized object.</returns>
        public static async Task<IDictionary<string, object>?> JsonDeserializeAsync(this Stream @this, JsonSerializerOptions? options = null)
        {
            return (await JsonSerializer
                .DeserializeAsync(@this, DefaultDeserializationType, options)
                .ConfigureAwait(false)) as IDictionary<string, object>;
        }

        /// <summary>
        /// Deserializes a JSON string into an object of the given type.
        /// </summary>
        /// <param name="this">The string containing the JSON.</param>
        /// <param name="options">The optional serializer options.</param>
        /// <returns>The deserialized object.</returns>
        public static T? JsonDeserialize<T>(this string @this, JsonSerializerOptions? options = default) =>
            JsonSerializer.Deserialize<T>(@this, options);

        /// <summary>
        /// Deserializes a JSON stream of UTF8 bytes into a dynamic object.
        /// </summary>
        /// <param name="this">The stream of bytes in UTF8.</param>
        /// <param name="options">Optional JSON serializer options.</param>
        /// <returns>The deserialized object.</returns>
        public static async Task<T?> JsonDeserializeAsync<T>(this Stream @this, JsonSerializerOptions? options = null)
        {
            return await JsonSerializer
                .DeserializeAsync<T>(@this, options)
                .ConfigureAwait(false);
        }
    }
}
