using System;
using System.Collections.Generic;
using System.Linq;
using Swan.Collections;

namespace Swan.Formatters
{
    /// <summary>
    /// A very simple, light-weight JSON library written by Mario
    /// to teach Geo how things are done
    /// 
    /// This is an useful helper for small tasks but it doesn't represent a full-featured
    /// serializer such as the beloved Json.NET.
    /// </summary>
    public static partial class Json
    {
        #region Constants 

        internal const string AddMethodName = "Add";

        private const char OpenObjectChar = '{';
        private const char CloseObjectChar = '}';

        private const char OpenArrayChar = '[';
        private const char CloseArrayChar = ']';

        private const char FieldSeparatorChar = ',';
        private const char ValueSeparatorChar = ':';

        private const char StringEscapeChar = '\\';
        private const char StringQuotedChar = '"';

        private const string EmptyObjectLiteral = "{ }";
        private const string EmptyArrayLiteral = "[ ]";
        private const string TrueLiteral = "true";
        private const string FalseLiteral = "false";
        private const string NullLiteral = "null";

        #endregion

        private static readonly CollectionCacheRepository<string> IgnoredPropertiesCache = new CollectionCacheRepository<string>();

        #region Public API

        /// <summary>
        /// Serializes the specified object into a JSON string.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="format">if set to <c>true</c> it formats and indents the output.</param>
        /// <param name="typeSpecifier">The type specifier. Leave null or empty to avoid setting.</param>
        /// <param name="includeNonPublic">if set to <c>true</c> non-public getters will be also read.</param>
        /// <param name="includedNames">The included property names.</param>
        /// <param name="excludedNames">The excluded property names.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents the current object.
        /// </returns>
        /// <example>
        /// The following example describes how to serialize a simple object.
        /// <code>
        /// using Swan.Formatters;
        /// 
        /// class Example
        /// {
        ///     static void Main()
        ///     {
        ///         var obj = new { One = "One", Two = "Two" };
        ///         
        ///         var serial = Json.Serialize(obj); // {"One": "One","Two": "Two"}
        ///     }
        /// }
        /// </code>
        /// 
        /// The following example details how to serialize an object using the <see cref="JsonPropertyAttribute"/>.
        /// 
        /// <code>
        /// using Swan.Attributes;
        /// using Swan.Formatters;
        /// 
        /// class Example
        /// {
        ///     class JsonPropertyExample
        ///     {
        ///         [JsonProperty("data")]
        ///         public string Data { get; set; }
        ///         
        ///         [JsonProperty("ignoredData", true)]
        ///         public string IgnoredData { get; set; }
        ///     }
        ///     
        ///     static void Main()
        ///     {
        ///         var obj = new JsonPropertyExample() { Data = "OK", IgnoredData = "OK" };
        ///         
        ///         // {"data": "OK"}
        ///         var serializedObj = Json.Serialize(obj);
        ///     }
        /// }
        /// </code>
        /// </example>
        public static string Serialize(
            object? obj,
            bool format = false,
            string? typeSpecifier = null,
            bool includeNonPublic = false,
            string[]? includedNames = null,
            params string[] excludedNames) =>
            Serialize(obj, format, typeSpecifier, includeNonPublic, includedNames, excludedNames, null, JsonSerializerCase.None);

        /// <summary>
        /// Serializes the specified object into a JSON string.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="jsonSerializerCase">The json serializer case.</param>
        /// <param name="format">if set to <c>true</c> [format].</param>
        /// <param name="typeSpecifier">The type specifier.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents the current object.
        /// </returns>
        public static string Serialize(
            object? obj,
            JsonSerializerCase jsonSerializerCase,
            bool format = false,
            string? typeSpecifier = null) => Serialize(obj, format, typeSpecifier, false, null, null, null, jsonSerializerCase);

