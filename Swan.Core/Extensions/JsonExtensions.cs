using System.Text.Json;

namespace Swan.Extensions
{
    public static class JsonExtensions
    {
        private static readonly JsonSerializerOptions ToJsonIndentedOptions = new(JsonSerializerDefaults.General)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };

        private static readonly JsonSerializerOptions ToJsonFlatOptions = new(JsonSerializerDefaults.General)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
        };

        /// <summary>
        /// Outputs JSON string representing this object.
        /// </summary>
        /// <param name="this">The object.</param>
        /// <param name="indent">if set to <c>true</c> format the output.</param>
        /// <returns>A <see cref="string" /> that represents the current object.</returns>
        public static string ToJson(this object? @this, bool indent = false) =>
            @this == null ? string.Empty : JsonSerializer.Serialize(@this, @this.GetType(), indent ? ToJsonIndentedOptions : ToJsonFlatOptions);

        /// <summary>
        /// Outputs JSON string representing this object.
        /// </summary>
        /// <param name="this">The object.</param>
        /// <param name="options">Specify custom serializer options.</param>
        /// <returns>A <see cref="string" /> that represents the current object.</returns>
        public static string ToJson(this object? @this, JsonSerializerOptions options) =>
            @this == null ? string.Empty : JsonSerializer.Serialize(@this, @this.GetType(), options);
    }
}
