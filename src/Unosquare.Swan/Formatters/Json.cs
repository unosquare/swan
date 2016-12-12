using System;
using System.Linq;
using System.Reflection;
using System.Text;
using Unosquare.Swan.Reflection;

namespace Unosquare.Swan.Formatters
{
    public static class Json
    {
        #region Constants 

        const char InitialObjectCharacter = '{';
        const char FinalObjectCharacter = '}';

        const char InitialArrayCharacter = '[';
        const char FinalArrayCharacter = ']';

        const char FieldSeparatorCharacter = ',';
        const char ValueSeparatorCharacter = ':';

        const char StringEscapeCharacter = '\\';
        const char StringQuotedCharacter = '"';
        const char MinusNumberCharacter = '-';

        const string TrueValue = "true";
        const string FalseValue = "false";
        const string NullValue = "null";

        #endregion

        #region Static Variables

        private static readonly object SyncLock = new object();
        private static readonly PropertyTypeCache TypeCache = new PropertyTypeCache();

        #endregion

        #region Enumerations

        /// <summary>
        /// Defines the 3 different read states
        /// for the parsing state machine
        /// </summary>
        private enum ReadState
        {
            WaitingForNewField,
            PushingFieldName,
            WaitingForValue,
            PushingValue
        }

        #endregion

        public static string Serialize(object obj)
        {
            var props = GetTypeProperties(obj.GetType());
            var sb = new StringBuilder();

            foreach (var prop in props)
            {
                var value = prop.GetValue(obj);

                if (value == null)
                {
                    sb.Append($"\"{prop.Name}\" : null, ");
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
                    // fallback to string
                    sb.Append($"\"{prop.Name}\" : \"{value}\", ");
                }
            }

            if (sb.Length > 0)
                sb.Remove(sb.Length - 2, 2);

            return "{" + sb.ToString() + "}";
        }

        public static object Deserialize(string source)
        {
            return null;
        }

        public static T Deserialize<T>(string source)
        {
            var result = Activator.CreateInstance<T>();
            var currentState = ReadState.WaitingForNewField;
            var currentPropertyName = new StringBuilder(1024);
            var currentValue = new StringBuilder(1024);

            // Extract properties from cache
            var properties = TypeCache.Retrieve<T>(() =>
            {
                return typeof(T).GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.CanWrite && Constants.BasicTypesInfo.ContainsKey(x.PropertyType));
            });

            var setPropertyAction = new Action<string, string, bool>((propertyName, propertyStringValue, isNull) =>
            {
                if (string.IsNullOrWhiteSpace(propertyName)) return;

                var targetProperty = properties.FirstOrDefault(p => p.Name.Equals(propertyName));

                // Skip if the property is not found
                if (targetProperty == null)
                    return;

                // Parse and assign the basic type value to the property
                try
                {
                    if (isNull)
                    {
                        targetProperty.SetValue(result, null);
                    }
                    else
                    {
                        object propertyValue = null;
                        if (Constants.BasicTypesInfo[targetProperty.PropertyType].TryParse(propertyStringValue, out propertyValue))
                            targetProperty.SetValue(result, propertyValue);
                    }
                }
                catch
                {
                    // swallow
                }
            });

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
                            setPropertyAction(currentPropertyName.ToString(), currentValue.ToString(), false);
                            currentPropertyName.Clear();
                            currentValue.Clear();

                            if (currentChar == InitialObjectCharacter) continue;

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
                                setPropertyAction(currentPropertyName.ToString(), true.ToStringInvariant(), false);
                                charIndex += TrueValue.Length;

                                currentState = ReadState.WaitingForNewField;
                            }
                            else if (currentChar == FalseValue[0] && nextChar.HasValue && nextChar == FalseValue[1])
                            {
                                setPropertyAction(currentPropertyName.ToString(), false.ToStringInvariant(), false);
                                charIndex += FalseValue.Length;

                                currentState = ReadState.WaitingForNewField;
                            }
                            else if (currentChar == NullValue[0] && nextChar.HasValue && nextChar == NullValue[1])
                            {
                                setPropertyAction(currentPropertyName.ToString(), null, true);
                                charIndex += NullValue.Length;

                                currentState = ReadState.WaitingForNewField;
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

        private static PropertyInfo[] GetTypeProperties(Type type)
        {
            lock (SyncLock)
            {
                return TypeCache.Retrieve(type, () =>
                    {
                        return type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p => p.CanRead)
                            .ToArray();
                    }).ToArray();
            }
        }
    }
}