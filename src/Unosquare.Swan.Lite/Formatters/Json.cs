namespace Unosquare.Swan.Formatters
{
    using Reflection;
    using System;
    using Components;
    using System.Collections.Generic;
    using System.Linq;
    using Attributes;
#if NETSTANDARD1_3
    using System.Reflection;
#endif

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
        /// using Unosquare.Swan.Formatters;
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
        /// The following example details how to serialize an object using the <see cref="JsonPropertyAttribute"/>.
        /// <code>
        /// using Unosquare.Swan.Attributes;
        /// using Unosquare.Swan.Formatters;
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
            object obj,
            bool format = false,
            string typeSpecifier = null,
            bool includeNonPublic = false,
            string[] includedNames = null,
            params string[] excludedNames)
        {
            return Serialize(obj, format, typeSpecifier, includeNonPublic, includedNames, excludedNames, null);
        }

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
        /// <returns>
        /// A <see cref="System.String" /> that represents the current object.
        /// </returns>
        public static string Serialize(
            object obj,
            bool format,
            string typeSpecifier,
            bool includeNonPublic,
            string[] includedNames,
            string[] excludedNames,
            List<WeakReference> parentReferences)
        {
            if (obj != null && (obj is string || Definitions.AllBasicValueTypes.Contains(obj.GetType())))
            {
                return SerializePrimitiveValue(obj);
            }

            var options = new SerializerOptions(
                format,
                typeSpecifier,
                includedNames,
                GetExcludedNames(obj?.GetType(), excludedNames),
                includeNonPublic,
                parentReferences);

            return Serializer.Serialize(obj, 0, options);
        }

        /// <summary>
        /// Serializes the specified object only including the specified property names.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="format">if set to <c>true</c> it formats and indents the output.</param>
        /// <param name="includeNames">The include names.</param>
        /// <returns>A <see cref="System.String" /> that represents the current object.</returns>
        /// <example>
        /// The following example shows how to serialize a simple object including the specified properties.
        /// <code>
        /// using Unosquare.Swan.Formatters;
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
        public static string SerializeOnly(object obj, bool format, params string[] includeNames)
        {
            var options = new SerializerOptions(format, null, includeNames);

            return Serializer.Serialize(obj, 0, options);
        }

        /// <summary>
        /// Serializes the specified object excluding the specified property names.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="format">if set to <c>true</c> it formats and indents the output.</param>
        /// <param name="excludeNames">The exclude names.</param>
        /// <returns>A <see cref="System.String" /> that represents the current object.</returns>
        /// <example>
        /// The following code shows how to serialize a simple object exluding the specified properties.
        /// <code>
        /// using Unosquare.Swan.Formatters;
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
        public static string SerializeExcluding(object obj, bool format, params string[] excludeNames)
        {
            var options = new SerializerOptions(format, null, null, excludeNames);

            return Serializer.Serialize(obj, 0, options);
        }

        /// <summary>
        /// Deserializes the specified json string as either a Dictionary[string, object] or as a List[object]
        /// depending on the syntax of the JSON string.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns>Type of the current deserializes.</returns>
        /// <example>
        /// The following code shows how to deserialize a JSON string into a Dictionary.
        /// <code>
        /// using Unosquare.Swan.Formatters;
        /// 
        /// class Example
        /// {
        ///     static void Main()
        ///     {
        ///         // json to deserialize
        ///         var basicJson = "{\"One\":\"One\",\"Two\":\"Two\",\"Three\":\"Three\"}";
        ///         
        ///         // deserializes the specified json into a Dictionary&lt;string, object&gt;.
        ///         var data = Json.Deserialize(basicJson);
        ///     }
        /// }
        /// </code>
        /// </example>
        public static object Deserialize(string json) => Deserializer.DeserializeInternal(json);

        /// <summary>
        /// Deserializes the specified json string and converts it to the specified object type.
        /// Non-public constructors and property setters are ignored.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <param name="json">The json.</param>
        /// <returns>The deserialized specified type object.</returns>
        /// <example>
        /// The following code describes how to deserialize a JSON string into an object of type T.
        /// <code>
        /// using Unosquare.Swan.Formatters;
        /// 
        /// class Example
        /// {
        ///     static void Main()
        ///     {
        ///         // json type BasicJson to serialize
        ///         var basicJson = "{\"One\":\"One\",\"Two\":\"Two\",\"Three\":\"Three\"}";
        ///         
        ///         // deserializes the specified string in a new instance of the type BasicJson.
        ///         var data = Json.Deserialize&lt;BasicJson&gt;(basicJson);
        ///     }
        /// }
        /// </code>
        /// </example>
        public static T Deserialize<T>(string json) => (T)Deserialize(json, typeof(T));

        /// <summary>
        /// Deserializes the specified json string and converts it to the specified object type.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize.</typeparam>
        /// <param name="json">The json.</param>
        /// <param name="includeNonPublic">if set to true, it also uses the non-public constructors and property setters.</param>
        /// <returns>The deserialized specified type object.</returns>
        public static T Deserialize<T>(string json, bool includeNonPublic) => (T)Deserialize(json, typeof(T), includeNonPublic);

        /// <summary>
        /// Deserializes the specified json string and converts it to the specified object type.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="resultType">Type of the result.</param>
        /// <param name="includeNonPublic">if set to true, it also uses the non-public constructors and property setters.</param>
        /// <returns>Type of the current conversion from json result.</returns>
        public static object Deserialize(string json, Type resultType, bool includeNonPublic = false)
            => Converter.FromJsonResult(Deserializer.DeserializeInternal(json), resultType, includeNonPublic);

        #endregion

        #region Private API

        private static string[] GetExcludedNames(Type type, string[] excludedNames)
        {
            if (type == null)
                return excludedNames;

            var excludedByAttr = IgnoredPropertiesCache.Retrieve(type, t => t.GetProperties()
                .Where(x => AttributeCache.DefaultCache.Value.RetrieveOne<JsonPropertyAttribute>(x)?.Ignored == true)
                .Select(x => x.Name));

            if (excludedByAttr?.Any() != true)
                return excludedNames;

            return excludedNames?.Any(string.IsNullOrWhiteSpace) == true
                ? excludedByAttr.Intersect(excludedNames.Where(y => !string.IsNullOrWhiteSpace(y))).ToArray()
                : excludedByAttr.ToArray();
        }

        private static string SerializePrimitiveValue(object obj)
        {
            switch (obj)
            {
                case string stringValue:
                    return stringValue;
                case bool boolValue:
                    return boolValue ? TrueLiteral : FalseLiteral;
                default:
                    return obj.ToString();
            }
        }
        #endregion
    }
}
