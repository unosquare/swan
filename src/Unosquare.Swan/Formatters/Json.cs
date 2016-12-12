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
            var currentIsString = false;
            var currentPropertyName = new StringBuilder(1024);
            var currentValue = new StringBuilder(1024);

            // Extract properties from cache
            var properties = TypeCache.Retrieve<T>(() =>
            {
                return typeof(T).GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.CanWrite && Constants.BasicTypesInfo.ContainsKey(x.PropertyType));
            });

            var setPropertyAction = new Action<string, string>((propertyName, propertyStringValue) =>
            {
                if (string.IsNullOrWhiteSpace(propertyName)) return;

                var targetProperty = properties.FirstOrDefault(p => p.Name.Equals(propertyName));

                // Skip if the property is not found
                if (targetProperty == null)
                    return;

                // Parse and assign the basic type value to the property
                try
                {
                    object propertyValue = null;
                    if (Constants.BasicTypesInfo[targetProperty.PropertyType].TryParse(propertyStringValue, out propertyValue))
                        targetProperty.SetValue(result, propertyValue);
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
                            setPropertyAction(currentPropertyName.ToString(), currentValue.ToString());
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

                            break;
                        }
                    case ReadState.PushingValue:
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