namespace Unosquare.Swan.Formatters
{
    using Reflection;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    /// <summary>
    /// A very simple, light-weight JSON library written by Mario
    /// to teach Geo how things are done
    /// 
    /// This is an useful helper for small tasks but it doesn't represent a full-featured
    /// serializer such as the beloved Json.NET
    /// </summary>
    public static partial class Json
    {
        private static readonly PropertyTypeCache PropertyTypeCache = new PropertyTypeCache();
        private static readonly FieldTypeCache FieldTypeCache = new FieldTypeCache();

        #region Constants 

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

        #region Private API

        /// <summary>
        /// Retrieves PropertyInfo[] (both public and non-public) for the given type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Properties for the given type</returns>
        private static PropertyInfo[] RetrieveProperties(Type type)
        {
            return PropertyTypeCache.Retrieve(type, PropertyTypeCache.GetAllPropertiesFunc(type));
        }

        /// <summary>
        /// Retrieves FieldInfo[] (public) for the given type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>Value of a field supported by a given object</returns>
        private static FieldInfo[] RetrieveFields(Type type)
        {
            return FieldTypeCache.Retrieve(type, FieldTypeCache.GetAllFieldsFunc(type));
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
            #region Setup: State and Validation

            const string addMethodName = "Add";
            object target = null;

            {
                if (source == null) return targetType.GetDefault();
                var sourceType = source.GetType();

                if (targetInstance != null) targetType = targetInstance.GetType();
                if (targetType == null || targetType == typeof(object)) targetType = sourceType;
                if (sourceType == targetType) return source;
            }

            #endregion

            #region Setup: Target Instantiation or Assignment

            {
                if (targetInstance == null)
                {
                    // Try to create a default instance
                    try
                    {
                        // When using arrays, there is no default constructor, attempt to build a compatible array
                        var sourceObjectList = source as List<object>;
                        if (sourceObjectList != null && targetType.IsArray)
                        {
                            target = Array.CreateInstance(targetType.GetElementType(), sourceObjectList.Count);
                        }
                        else if (source is string && targetType == typeof(byte[]))
                        {
                            // do nothing. Simply skip creation
                        }
                        else
                        {
                            target = Activator.CreateInstance(targetType, includeNonPublic);
                        }
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
            }

            #endregion

            #region Case 0: Special Cases Handling (Source and Target are of specific convertible types)

            {
                #region Case 0.1: Source is string, Target is byte[]

                var sourceString = source as string;
                if (sourceString != null && targetType == typeof(byte[]))
                {
                    try
                    {
                        target = Convert.FromBase64String(sourceString);
                    } // Try conversion from Base 64
                    catch
                    {
                        target = Encoding.UTF8.GetBytes(sourceString);
                    } // Get the string bytes in UTF8

                    return target;
                }

                #endregion
            }

            #endregion

            #region Case 1: Source is a Dictionary<string, object>

            {
                var sourceProperties = source as Dictionary<string, object>;
                if (sourceProperties != null)
                {
                    #region Case 1.1: Source is Dictionary, Target is IDictionary

                    var targetDictionary = target as IDictionary;
                    if (targetDictionary != null)
                    {
                        // find the add method of the target dictionary
                        var addMethod = targetType.GetMethods()
                            .FirstOrDefault(
                                m => m.Name.Equals(addMethodName) && m.IsPublic && m.GetParameters().Length == 2);

                        // skip if we don't have a compatible add method
                        if (addMethod == null) return target;
                        var addMethodParameters = addMethod.GetParameters();
                        if (addMethodParameters[0].ParameterType != typeof(string)) return target;

                        // Retrieve the target entry type
                        var targetEntryType = addMethodParameters[1].ParameterType;

                        // Add the items to the target dictionary
                        foreach (var sourceProperty in sourceProperties)
                        {
                            try
                            {
                                object instance = null;
                                var targetEntryValue = ConvertFromJsonResult(
                                    sourceProperty.Value, 
                                    targetEntryType,
                                    ref instance, 
                                    includeNonPublic);
                                targetDictionary.Add(sourceProperty.Key, targetEntryValue);
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }

                    #endregion

                    #region Case 1.1: Source is Dictionary, Target is not IDictionary (i.e. it is a complex type)

                    else
                    {
                        PopulateObject(targetType, includeNonPublic, sourceProperties, target);
                    }

                    #endregion

                    return target;
                }
            }

            #endregion

            #region Case 2: Source is a List<object>

            {
                var sourceList = source as List<object>;
                if (sourceList != null)
                {
                    var targetArray = target as Array;
                    var targetList = target as IList;

                    #region Case 2.1: Source is List, Target is Array

                    if (targetArray != null)
                    {
                        for (var i = 0; i < sourceList.Count; i++)
                        {
                            try
                            {
                                object nullRef = null;
                                var targetItem = ConvertFromJsonResult(
                                    sourceList[i], 
                                    targetType.GetElementType(),
                                    ref nullRef, 
                                    includeNonPublic);
                                targetArray.SetValue(targetItem, i);
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }

                    #endregion

                    #region Case 2.2: Source is List,  Target is IList

                    else if (targetList != null)
                    {
                        // find the add method of the target list
                        var addMethod = targetType.GetMethods()
                            .FirstOrDefault(
                                m => m.Name.Equals(addMethodName) && m.IsPublic && m.GetParameters().Length == 1);

                        if (addMethod == null) return target;

                        foreach (var item in sourceList)
                        {
                            try
                            {
                                object nullRef = null;
                                var targetItem = ConvertFromJsonResult(
                                    item, 
                                    addMethod.GetParameters()[0].ParameterType,
                                    ref nullRef, 
                                    includeNonPublic);
                                targetList.Add(targetItem);
                            }
                            catch
                            {
                                // ignored
                            }
                        }
                    }

                    #endregion

                    return target;
                }
            }

            #endregion

            #region Case 3: Source is a simple type; Attempt conversion

            {
                var sourceStringValue = source.ToStringInvariant();

                if (Definitions.BasicTypesInfo.ContainsKey(targetType))
                {
                    // Handle basic types
                    if (Definitions.BasicTypesInfo[targetType].TryParse(sourceStringValue, out target) == false)
                        return targetType.GetDefault();
                }
                else
                {
                    // Handle Enumerations
                    var enumType = Nullable.GetUnderlyingType(targetType);
                    if (enumType == null && targetType.GetTypeInfo().IsEnum) enumType = targetType;
                    if (enumType == null) return target;

                    try
                    {
                        target = Enum.Parse(enumType, sourceStringValue);
                    }
                    catch
                    {
                        // ignored
                    }
                }

                return target;
            }

            #endregion
        }

        private static void PopulateObject(Type targetType, bool includeNonPublic, Dictionary<string, object> sourceProperties, object target)
        {
            var fields = new List<MemberInfo>();

            if (targetType.IsValueType())
            {
                fields.AddRange(RetrieveFields(targetType));
            }

            fields.AddRange(RetrieveProperties(targetType).Where(p => p.CanWrite).ToArray());

            foreach (var targetProperty in fields)
            {
                var targetPropertyName = targetProperty.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ??
                                         targetProperty.Name;
                var sourcePropertyValue = sourceProperties.ContainsKey(targetPropertyName)
                    ? sourceProperties[targetPropertyName]
                    : null;

                if (sourcePropertyValue == null) continue;

                object currentPropertyValue = null;

                if (!targetType.IsValueType() && (targetProperty as PropertyInfo).PropertyType.IsArray == false)
                {
                    try
                    {
                        currentPropertyValue = (targetProperty as PropertyInfo).GetGetMethod(includeNonPublic)?.Invoke(target, null);
                    }
                    catch
                    {
                        // ignored
                    }
                }

                try
                {
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

                        // HACK: Always try to write the value of possible; otherwise it was most likely (hopefully) set by reference
                        // if (currentPropertyValue == null || targetProperty.PropertyType == typeof(string) || targetProperty.PropertyType.IsValueType())

                        (targetProperty as PropertyInfo).GetSetMethod(includeNonPublic)?
                            .Invoke(target, new[] {targetPropertyValue});
                    }
                }
                catch
                {
                    // ignored
                }
            }
        }

        #endregion

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
        /// <returns>A string that represents the current object</returns>
        public static string Serialize(
            object obj, 
            bool format = false, 
            string typeSpecifier = null, 
            bool includeNonPublic = false,
            string[] includedNames = null, 
            string[] excludedNames = null)
        {
            if (obj != null && Definitions.AllBasicValueTypes.Contains(obj.GetType()))
                throw new ArgumentException("You need to provide an object or array", nameof(obj));

            var excludedByAttr = obj?.GetType().GetProperties()
                .Where(x => x?.GetCustomAttribute<JsonPropertyAttribute>()?.Ignored == true).Select(x => x.Name).ToArray();

            if (excludedByAttr?.Any() == true)
            {
                excludedNames = excludedNames == null ? excludedByAttr.ToArray() : excludedByAttr.Intersect(excludedNames).ToArray();
            }

            return Serializer.Serialize(obj, 0, format, typeSpecifier, includedNames, excludedNames, includeNonPublic, null);
        }

        /// <summary>
        /// Serializes the specified object only including the specified property names.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="format">if set to <c>true</c> it formats and indents the output.</param>
        /// <param name="includeNames">The include names.</param>
        /// <returns>A string that represents the current object</returns>
        public static string SerializeOnly(object obj, bool format, params string[] includeNames)
        {
            return Serializer.Serialize(obj, 0, format, null, includeNames, null, true, null);
        }

        /// <summary>
        /// Serializes the specified object excluding the specified property names.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="format">if set to <c>true</c> it formats and indents the output.</param>
        /// <param name="excludeNames">The exclude names.</param>
        /// <returns>A string that represents the current object</returns>
        public static string SerializeExcluding(object obj, bool format, params string[] excludeNames)
        {
            return Serializer.Serialize(obj, 0, format, null, null, excludeNames, false, null);
        }

        /// <summary>
        /// Deserializes the specified json string as either a Dictionary[string, object] or as a List[object]
        /// depending on the syntax of the JSON string
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns>Type of the current deserializes</returns>
        public static object Deserialize(string json)
        {
            return Deserializer.DeserializeInternal(json);
        }

        /// <summary>
        /// Deserializes the specified json string and converts it to the specified object type.
        /// Non-public constructors and property setters are ignored.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json">The json.</param>
        /// <returns>The deserialized specified type object</returns>
        public static T Deserialize<T>(string json)
        {
            return (T) Deserialize(json, typeof(T), false);
        }

        /// <summary>
        /// Deserializes the specified json string and converts it to the specified object type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json">The json.</param>
        /// <param name="includeNonPublic">if set to true, it also uses the non-public constructors and property setters.</param>
        /// <returns>The deserialized specified type object</returns>
        public static T Deserialize<T>(string json, bool includeNonPublic)
        {
            return (T) Deserialize(json, typeof(T), includeNonPublic);
        }

        /// <summary>
        /// Deserializes the specified json string and converts it to the specified object type.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="resultType">Type of the result.</param>
        /// <param name="includeNonPublic">if set to true, it also uses the non-public constructors and property setters.</param>
        /// <returns>Type of the current convertion from json result</returns>
        public static object Deserialize(string json, Type resultType, bool includeNonPublic)
        {
            var source = Deserializer.DeserializeInternal(json);
            object nullRef = null;
            return ConvertFromJsonResult(source, resultType, ref nullRef, includeNonPublic);
        }

        #endregion
    }
}