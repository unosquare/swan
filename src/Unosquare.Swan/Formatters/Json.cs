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
        private const string EmtpyArrayLiteral = "[ ]";
        private const string TrueLiteral = "true";
        private const string FalseLiteral = "false";
        private const string NullLiteral = "null";

        #endregion

        #region Private API

        /// <summary>
        /// Retrieves PropertyInfo[] (both public and non-public).
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

        #endregion

        #region Public API

        /// <summary>
        /// Serializes the specified object. All properties are serialized
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="format">if set to <c>true</c> it formats and indents the output.</param>
        /// <returns></returns>
        public static string Serialize(object obj, bool format = false)
        {
            return Serializer.Serialize(obj, 0, format, null, null);
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
            return Serializer.Serialize(obj, 0, format, includeNames, null);
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
            return Serializer.Serialize(obj, 0, format, null, excludeNames);
        }

        /// <summary>
        /// Deserializes the specified json string as either a Dictionary of string and object or as a List of object
        /// depending on the syntax of the JSON string
        /// </summary>
        /// <param name="json">The json.</param>
        /// <returns></returns>
        public static object Deserialize(string json)
        {
            return Deserializer.Deserialize(json);
        }

        private static object ConvertBasicValue(object sourceValue, Type targetType)
        {
            if (sourceValue == null) return targetType.GetDefault();
            if (Constants.BasicTypesInfo.ContainsKey(targetType) == false) return targetType.GetDefault();

            var sourceStringValue = sourceValue.ToStringInvariant();
            object target = null;

            if (Constants.BasicTypesInfo[targetType].TryParse(sourceStringValue, out target))
                return target;
            else
                return targetType.GetDefault();
        }

        private static void CopyTo(object target, object source, bool includeNonPublic)
        {
            const string AddMethodName = "Add";

            if (target == null) return;
            if (source == null) return;

            if (source is Dictionary<string, object>)
            {
                var sourceProperties = source as Dictionary<string, object>;
                var targetProperties = RetrieveProperties(target.GetType());

                foreach (var targetProperty in targetProperties)
                {
                    // weed out the nulls and non-matching names
                    if (sourceProperties.ContainsKey(targetProperty.Name) == false) continue;
                    var sourcePropertyValue = sourceProperties[targetProperty.Name];
                    if (sourcePropertyValue == null) continue;

                    // Get the current property value
                    var targetPropertyValue = targetProperty.GetGetMethod().Invoke(target, null);

                    // Case 1: Objects or dictionaries: Add(K, V)
                    if (sourcePropertyValue is Dictionary<string, object>)
                    {
                        // The target is null, go ahead an try to create an instance
                        if (targetPropertyValue == null)
                            targetPropertyValue = Activator.CreateInstance(targetProperty.PropertyType);

                        // The target is also a dictionary
                        if (targetPropertyValue is IDictionary)
                        {
                            var sourceDictionary = sourcePropertyValue as Dictionary<string, object>;
                            var targetDictionary = targetPropertyValue as IDictionary;

                            foreach (var kvp in sourceDictionary)
                            {
                                if (kvp.Value == null) targetDictionary.Add(kvp.Key, kvp.Value);
                                var kvpValueType = kvp.Value.GetType();
                                var kvpValue = Activator.CreateInstance(kvpValueType);

                                CopyTo(kvpValue, kvp.Value, includeNonPublic);
                                targetDictionary.Add(kvp.Key, kvpValue);
                            }

                            targetProperty.GetSetMethod(includeNonPublic).Invoke(target, new object[] { targetPropertyValue });
                            continue;
                        }

                        // Try to just copy properties
                        {
                            var sourceDictionary = sourcePropertyValue as Dictionary<string, object>;
                            var targetDictionary = RetrieveProperties(targetProperty.PropertyType);

                            foreach (var targetItem in targetDictionary)
                            {

                            }

                            continue;
                        }
                        
                    }

                    // Case 2: Arrays or Collections: Add(T)
                    if (sourcePropertyValue is List<object>)
                    {

                        continue;
                    }

                    // Case 3: Simple property copying
                    targetPropertyValue = ConvertBasicValue(sourcePropertyValue, targetProperty.PropertyType);
                    targetProperty.GetSetMethod(includeNonPublic).Invoke(target, new object[] { targetPropertyValue });
                }

            }

        }

        #endregion

    }
}