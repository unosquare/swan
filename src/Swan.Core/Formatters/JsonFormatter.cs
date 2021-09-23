using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Swan.Formatters
{
    /// <summary>
    /// Provides extension methods for JSON serialization and deserialization.
    /// </summary>
    public static partial class JsonFormatter
    {
        private static readonly JsonSerializerOptions JsonFlatOptions = new(JsonSerializerDefaults.General)
        {
            PropertyNamingPolicy = null, // Pascal case
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            ReferenceHandler = null, // TODO: In .net 6.0 change this to IgnoreCycles
            IgnoreReadOnlyFields = false,
            IgnoreReadOnlyProperties = false,
            IncludeFields = true
        };

        private static readonly JsonSerializerOptions JsonIndentedOptions = new(JsonFlatOptions)
        {
            WriteIndented = true
        };

        /// <summary>
        /// Outputs JSON string representing this object.
        /// </summary>
        /// <param name="this">The object.</param>
        /// <param name="options">Specify custom serializer options.</param>
        /// <returns>A <see cref="string" /> that represents the current object.</returns>
        public static string JsonSerialize(this object? @this, JsonSerializerOptions options) =>
            @this == null ? string.Empty : JsonSerializer.Serialize(@this, @this.GetType(), options);

        /// <summary>
        /// Outputs JSON string representing this object.
        /// </summary>
        /// <param name="this">The object.</param>
        /// <param name="indent">if set to <c>true</c> format the output.</param>
        /// <returns>A <see cref="string" /> that represents the current object.</returns>
        public static string JsonSerialize(this object? @this, bool indent = default) =>
            JsonSerialize(@this, indent ? JsonIndentedOptions : JsonFlatOptions);

        /// <summary>
        /// Deserializes a JSON string into an object of the given type.
        /// </summary>
        /// <param name="this">The string containing the JSON.</param>
        /// <param name="type">The type to deserialize into.</param>
        /// <param name="options">The optional serializer options.</param>
        /// <returns>The deserialized object.</returns>
        public static object? JsonDeserialize(this string @this, Type type, JsonSerializerOptions? options = default) =>
            type == null
                ? throw new ArgumentNullException(nameof(type))
                : JsonSerializer.Deserialize(@this, type, options ?? JsonFlatOptions);

        /// <summary>
        /// Deserializes a JSON stream of UTF8 bytes into a dynamic object.
        /// </summary>
        /// <param name="this">The stream of bytes in UTF8.</param>
        /// <param name="type">The type to deserialize into.</param>
        /// <param name="options">Optional JSON serializer options.</param>
        /// <returns>The deserialized object.</returns>
        public static async Task<object?> JsonDeserializeAsync(this Stream @this, Type type, JsonSerializerOptions? options = default)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return await JsonSerializer
                .DeserializeAsync(@this, type, options ?? JsonFlatOptions)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Deserializes a JSON string into an object of the given type.
        /// </summary>
        /// <param name="this">The string containing the JSON.</param>
        /// <param name="options">The optional serializer options.</param>
        /// <returns>The deserialized object.</returns>
        public static T? JsonDeserialize<T>(this string @this, JsonSerializerOptions? options = default) =>
            JsonSerializer.Deserialize<T>(@this, options ?? JsonFlatOptions);

        /// <summary>
        /// Deserializes a JSON stream of UTF8 bytes into a dynamic object.
        /// </summary>
        /// <param name="this">The stream of bytes in UTF8.</param>
        /// <param name="options">Optional JSON serializer options.</param>
        /// <returns>The deserialized object.</returns>
        public static async Task<T?> JsonDeserializeAsync<T>(this Stream @this, JsonSerializerOptions? options = default)
        {
            return await JsonSerializer
                .DeserializeAsync<T>(@this, options ?? JsonFlatOptions)
                .ConfigureAwait(false);
        }
    }
}
