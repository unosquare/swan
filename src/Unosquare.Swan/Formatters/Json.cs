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
        private static readonly PropertyTypeCache TypeCache = new PropertyTypeCache();

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
        /// Retrieves PropertyInfo[] (both public and non-public). for the given type
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static PropertyInfo[] RetrieveProperties(Type type)
        {
            return TypeCache.Retrieve(type, () =>
            {
                return
                    type.GetTypeInfo()
                        .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(p => p.CanRead || p.CanWrite).ToArray();
            });
        }

        /// <summary>
        /// Converts a json deserialized object (simple type, dictionary or list) to a new instance of the sepcified target type.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <param name="targetInstance">An optional target instance. If null, we will attempt creation.</param>
        /// <param name="includeNonPublic">if set to <c>true</c> [include private].</param>
        /// <returns></returns>
        private static object ConvertFromJsonResult(object source, Type targetType, ref object targetInstance,
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
                        var addMethod = targetType.GetTypeInfo()
                            .GetMethods()
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
                                var targetEntryValue = ConvertFromJsonResult(sourceProperty.Value, targetEntryType,
                                    ref instance, includeNonPublic);
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
                        var targetProperties = RetrieveProperties(targetType); //.Where(p => p.CanWrite);
                        foreach (var targetProperty in targetProperties)
                        {
                            var targetPropertyName = targetProperty.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? targetProperty.Name;
                            var sourcePropertyValue = sourceProperties.ContainsKey(targetPropertyName)
                                ? sourceProperties[targetPropertyName]
                                : null;

                            if (sourcePropertyValue == null) continue;

                            object currentPropertyValue = null;
                            
                            if (targetProperty.PropertyType.IsArray == false)
                            { 
                                // Check if we already have an instance of the current value created for us
                                try
                                {
                                    currentPropertyValue = targetProperty.GetGetMethod(includeNonPublic)?
                                        .Invoke(target, null);
                                }
                                catch
                                {
                                    // ignored
                                }
                            }

                            try
                            {
                                // Try to write properties to the current property value as a reference to the current property value
                                var targetPropertyValue = ConvertFromJsonResult(sourcePropertyValue,
                                    targetProperty.PropertyType, ref currentPropertyValue, includeNonPublic);

                                // HACK: Always try to write the value of possible; otherwise it was most likely (hopefully) set by reference
                                // if (currentPropertyValue == null || targetProperty.PropertyType == typeof(string) || targetProperty.PropertyType.IsValueType())
                                targetProperty.GetSetMethod(includeNonPublic)?
                                    .Invoke(target, new[] {targetPropertyValue});
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
                                var targetItem = ConvertFromJsonResult(sourceList[i], targetType.GetElementType(),
                                    ref nullRef, includeNonPublic);
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
                        var addMethod = targetType.GetTypeInfo()
                            .GetMethods()
                            .FirstOrDefault(
                                m => m.Name.Equals(addMethodName) && m.IsPublic && m.GetParameters().Length == 1);

                        if (addMethod == null) return target;

                        foreach (var item in sourceList)
                        {
                            try
                            {
                                object nullRef = null;
                                var targetItem = ConvertFromJsonResult(item, addMethod.GetParameters()[0].ParameterType,
                                    ref nullRef, includeNonPublic);
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
        /// <returns></returns>
        public static string Serialize(object obj, bool format = false, string typeSpecifier = null, bool includeNonPublic = false,
            string[] includedNames = null, string[] excludedNames = null)
        {
            if (obj != null && Definitions.AllBasicValueTypes.Contains(obj.GetType()))
                throw new ArgumentException("You need to provide an object or array", nameof(obj));

            var excludedByAttr = obj?.GetType().GetTypeInfo().GetProperties().Where(x => x?.GetCustomAttribute<JsonPropertyAttribute>()?.Ignored == true).Select(x => x.Name);

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
        /// <returns></returns>
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
        /// <returns></returns>
        public static string SerializeExcluding(object obj, bool format, params string[] excludeNames)
        {
            return Serializer.Serialize(obj, 0, format, null, null, excludeNames, false, null);
        }

        /// <summary>
        /// Deserializes the specified json string as either a Dictionary[string, object] or as a List[object]
        /// depending on the syntax of the JSON string
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
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
        /// <returns></returns>
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
        /// <returns></returns>
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
        /// <returns></returns>
        public static object Deserialize(string json, Type resultType, bool includeNonPublic)
        {
            var source = Deserializer.DeserializeInternal(json);
            object nullRef = null;
            return ConvertFromJsonResult(source, resultType, ref nullRef, includeNonPublic);
        }

        #endregion
    }
}