        /// <summary>
        /// Serializes the specified object into a JSON string.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="format">if set to <c>true</c> it formats and indents the output.</param>
        /// <param name="typeSpecifier">The type specifier. Leave null or empty to avoid setting.</param>
        /// <param name="includeNonPublic">if set to <c>true</c> non-public getters will be also read.</param>
        /// <param name="includedNames">The included property names.</param>
        /// <param name="excludedNames">The excluded property names.</param>
        /// <param name="parentReferences">The parent references.</param>
        /// <param name="jsonSerializerCase">The json serializer case.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents the current object.
        /// </returns>
        public static string Serialize(
            object? obj,
            bool format,
            string? typeSpecifier,
            bool includeNonPublic,
            string[]? includedNames,
            string[]? excludedNames,
            List<WeakReference>? parentReferences,
            JsonSerializerCase jsonSerializerCase)
        {
            if (obj != null && (obj is string || Definitions.AllBasicValueTypes.Contains(obj.GetType())))
            {
                return SerializePrimitiveValue(obj);
            }

            var options = new SerializerOptions(
                format,
                typeSpecifier,
                includedNames,
                Serializer.GetExcludedNames(obj?.GetType(), excludedNames),
                includeNonPublic,
                parentReferences,
                jsonSerializerCase);

            return Serializer.Serialize(obj, 0, options, excludedNames); 
        }

        /// <summary>
        /// Serializes the specified object using the SerializerOptions provided.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="options">The options.</param>
        /// <returns>
        /// A <see cref="string" /> that represents the current object.
        /// </returns>
        public static string Serialize(object? obj, SerializerOptions options) => Serializer.Serialize(obj, 0, options);

        /// <summary>
        /// Serializes the specified object only including the specified property names.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="format">if set to <c>true</c> it formats and indents the output.</param>
        /// <param name="includeNames">The include names.</param>
        /// <returns>A <see cref="string" /> that represents the current object.</returns>
        /// <example>
        /// The following example shows how to serialize a simple object including the specified properties.
        /// <code>
        /// using Swan.Formatters;
        /// 
        /// class Example
        /// {
        ///     static void Main()
        ///     {
        ///         // object to serialize
        ///         var obj = new { One = "One", Two = "Two", Three = "Three" };
        ///         
        ///         // the included names
        ///         var includedNames  = new[] { "Two", "Three" };
        ///         
        ///         // serialize only the included names
        ///         var data = Json.SerializeOnly(basicObject, true, includedNames); 
        ///         // {"Two": "Two","Three": "Three" }
        ///     }
        /// }
        /// </code>
        /// </example>
        public static string SerializeOnly(object? obj, bool format, params string[] includeNames)
            => Serialize(obj, new SerializerOptions(format, null, includeNames));

        /// <summary>
        /// Serializes the specified object excluding the specified property names.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="format">if set to <c>true</c> it formats and indents the output.</param>
        /// <param name="excludeNames">The exclude names.</param>
        /// <returns>A <see cref="string" /> that represents the current object.</returns>
        /// <example>
        /// The following code shows how to serialize a simple object excluding the specified properties.
        /// <code>
        /// using Swan.Formatters;
        /// 
        /// class Example
        /// {
        ///     static void Main()
        ///     {
        ///         // object to serialize
        ///         var obj = new { One = "One", Two = "Two", Three = "Three" };
        ///         
        ///         // the excluded names
        ///         var excludeNames = new[] { "Two", "Three" };
        ///         
        ///         // serialize excluding
        ///         var data = Json.SerializeExcluding(basicObject, false, includedNames); 
        ///         // {"One": "One"}
        ///     }
        /// }
        /// </code>
        /// </example>
        public static string SerializeExcluding(object? obj, bool format, params string[] excludeNames) 
            => Serializer.Serialize(obj, 0, new SerializerOptions(format, null, null), excludeNames);

        /// <summary>
        /// Deserializes the specified json string as either a Dictionary[string, object] or as a List[object]
        /// depending on the syntax of the JSON string.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <param name="jsonSerializerCase">The json serializer case.</param>
        /// <returns>
        /// Type of the current deserializes.
        /// </returns>
        /// <example>
        /// The following code shows how to deserialize a JSON string into a Dictionary.
        /// <code>
        /// using Swan.Formatters;
        /// class Example
        /// {
        /// static void Main()
        /// {
        /// // json to deserialize
        /// var basicJson = "{\"One\":\"One\",\"Two\":\"Two\",\"Three\":\"Three\"}";
        /// // deserializes the specified json into a Dictionary&lt;string, object&gt;.
        /// var data = Json.Deserialize(basicJson, JsonSerializerCase.None);
        /// }
        /// }
        /// </code></example>
        public static object? Deserialize(string json, JsonSerializerCase jsonSerializerCase) =>
            json == null
                ? throw new ArgumentNullException(nameof(json))
                : Converter.FromJsonResult(Deserializer.DeserializeInternal(json), jsonSerializerCase);

