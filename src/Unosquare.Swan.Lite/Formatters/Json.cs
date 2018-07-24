namespace Unosquare.Swan.Formatters
{
    using Reflection;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Attributes;

    /// <summary>
    /// A very simple, light-weight JSON library written by Mario
    /// to teach Geo how things are done
    /// 
    /// This is an useful helper for small tasks but it doesn't represent a full-featured
    /// serializer such as the beloved Json.NET
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

        private static readonly PropertyTypeCache PropertyTypeCache = new PropertyTypeCache();
        private static readonly FieldTypeCache FieldTypeCache = new FieldTypeCache();

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
        /// A <see cref="System.String" /> that represents the current object
        /// </returns>
        /// <exception cref="ArgumentException">You need to provide an object or array</exception>
        /// <example>
        /// The following example describes how to serialize a simple object
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
        /// The following example details how to serialize an object using the <see cref="JsonPropertyAttribute"/>
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
            string[] excludedNames = null)
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
        /// A <see cref="System.String" /> that represents the current object
        /// </returns>
        /// <exception cref="ArgumentException">You need to provide an object or array</exception>
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

            return Serializer.Serialize(obj, 0, format, typeSpecifier, includedNames, GeExcludedNames(obj, excludedNames), includeNonPublic, parentReferences);
        }

        /// <summary>
        /// Serializes the specified object only including the specified property names.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="format">if set to <c>true</c> it formats and indents the output.</param>
        /// <param name="includeNames">The include names.</param>
        /// <returns>A <see cref="System.String" /> that represents the current object</returns>
        /// <example>
        /// The following example shows how to serialize a simple object including the specified properties
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
            return Serializer.Serialize(obj, 0, format, null, includeNames, null, true);
        }

        /// <summary>
        /// Serializes the specified object excluding the specified property names.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="format">if set to <c>true</c> it formats and indents the output.</param>
        /// <param name="excludeNames">The exclude names.</param>
        /// <returns>A <see cref="System.String" /> that represents the current object</returns>
        /// <example>
        /// The following code shows how to serialize a simple object exluding the specified properties
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
        ///         
        ///     }
        /// }
        /// </code>
        /// </example>
        public static string SerializeExcluding(object obj, bool format, params string[] excludeNames)
        {
            return Serializer.Serialize(obj, 0, format, null, null, excludeNames, false);
        }

        /// <summary>
        /// Deserializes the specified json string as either a Dictionary[string, object] or as a List[object]
        /// depending on the syntax of the JSON string
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns>Type of the current deserializes</returns>
        /// <example>
        /// The following code shows how to deserialize a JSON string into a Dictionary
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
        ///         
        ///     }
        /// }
        /// </code>
        /// </example>
        public static object Deserialize(string json) => Deserializer.DeserializeInternal(json);

        /// <summary>
        /// Deserializes the specified json string and converts it to the specified object type.
        /// Non-public constructors and property setters are ignored.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize</typeparam>
        /// <param name="json">The json.</param>
        /// <returns>The deserialized specified type object</returns>
        /// <example>
        /// The following code describes how to deserialize a JSON string into an object of type T
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
        ///         
        ///     }
        /// }
        /// </code>
        /// </example>
        public static T Deserialize<T>(string json)
        {
            return (T)Deserialize(json, typeof(T));
        }

        /// <summary>
        /// Deserializes the specified json string and converts it to the specified object type.
        /// </summary>
        /// <typeparam name="T">The type of object to deserialize</typeparam>
        /// <param name="json">The json.</param>
        /// <param name="includeNonPublic">if set to true, it also uses the non-public constructors and property setters.</param>
        /// <returns>The deserialized specified type object</returns>
        public static T Deserialize<T>(string json, bool includeNonPublic)
        {
            return (T)Deserialize(json, typeof(T), includeNonPublic);
        }

        /// <summary>
        /// Deserializes the specified json string and converts it to the specified object type.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="resultType">Type of the result.</param>
        /// <param name="includeNonPublic">if set to true, it also uses the non-public constructors and property setters.</param>
        /// <returns>Type of the current conversion from json result</returns>
        public static object Deserialize(string json, Type resultType, bool includeNonPublic = false) 
            => ConvertFromJsonResult(Deserializer.DeserializeInternal(json), resultType, includeNonPublic);

        #endregion

        #region Private API
        
        private static string[] GeExcludedNames(object obj, string[] excludedNames)
        {
            var excludedByAttr = obj?.GetType().GetProperties()
                .Where(x => x?.GetCustomAttribute<JsonPropertyAttribute>()?.Ignored == true).Select(x => x.Name).ToArray();

            if (excludedByAttr?.Any() == true)
            {
                excludedNames = excludedNames == null
                    ? excludedByAttr.ToArray()
                    : excludedByAttr.Intersect(excludedNames).ToArray();
            }

            return excludedNames;
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

        private static object ConvertFromJsonResult(
            object source,
            Type targetType,
            bool includeNonPublic)
        {
            object nullRef = null;
            return ConvertFromJsonResult(source, targetType, ref nullRef, includeNonPublic);
        }

        /// <summary>
        /// Converts a json deserialized object (simple type, dictionary or list) to a new instance of the specified target type.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="targetInstance">An optional target instance. If null, we will attempt creation.</param>
        /// <param name="includeNonPublic">if set to <c>true</c> [include private].</param>
        /// <returns>Type of the current conversion from json result</returns>
        private static object ConvertFromJsonResult(
            object source,
            Type targetType,
            ref object targetInstance,
            bool includeNonPublic)
        {
            if (source == null) return targetType.GetDefault();

            object target = null;

            var sourceType = source.GetType();

            if (targetInstance != null) targetType = targetInstance.GetType();
            if (targetType == null || targetType == typeof(object)) targetType = sourceType;
            if (sourceType == targetType) return source;

            if (targetInstance == null)
            {
                // Try to create a default instance
                try
                {
                    source.CreateTarget(targetType, includeNonPublic, ref target);
                }
                catch
                {
                    return targetType.GetDefault();
                }
            }
            else
            {
                target = targetInstance;
            }

            // Case 0: Special Cases Handling (Source and Target are of specific convertible types)
            // Case 0.1: Source is string, Target is byte[]
            if (source is string sourceString && targetType == typeof(byte[]))
            {
                return GetByteArray(sourceString);
            }

            // Case 1: Source is a Dictionary<string, object>
            if (source is Dictionary<string, object> sourceProperties)
            {
                if (target is IDictionary targetDictionary)
                {
                    // Case 1.1: Source is Dictionary, Target is IDictionary
                    PopulateDictionary(targetType, includeNonPublic, sourceProperties, targetDictionary);
                }
                else
                {
                    // Case 1.2: Source is Dictionary, Target is not IDictionary (i.e. it is a complex type)
                    PopulateObject(targetType, includeNonPublic, sourceProperties, target);
                }

                return target;
            }

            // Case 2: Source is a List<object>
            if (source is List<object> sourceList)
            {
                switch (target)
                {
                    // Case 2.1: Source is List, Target is Array
                    case Array targetArray:
                        for (var i = 0; i < sourceList.Count; i++)
                        {
                            try
                            {
                                var targetItem = ConvertFromJsonResult(
                                    sourceList[i],
                                    targetType.GetElementType(),
                                    includeNonPublic);
                                targetArray.SetValue(targetItem, i);
                            }
                            catch
                            {
                                // ignored
                            }
                        }

                        break;
                    case IList targetList:

                        // Case 2.2: Source is List,  Target is IList
                        // find the add method of the target list
                        var addMethod = targetType.GetMethods()
                            .FirstOrDefault(
                                m => m.Name.Equals(AddMethodName) && m.IsPublic && m.GetParameters().Length == 1);

                        if (addMethod == null) return target;

                        foreach (var item in sourceList)
                        {
                            try
                            {
                                var targetItem = ConvertFromJsonResult(
                                    item,
                                    addMethod.GetParameters()[0].ParameterType,
                                    includeNonPublic);
                                targetList.Add(targetItem);
                            }
                            catch
                            {
                                // ignored
                            }
                        }

                        break;
                }

                return target;
            }

            // Case 3: Source is a simple type; Attempt conversion
            var sourceStringValue = source.ToStringInvariant();

            if (Definitions.BasicTypesInfo.ContainsKey(targetType))
            {
                // Handle basic types
                if (!targetType.TryParseBasicType(sourceStringValue, out target))
                    return targetType.GetDefault();
            }
            else
            {
                // Handle Enumerations
                GetEnumValue(targetType, sourceStringValue, ref target);
            }

            return target;
        }

        private static void GetEnumValue(Type targetType, string sourceStringValue, ref object target)
        {
            var enumType = Nullable.GetUnderlyingType(targetType);
            if (enumType == null && targetType.GetTypeInfo().IsEnum) enumType = targetType;
            if (enumType == null) return;

            try
            {
                target = Enum.Parse(enumType, sourceStringValue);
            }
            catch
            {
                // ignored
            }
        }

        private static void PopulateDictionary(Type targetType, bool includeNonPublic, Dictionary<string, object> sourceProperties, IDictionary targetDictionary)
        {
            // find the add method of the target dictionary
            var addMethod = targetType.GetMethods()
                .FirstOrDefault(
                    m => m.Name.Equals(AddMethodName) && m.IsPublic && m.GetParameters().Length == 2);

            // skip if we don't have a compatible add method
            if (addMethod == null) return;
            var addMethodParameters = addMethod.GetParameters();
            if (addMethodParameters[0].ParameterType != typeof(string)) return;

            // Retrieve the target entry type
            var targetEntryType = addMethodParameters[1].ParameterType;

            // Add the items to the target dictionary
            foreach (var sourceProperty in sourceProperties)
            {
                try
                {
                    var targetEntryValue = ConvertFromJsonResult(
                        sourceProperty.Value,
                        targetEntryType,
                        includeNonPublic);
                    targetDictionary.Add(sourceProperty.Key, targetEntryValue);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private static byte[] GetByteArray(string sourceString)
        {
            try
            {
                return Convert.FromBase64String(sourceString);
            } // Try conversion from Base 64
            catch
            {
                return Encoding.UTF8.GetBytes(sourceString);
            } // Get the string bytes in UTF8
        }

        private static void PopulateObject(Type targetType, bool includeNonPublic, Dictionary<string, object> sourceProperties, object target)
        {
            var fields = new List<MemberInfo>();

            if (targetType.IsValueType())
            {
                fields.AddRange(FieldTypeCache.RetrieveAllFields(targetType));
            }

            fields.AddRange(PropertyTypeCache.RetrieveAllProperties(targetType).Where(p => p.CanWrite).ToArray());

            foreach (var targetProperty in fields)
            {
                var sourcePropertyValue = GetSourcePropertyValue(sourceProperties, targetProperty);

                if (sourcePropertyValue == null) continue;

                try
                {
                    SetValue(targetType, includeNonPublic, target, sourcePropertyValue, targetProperty);
                }
                catch
                {
                    // ignored
                }
            }
        }

        private static object GetSourcePropertyValue(Dictionary<string, object> sourceProperties, MemberInfo targetProperty)
        {
            var targetPropertyName = targetProperty.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ??
                                     targetProperty.Name;

            return sourceProperties.ContainsKey(targetPropertyName)
                ? sourceProperties[targetPropertyName]
                : null;
        }

        private static object GetCurrentPropertyValue(bool includeNonPublic, 
            object target,
            MemberInfo targetProperty)
        {
            try
            {
                if (targetProperty is PropertyInfo property && !property.PropertyType.IsArray)
                    return property.GetGetMethod(includeNonPublic).Invoke(target, null);
            }
            catch
            {
                // ignored
            }

            return null;
        }

        private static void SetValue(
            Type targetType, 
            bool includeNonPublic, 
            object target, 
            object sourcePropertyValue,
            MemberInfo targetProperty)
        {
            var currentPropertyValue = GetCurrentPropertyValue(includeNonPublic, target, targetProperty);

            if (targetType.IsValueType())
            {
                var targetPropertyValue = ConvertFromJsonResult(
                    sourcePropertyValue,
                    (targetProperty as FieldInfo).FieldType,
                    ref currentPropertyValue,
                    includeNonPublic);

                (targetProperty as FieldInfo).SetValue(target, targetPropertyValue);
            }
            else
            {
                // Try to write properties to the current property value as a reference to the current property value
                var targetPropertyValue = ConvertFromJsonResult(
                    sourcePropertyValue,
                    (targetProperty as PropertyInfo).PropertyType,
                    ref currentPropertyValue,
                    includeNonPublic);

                (targetProperty as PropertyInfo).GetSetMethod(includeNonPublic)?.Invoke(target, new[] {targetPropertyValue});
            }
        }

        #endregion
    }
}