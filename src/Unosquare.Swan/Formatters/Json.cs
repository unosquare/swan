namespace Unosquare.Swan.Formatters
{
    using Reflection;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// A very simple JSON library written by Mario
    /// to teach Geo how things are done
    /// </summary>
    static public partial class Json
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
                type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
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
        private static object ConvertFromJsonResult(object source, Type targetType, ref object targetInstance, bool includeNonPublic)
        {
            #region Validation

            const string AddMethodName = "Add";

            if (source == null) return targetType.GetDefault();
            var sourceType = source.GetType();

            if (targetInstance != null) targetType = targetInstance.GetType();
            if (targetType == null || targetType == typeof(object)) targetType = sourceType;
            if (sourceType == targetType) return source;

            #endregion

            #region Target Instantiation or Assignment

            object target = null;

            if (targetInstance == null)
            {
                // Try to create a defult instance
                try
                {
                    // When using arrays, there is no default constructor, attempt to build a compatible array
                    if (source is List<object> && targetType.IsArray)
                    {
                        target = Activator.CreateInstance(targetType, new object[]
                        {
                            (source as List<object>) == null ? 0 : (source as List<object>).Count
                        });
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

            #endregion

            #region Case 0: Special Cases

            if (source is string && targetType == typeof(byte[]))
            {
                target = Convert.FromBase64String((source as string));
                return target;
            }

            #endregion

            #region Case 1: source is Dictionary<string, object>

            if (source is Dictionary<string, object>)
            {
                var sourceProperties = source as Dictionary<string, object>;
                if (target is IDictionary)
                {
                    // obtain a reference to the dictionary to be written out
                    var targetDictionary = target as IDictionary;

                    // find the add method of the target dictionary
                    var addMethod = targetType.GetTypeInfo().GetMethods()
                        .Where(m => m.Name.Equals(AddMethodName) && m.IsPublic && m.GetParameters().Length == 2).FirstOrDefault();

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
                            var targetEntryValue = ConvertFromJsonResult(sourceProperty.Value, targetEntryType, ref instance, includeNonPublic);
                            targetDictionary.Add(sourceProperty.Key, targetEntryValue);
                        }
                        catch { }
                    }
                }
                else
                {
                    var targetProperties = RetrieveProperties(targetType); //.Where(p => p.CanWrite);
                    foreach (var targetProperty in targetProperties)
                    {
                        var sourcePropertyValue = (sourceProperties.ContainsKey(targetProperty.Name)) ?
                            sourceProperties[targetProperty.Name] : null;

                        if (sourcePropertyValue == null) continue;

                        // Check if we already have an instance of the current value created for us
                        object currentPropertyValue = null;
                        try { currentPropertyValue = targetProperty.GetGetMethod(includeNonPublic)?.Invoke(target, null); }
                        catch { }

                        try
                        {
                            // Try to write properties to the current property value as a reference to the current property value
                            var targetPropertyValue = ConvertFromJsonResult(sourcePropertyValue, targetProperty.PropertyType, ref currentPropertyValue, includeNonPublic);

                            // HACK: Always try to write the value of possible; otherwise it was most likely set by reference
                            // if (currentPropertyValue == null || targetProperty.PropertyType == typeof(string) || targetProperty.PropertyType.IsValueType())
                            targetProperty.GetSetMethod(includeNonPublic)?.Invoke(target, new object[] { targetPropertyValue });
                        }
                        catch { }
                    }
                }

                return target;
            }

            #endregion

            #region Case 2: Source is a List<object>

            if (source is List<object>)
            {
                var sourceList = source as List<object>;
                if (targetType.IsArray)
                {
                    var targetArray = target as Array;
                    for (var i = 0; i < sourceList.Count; i++)
                    {
                        try
                        {
                            object nullRef = null;
                            var targetItem = ConvertFromJsonResult(sourceList[i], targetType.GetElementType(), ref nullRef, includeNonPublic);
                            targetArray.SetValue(targetItem, i);
                        }
                        catch { }
                    }
                }
                else if (target is IList)
                {
                    var targetList = target as IList;

                    // find the add method of the target list
                    var addMethod = targetType.GetTypeInfo().GetMethods()
                        .Where(m => m.Name.Equals(AddMethodName) && m.IsPublic && m.GetParameters().Length == 1).FirstOrDefault();

                    if (addMethod == null) return target;

                    foreach (var item in sourceList)
                    {
                        try
                        {
                            object nullRef = null;
                            var targetItem = ConvertFromJsonResult(item, addMethod.GetParameters()[0].ParameterType, ref nullRef, includeNonPublic);
                            targetList.Add(targetItem);
                        }
                        catch { }
                    }
                }

                return target;
            }

            #endregion


            #region Case 3: Source is a simple type; Attempt conversion

            {
                var sourceStringValue = source.ToStringInvariant();

                if (Constants.BasicTypesInfo.ContainsKey(targetType))
                {
                    // Handle basic types
                    if (Constants.BasicTypesInfo[targetType].TryParse(sourceStringValue, out target) == false)
                        return targetType.GetDefault();
                }
                else
                {
                    // Handle Enumerations
                    var enumType = Nullable.GetUnderlyingType(targetType);
                    if (enumType == null && targetType.GetTypeInfo().IsEnum == true) enumType = targetType;
                    if (enumType == null) return target;

                    try
                    {
                        target = Enum.Parse(enumType, sourceStringValue);
                    }
                    catch { }
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
        /// <param name="includeNonPublic">if set to <c>true</c> non-publuc getters will be also read.</param>
        /// <param name="includedNames">The included property names.</param>
        /// <param name="excludedNames">The excluded property names.</param>
        /// <returns></returns>
        public static string Serialize(object obj, bool format = false, bool includeNonPublic = false, string[] includedNames = null, string[] excludedNames = null)
        {
            return Serializer.Serialize(obj, 0, format, includedNames, excludedNames, includeNonPublic);
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
            return Serializer.Serialize(obj, 0, format, includeNames, null, true);
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
            return Serializer.Serialize(obj, 0, format, null, excludeNames, false);
        }

        /// <summary>
        /// Deserializes the specified json string as either a Dictionary[string, object] or as a List[object]
        /// depending on the syntax of the JSON string
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public static object Deserialize(string json)
        {
            return Deserializer.Deserialize(json);
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
            return (T)Deserialize(json, typeof(T), false);
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
            return (T)Deserialize(json, typeof(T), includeNonPublic);
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
            var source = Deserializer.Deserialize(json);
            object nullRef = null;
            return ConvertFromJsonResult(source, resultType, ref nullRef, includeNonPublic);
        }

        #endregion

    }
}