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
        /// Defines the 4 different read states
        /// for the parsing state machine
        /// </summary>
        private enum ReadState
        {
            WaitingForNewField,
            PushingFieldName,
            WaitingForValue,
            PushingValue,
            WaitingForArrayEnd
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
                return NullValue;

            var firstItem = coll.Cast<object>().FirstOrDefault(x => x != null);
            var quotedValues = firstItem is string;

            return "[" +
                   string.Join(FieldSeparatorCharacter.ToStringInvariant(),
                       coll.Cast<object>().Select(x => quotedValues ? $"\"{x}\"" : x.ToStringInvariant())) + "]";
        }

        /// <summary>
        /// Serializes the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static string Serialize(object obj)
        {
            if (obj == null)
                return NullValue;

            var props = GetTypeProperties(obj.GetType()).Where(x => x.CanRead);
            var sb = new StringBuilder();

            foreach (var prop in props)
            {
                var value = prop.GetValue(obj);

                if (value == null)
                {
                    sb.Append($"\"{prop.Name}\" : null, ");
                }
                else if (prop.PropertyType != typeof(string) &&
                         typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(prop.PropertyType))
                {
                    sb.Append($"\"{prop.Name}\" : {Serialize(value as IEnumerable)}, ");
                }
                else if (Constants.AllNumericTypes.Contains(prop.PropertyType))
                {
                    sb.Append($"\"{prop.Name}\" : {value}, ");
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    sb.Append($"\"{prop.Name}\" : {value.ToStringInvariant().ToLowerInvariant()}, ");
                }
                else
                {
                    // fall-back to string
                    sb.Append($"\"{prop.Name}\" : \"{value}\", ");
                }
            }

            if (sb.Length > 0)
                sb.Remove(sb.Length - 2, 2);

            return $"{{{sb}}}";
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
                        return (T) ParseArray(genericArgs[0], source);

                    return default(T);
                default:
                    return default(T);
            }
        }

        #endregion

        #region Support Methods

        private static void SetPropertyValue<T>(IEnumerable<PropertyInfo> properties, string propertyName,
            IEnumerable propertyValue, T result)
        {
            if (string.IsNullOrWhiteSpace(propertyName)) return;

            var targetProperty = properties.FirstOrDefault(p => p.Name.Equals(propertyName));

            // Skip if the property is not found
            if (targetProperty == null)
                return;

            var itemType = targetProperty.PropertyType.GetElementType();

            // Parse and assign the basic type value to the property
            try
            {
                var arr = Array.CreateInstance(itemType, propertyValue.Cast<object>().Count());

                var i = 0;
                foreach (var value in propertyValue)
                {
                    object itemvalue;
                    if (Constants.BasicTypesInfo[itemType].TryParse(value.ToString(), out itemvalue))
                        arr.SetValue(itemvalue, i++);
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
            if (string.IsNullOrWhiteSpace(propertyName)) return;

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

        private static IList ParseArray(Type type, string source)
        {
            var result = (IList) Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
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
                        if (currentChar == StringQuotedCharacter)
                        {
                            currentState = ReadState.PushingValue;
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
                        if (currentChar == StringQuotedCharacter ||
                            currentChar == FieldSeparatorCharacter)
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
                }
            }

            return result;
        }

        private static T ParseObject<T>(string source)
        {
            var result = Activator.CreateInstance<T>();
            var props = GetTypeProperties(result.GetType()).Where(x => x.CanWrite);

            var currentState = ReadState.WaitingForNewField;
            var currentPropertyName = new StringBuilder(1024);
            var currentValue = new StringBuilder(1024);

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
                        SetPropertyValue(props, currentPropertyName.ToString(), currentValue.ToString(), result);
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
                            currentState = ReadState.PushingValue;
                        }
                        else if (currentChar == MinusNumberCharacter ||
                                 char.IsNumber(currentChar))
                        {
                            currentState = ReadState.PushingValue;
                            currentValue.Append(currentChar);
                        }
                        else if (currentChar == TrueValue[0] && nextChar.HasValue && nextChar == TrueValue[1])
                        {
                            SetPropertyValue(props, currentPropertyName.ToString(), true.ToStringInvariant(),
                                result);
                            currentPropertyName.Clear();
                            charIndex += TrueValue.Length;
                            
                            currentState = ReadState.WaitingForNewField;
                        }
                        else if (currentChar == FalseValue[0] && nextChar.HasValue && nextChar == FalseValue[1])
                        {
                            SetPropertyValue(props, currentPropertyName.ToString(), false.ToStringInvariant(),
                                result);
                            currentPropertyName.Clear();
                            charIndex += FalseValue.Length;

                            currentState = ReadState.WaitingForNewField;
                        }
                        else if (currentChar == NullValue[0] && nextChar.HasValue && nextChar == NullValue[1])
                        {
                            SetPropertyValue(props, currentPropertyName.ToString(), null, result);
                            currentPropertyName.Clear();
                            charIndex += NullValue.Length;

                            currentState = ReadState.WaitingForNewField;
                        }
                        else if (currentChar == InitialArrayCharacter)
                        {
                            currentState = ReadState.WaitingForArrayEnd;
                            currentValue.Append(currentChar);
                        }
                        break;
                    }
                    case ReadState.WaitingForArrayEnd:
                    {
                        if (currentChar == FinalArrayCharacter)
                        {
                            currentValue.Append(currentChar);
                            var array = ParseArray(typeof(string), currentValue.ToString());
                            SetPropertyValue(props, currentPropertyName.ToString(), array, result);
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
                        if (currentChar == StringQuotedCharacter ||
                            currentChar == FieldSeparatorCharacter)
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

            return result;
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