        /// <summary>
        /// Deserializes the specified json string as either a Dictionary[string, object] or as a List[object]
        /// depending on the syntax of the JSON string.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>
        /// Type of the current deserializes.
        /// </returns>
        /// <example>
        /// The following code shows how to deserialize a JSON string into a Dictionary.
        /// <code>
        /// using Swan.Formatters;
        /// class Example
        /// {
        /// static void Main()
        /// {
        /// // json to deserialize
        /// var basicJson = "{\"One\":\"One\",\"Two\":\"Two\",\"Three\":\"Three\"}";
        /// // deserializes the specified json into a Dictionary&lt;string, object&gt;.
        /// var data = Json.Deserialize(basicJson);
        /// }
        /// }
        /// </code></example>
        public static object? Deserialize(string json) =>
            json == null
                ? throw new ArgumentNullException(nameof(json))
                : Deserialize(json, JsonSerializerCase.None);

        /// <summary>
        /// Deserializes the specified JSON string and converts it to the specified object type.
        /// Non-public constructors and property setters are ignored.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <param name="json">The JSON string.</param>
        /// <param name="jsonSerializerCase">The JSON serializer case.</param>
        /// <returns>
        /// The deserialized specified type object.
        /// </returns>
        /// <example>
        /// The following code describes how to deserialize a JSON string into an object of type T.
        /// <code>
        /// using Swan.Formatters;
        /// class Example
        /// {
        /// static void Main()
        /// {
        /// // json type BasicJson to serialize
        /// var basicJson = "{\"One\":\"One\",\"Two\":\"Two\",\"Three\":\"Three\"}";
        /// // deserializes the specified string in a new instance of the type BasicJson.
        /// var data = Json.Deserialize&lt;BasicJson&gt;(basicJson);
        /// }
        /// }
        /// </code></example>
        public static T Deserialize<T>(string json, JsonSerializerCase jsonSerializerCase = JsonSerializerCase.None) =>
            json == null
                ? throw new ArgumentNullException(nameof(json))
                : (T)Deserialize(json, typeof(T), jsonSerializerCase: jsonSerializerCase);

        /// <summary>
        /// Deserializes the specified JSON string and converts it to the specified object type.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <param name="json">The JSON string.</param>
        /// <param name="includeNonPublic">if set to true, it also uses the non-public constructors and property setters.</param>
        /// <returns>
        /// The deserialized specified type object.
        /// </returns>
        public static T Deserialize<T>(string json, bool includeNonPublic) =>
            json == null
                ? throw new ArgumentNullException(nameof(json))
                : (T)Deserialize(json, typeof(T), includeNonPublic);

        /// <summary>
        /// Deserializes the specified JSON string and converts it to the specified object type.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <param name="resultType">Type of the result.</param>
        /// <param name="includeNonPublic">if set to true, it also uses the non-public constructors and property setters.</param>
        /// <param name="jsonSerializerCase">The json serializer case.</param>
        /// <returns>
        /// Type of the current conversion from json result.
        /// </returns>
        public static object? Deserialize(string json, Type resultType, bool includeNonPublic = false, JsonSerializerCase jsonSerializerCase = JsonSerializerCase.None) =>
            json == null
                ? throw new ArgumentNullException(nameof(json))
                : Converter.FromJsonResult(
                    Deserializer.DeserializeInternal(json),
                    jsonSerializerCase,
                    resultType,
                    includeNonPublic);

        #endregion

        #region Private API
        private static string SerializePrimitiveValue(object obj) =>
            obj switch
            {
                string stringValue => $"\"{stringValue}\"",
                bool boolValue => boolValue ? TrueLiteral : FalseLiteral,
                _ => obj.ToString()
            };

        #endregion
    }
}
