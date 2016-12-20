namespace Unosquare.Swan.Formatters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Unosquare.Swan.Reflection;

    /// <summary>
    /// Represents a light-weight JSON serializer/deserializer. You can serialize/deserialize
    /// objects with primitives and arrays, and one-dimensional arrays only.
    /// 
    /// This is an useful helper for small tasks but it doesn't represent a full-featured
    /// serializer such as the beloved Json.NET
    /// </summary>
    public static class JsonFormatter
    {
        #region Constants 

        private const char InitialObjectCharacter = '{';
        private const char FinalObjectCharacter = '}';

        private const char InitialArrayCharacter = '[';
        private const char FinalArrayCharacter = ']';

        private const char FieldSeparatorCharacter = ',';
        private const char ValueSeparatorCharacter = ':';

        private const char StringEscapeCharacter = '\\';
        private const char StringQuotedCharacter = '"';
        private const char MinusNumberCharacter = '-';

        private const string TrueValue = "true";
        private const string FalseValue = "false";
        private const string NullValue = "null";

        #endregion

        #region Static Variables

        private static readonly object SyncLock = new object();
        private static readonly PropertyTypeCache TypeCache = new PropertyTypeCache();

        #endregion

        #region Enumerations

        /// <summary>
        /// Defines states for the parsing state machine
        /// </summary>
        private enum ReadState
        {
            WaitingForNewField,
            PushingFieldName,
            WaitingForValue,
            PushingStringValue,
            PushingValue,
            WaitingForArrayEnd,
            WaitingForObject
        }

        #endregion

        #region Methods

        /// <summary>
        /// Serializes the specified collection.
        /// </summary>
        /// <param name="coll">The coll.</param>
        /// <returns></returns>
        public static string Serialize(IEnumerable coll)
        {
            if (coll == null)
                return $"{InitialArrayCharacter} {FinalArrayCharacter}";

            var data = string.Join(FieldSeparatorCharacter.ToStringInvariant(), coll.Cast<object>().Select(x => InternalSerialize(x, true)));

            return $"{InitialArrayCharacter} {data} {FinalArrayCharacter}";
        }

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            return InternalSerialize(obj, false);
        }

        internal static string InternalSerialize(object obj, bool fromArray)
        {
            if (obj == null)
                return $"{InitialObjectCharacter} {FinalObjectCharacter}";

            var sb = new StringBuilder();

            if (Constants.AllBasicTypes.Contains(obj.GetType()))
            {
                if (fromArray == false)
                    throw new InvalidOperationException("You need an object or array to serialize");

                if (obj is bool)
                {
                    sb.Append(obj.ToStringInvariant().ToLowerInvariant());
                }
                else if (Constants.AllNumericTypes.Contains(obj.GetType()))
                {
                    sb.Append(obj);
                }
                else
                {
                    sb.Append($"\"{obj}\"");
                }
            }
            else
            {
                var props = GetTypeProperties(obj.GetType()).Where(x => x.CanRead);

                sb.Append(InitialObjectCharacter);

                foreach (var prop in props)
                {
                    var value = prop.GetValue(obj);

                    if (value == null)
                    {
                        sb.Append($"\"{prop.Name}\" : null, ");
                    }
                    else if (prop.IsCollection())
                    {
                        sb.Append($"\"{prop.Name}\" : {Serialize(value as IEnumerable)}, ");
                    }
                    else if (Constants.AllNumericTypes.Contains(prop.PropertyType) || value is int) // TODO: How to detect numbers in object properties?
                    {
                        sb.Append($"\"{prop.Name}\" : {value}, ");
                    }
                    else if (prop.PropertyType == typeof(bool))
                    {
                        sb.Append($"\"{prop.Name}\" : {value.ToStringInvariant().ToLowerInvariant()}, ");
                    }
                    else if (Constants.AllBasicTypes.Contains(prop.PropertyType) == false)
                    {
                        sb.Append($"\"{prop.Name}\" : {InternalSerialize(value, true)}, ");
                    }
                    else
                    {
                        // fall-back to string
                        sb.Append($"\"{prop.Name}\" : \"{value}\", ");
                    }
                }

                if (sb.Length > 0)
                    sb.Remove(sb.Length - 2, 2);

                sb.Append(FinalObjectCharacter);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Deserializes the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">source</exception>
        public static Dictionary<string, object> Deserialize(string source)
        {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentNullException(nameof(source));

            switch (source[0])
            {
                case InitialObjectCharacter:
                    return ParseDictionary(source);
                default:
                    return default(Dictionary<string, object>);
            }
        }

        /// <summary>
        /// Deserializes the specified JSON string.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">source</exception>
        public static T Deserialize<T>(string source)
        {
            if (string.IsNullOrEmpty(source))
                throw new ArgumentNullException(nameof(source));

            switch (source[0])
            {
                case InitialObjectCharacter:
                    return ParseObject<T>(source);
                case InitialArrayCharacter:
                    var genericArgs = typeof(T).GetTypeInfo().GetGenericArguments();
                    if (genericArgs.Any())
                        return (T)ParseArray(genericArgs[0], source);

                    return default(T);
                default:
                    return default(T);
            }
        }

        #endregion

        #region Support Methods

        private static void SetPropertyArrayValue<T>(IEnumerable<PropertyInfo> properties, string propertyName,
            string source, T result)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) return;

            var targetProperty = properties.FirstOrDefault(p => p.Name.Equals(propertyName));

            // Skip if the property is not found
            if (targetProperty == null)
                return;

            var itemType = targetProperty.PropertyType.GetElementType();
            var primitiveValue = Constants.AllBasicTypes.Contains(itemType);

            // Parse and assign the basic type value to the property
            try
            {
                var propertyValue = ParseArray(itemType, source);
                var arr = Array.CreateInstance(itemType, propertyValue.Cast<object>().Count());

                var i = 0;
                foreach (var value in propertyValue)
                {
                    if (primitiveValue)
                    {
                        object itemvalue;
                        if (Constants.BasicTypesInfo[itemType].TryParse(value.ToString(), out itemvalue))
                            arr.SetValue(itemvalue, i++);
                    }
                    else
                    {
                        arr.SetValue(value, i++);
                    }
                }

                targetProperty.SetValue(result, arr);
            }
            catch (Exception ex)
            {
                ex.ToStringInvariant().Info();
                // swallow
            }
        }

        private static void SetPropertyValue<T>(IEnumerable<PropertyInfo> properties, string propertyName,
            string propertyStringValue, T result)
        {
            var targetProperty = properties.FirstOrDefault(p => p.Name.Equals(propertyName));

            // Skip if the property is not found
            if (targetProperty == null)
                return;

            // Parse and assign the basic type value to the property
            try
            {
                if (propertyStringValue == null)
                {
                    targetProperty.SetValue(result, null);
                }
                else
                {
                    object propertyValue;
                    if (Constants.BasicTypesInfo[targetProperty.PropertyType].TryParse(propertyStringValue,
                        out propertyValue))
                        targetProperty.SetValue(result, propertyValue);
                }
            }
            catch
            {
                // swallow
            }
        }

        private static void SetPropertyObjectValue<T>(IEnumerable<PropertyInfo> properties, string propertyName,
            string source, T result)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) return;

            var targetProperty = properties.FirstOrDefault(p => p.Name.Equals(propertyName));

            // Skip if the property is not found
            if (targetProperty == null)
                return;

            // Parse and assign the basic type value to the property
            try
            {
                var obj = ParseObject(targetProperty.PropertyType, source);
                targetProperty.SetValue(result, obj);
            }
            catch
            {
                // swallow
            }
        }

        private static IList ParseArray(Type type, string source)
        {
            var result = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
            var currentState = ReadState.WaitingForValue;
            var currentValue = new StringBuilder(1024);

            for (var charIndex = 0; charIndex < source.Length; charIndex++)
            {
                // Get the current and next character
                var currentChar = source[charIndex];
                var nextChar = charIndex < source.Length - 1 ? source[charIndex + 1] : new char?();

                // Perform logic based on state and decide on next state
                switch (currentState)
                {
                    case ReadState.WaitingForValue:
                        {
                            if (currentChar == InitialObjectCharacter)
                            {
                                currentState = ReadState.WaitingForObject;
                                currentValue.Append(currentChar);
                            }
                            else if (currentChar == StringQuotedCharacter)
                            {
                                currentState = ReadState.PushingStringValue;
                            }
                            else if (currentChar == MinusNumberCharacter ||
                                     char.IsNumber(currentChar))
                            {
                                currentState = ReadState.PushingValue;
                                currentValue.Append(currentChar);
                            }
                            else if (currentChar == TrueValue[0] && nextChar.HasValue && nextChar == TrueValue[1])
                            {
                                result.Add(TrueValue);
                                charIndex += TrueValue.Length;

                                currentState = ReadState.WaitingForValue;
                            }
                            else if (currentChar == FalseValue[0] && nextChar.HasValue && nextChar == FalseValue[1])
                            {
                                result.Add(FalseValue);
                                charIndex += FalseValue.Length;

                                currentState = ReadState.WaitingForValue;
                            }
                            else if (currentChar == NullValue[0] && nextChar.HasValue && nextChar == NullValue[1])
                            {
                                result.Add(NullValue);
                                charIndex += NullValue.Length;

                                currentState = ReadState.WaitingForValue;
                            }
                            break;
                        }
                    case ReadState.PushingValue:
                        {
                            if (currentChar == FieldSeparatorCharacter)
                            {
                                result.Add(currentValue.ToString());
                                currentValue.Clear();
                                currentState = ReadState.WaitingForValue;
                            }
                            else
                            {
                                currentValue.Append(currentChar);
                            }
                            break;
                        }
                    case ReadState.PushingStringValue:
                        {
                            if (currentChar == StringQuotedCharacter)
                            {
                                result.Add(currentValue.ToString());
                                currentValue.Clear();
                                currentState = ReadState.WaitingForValue;
                            }
                            else
                            {
                                currentValue.Append(currentChar);
                            }
                            break;
                        }
                    case ReadState.WaitingForObject:
                        {
                            if (currentChar == FinalObjectCharacter)
                            {
                                currentValue.Append(currentChar);
                                var obj = ParseObject(type, currentValue.ToString());
                                result.Add(obj);
                                currentValue.Clear();
                                currentState = ReadState.WaitingForValue;
                            }
                            else
                            {
                                currentValue.Append(currentChar);
                            }
                            break;
                        }
                }
            }

            return result;
        }

        private static Dictionary<string, object> ParseDictionary(string source)
        {
            var result = new Dictionary<string, object>();

            ParseObject(source,
                (propertyName, currentValue) => result[propertyName] = currentValue,
                (propertyName, currentValue) => result[propertyName] = ParseDictionary(currentValue),
                (propertyName, currentValue) => result[propertyName] = ParseArray(typeof(string), currentValue));

            return result;
        }

        private static T ParseObject<T>(string source)
        {
            return (T)ParseObject(typeof(T), source);
        }

        private static object ParseObject(Type type, string source)
        {
            var result = Activator.CreateInstance(type);
            var props = GetTypeProperties(result.GetType()).Where(x => x.CanWrite);

            ParseObject(source,
                (propertyName, currentValue) => SetPropertyValue(props, propertyName, currentValue, result),
                (propertyName, currentValue) => SetPropertyObjectValue(props, propertyName, currentValue, result),
                (propertyName, currentValue) => SetPropertyArrayValue(props, propertyName, currentValue, result));

            return result;
        }

        private static void ParseObject(string source, Action<string, string> setPropertyValue, Action<string, string> setPropertyObjectValue, Action<string, string> setPropertyArray)
        {
            var currentState = ReadState.WaitingForNewField;
            var currentPropertyName = new StringBuilder(1024);
            var currentValue = new StringBuilder(1024);
            var skipskipFinalObjectCharacter = 0;

            for (var charIndex = 0; charIndex < source.Length; charIndex++)
            {
                // Get the current and next character
                var currentChar = source[charIndex];
                var nextChar = charIndex < source.Length - 1 ? source[charIndex + 1] : new char?();

                // Perform logic based on state and decide on next state
                switch (currentState)
                {
                    case ReadState.WaitingForNewField:
                        {
                            // clean up
                            if (string.IsNullOrWhiteSpace(currentPropertyName.ToString()) == false)
                                setPropertyValue(currentPropertyName.ToString(), currentValue.ToString());

                            currentPropertyName.Clear();
                            currentValue.Clear();

                            if (currentChar == StringQuotedCharacter)
                            {
                                currentState = ReadState.PushingFieldName;
                            }
                            break;
                        }
                    case ReadState.PushingFieldName:
                        {
                            if (currentChar == StringQuotedCharacter)
                            {
                                currentState = ReadState.WaitingForValue;
                            }
                            else
                            {
                                currentPropertyName.Append(currentChar);
                            }
                            break;
                        }
                    case ReadState.WaitingForValue:
                        {
                            if (currentChar == StringQuotedCharacter)
                            {
                                currentState = ReadState.PushingStringValue;
                            }
                            else if (currentChar == MinusNumberCharacter ||
                                     char.IsNumber(currentChar))
                            {
                                currentState = ReadState.PushingValue;
                                currentValue.Append(currentChar);
                            }
                            else if (currentChar == TrueValue[0] && nextChar.HasValue && nextChar == TrueValue[1])
                            {
                                if (string.IsNullOrWhiteSpace(currentPropertyName.ToString()) == false)
                                    setPropertyValue(currentPropertyName.ToString(), true.ToStringInvariant());
                                currentPropertyName.Clear();
                                charIndex += TrueValue.Length;

                                currentState = ReadState.WaitingForNewField;
                            }
                            else if (currentChar == FalseValue[0] && nextChar.HasValue && nextChar == FalseValue[1])
                            {
                                if (string.IsNullOrWhiteSpace(currentPropertyName.ToString()) == false)
                                    setPropertyValue(currentPropertyName.ToString(), false.ToStringInvariant());
                                currentPropertyName.Clear();
                                charIndex += FalseValue.Length;

                                currentState = ReadState.WaitingForNewField;
                            }
                            else if (currentChar == NullValue[0] && nextChar.HasValue && nextChar == NullValue[1])
                            {
                                if (string.IsNullOrWhiteSpace(currentPropertyName.ToString()) == false)
                                    setPropertyValue(currentPropertyName.ToString(), null);
                                currentPropertyName.Clear();
                                charIndex += NullValue.Length;

                                currentState = ReadState.WaitingForNewField;
                            }
                            else if (currentChar == InitialArrayCharacter)
                            {
                                currentState = ReadState.WaitingForArrayEnd;
                                currentValue.Append(currentChar);
                            }
                            else if (currentChar == InitialObjectCharacter)
                            {
                                currentState = ReadState.WaitingForObject;
                                currentValue.Append(currentChar);
                            }
                            break;
                        }
                    case ReadState.WaitingForObject:
                        {
                            if (currentChar == FinalObjectCharacter)
                            {
                                currentValue.Append(currentChar);

                                if (skipskipFinalObjectCharacter == 0)
                                {
                                    setPropertyObjectValue(currentPropertyName.ToString(), currentValue.ToString());
                                    currentPropertyName.Clear();
                                    currentState = ReadState.WaitingForNewField;
                                }
                                else
                                {
                                    skipskipFinalObjectCharacter--;
                                }
                            }
                            else if (currentChar == InitialObjectCharacter)
                            {
                                skipskipFinalObjectCharacter++;
                                currentValue.Append(currentChar);
                            }
                            else
                            {
                                currentValue.Append(currentChar);
                            }
                            break;
                        }
                    case ReadState.WaitingForArrayEnd:
                        {
                            if (currentChar == FinalArrayCharacter)
                            {
                                currentValue.Append(currentChar);
                                setPropertyArray(currentPropertyName.ToString(), currentValue.ToString());
                                currentPropertyName.Clear();
                                currentState = ReadState.WaitingForNewField;
                            }
                            else
                            {
                                currentValue.Append(currentChar);
                            }
                            break;
                        }
                    case ReadState.PushingStringValue:
                        {
                            if (currentChar == StringQuotedCharacter)
                            {
                                currentState = ReadState.WaitingForNewField;
                            }
                            else
                            {
                                currentValue.Append(currentChar);
                            }
                            break;
                        }
                    case ReadState.PushingValue:
                        {
                            if (currentChar == FieldSeparatorCharacter)
                            {
                                currentState = ReadState.WaitingForNewField;
                            }
                            else
                            {
                                currentValue.Append(currentChar);
                            }
                            break;
                        }
                }
            }
        }

        private static IEnumerable<PropertyInfo> GetTypeProperties(Type type)
        {
            lock (SyncLock)
            {
                return TypeCache.Retrieve(type, () =>
                {
                    return type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.CanRead || p.CanWrite)
                        .ToArray();
                });
            }
        }

        #endregion

    }
}