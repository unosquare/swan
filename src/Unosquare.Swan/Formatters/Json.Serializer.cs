namespace Unosquare.Swan.Formatters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public partial class Json
    {
        /// <summary>
        /// A simple JSON serializer
        /// </summary>
        private class Serializer
        {
            #region Private Declarations

            private static readonly Dictionary<int, string> IndentStrings = new Dictionary<int, string>();

            private readonly string Result;
            private readonly StringBuilder Builder;
            private readonly bool Format;
            private readonly string LastCommaSearch;
            private readonly List<string> ExcludeProperties = new List<string>();
            private readonly List<string> IncludeProperties = new List<string>();

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="Serializer" /> class.
            /// </summary>
            /// <param name="obj">The object.</param>
            /// <param name="depth">The depth.</param>
            /// <param name="format">if set to <c>true</c> [format].</param>
            /// <param name="typeSpecifier">The type specifier. Leave null or empty to skip.</param>
            /// <param name="includeProperties">The include properties.</param>
            /// <param name="excludeProperties">The exclude properties.</param>
            /// <param name="includeNonPublic">if set to <c>true</c> [include non public].</param>
            /// <param name="parentReferences">The parent references.</param>
            private Serializer(object obj, int depth, bool format, string typeSpecifier, string[] includeProperties, string[] excludeProperties, bool includeNonPublic, List<WeakReference> parentReferences)
            {
                #region Property Settings

                if (includeProperties != null && includeProperties.Length > 0)
                    IncludeProperties.AddRange(includeProperties);

                if (excludeProperties != null && excludeProperties.Length > 0)
                    ExcludeProperties.AddRange(excludeProperties);

                Format = format;
                LastCommaSearch = FieldSeparatorChar + (Format ? Environment.NewLine : string.Empty);

                #endregion

                #region Basic Type Handling (nulls, strings and bools)

                if (obj == null)
                {
                    Result = depth == 0 ? EmptyObjectLiteral : NullLiteral;
                    return;
                }

                if (obj is string)
                {
                    Result = $"{StringQuotedChar}{Escape((string)obj)}{StringQuotedChar}";
                    return;
                }

                if (obj is Type || obj is Assembly || obj is MethodInfo || obj is PropertyInfo || obj is EventInfo)
                {
                    Result = $"{StringQuotedChar}{Escape(obj.ToString())}{StringQuotedChar}";
                    return;
                }

                if (obj is bool)
                {
                    Result = ((bool)obj) ? TrueLiteral : FalseLiteral;
                    return;
                }

                #endregion

                #region Extended Type Handling (numbers and other fundamental types)

                var target = obj;
                var targetType = obj.GetType();

                if (Definitions.BasicTypesInfo.ContainsKey(targetType))
                {
                    if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                    {
                        var date = targetType == typeof(DateTime) ? (DateTime)target : ((DateTime?)target).Value;

                        Result = $"{StringQuotedChar}{date:s}{StringQuotedChar}";
                    }
                    else
                    {
                        var escapedValue = Escape(Definitions.BasicTypesInfo[targetType].ToStringInvariant(target));
                        decimal val;

                        Result = decimal.TryParse(escapedValue, out val) ?
                            $"{escapedValue}" :
                            $"{StringQuotedChar}{escapedValue}{StringQuotedChar}";
                    }

                    return;
                }

                // At this point, we will need to construct the object with a stringbuilder.
                Builder = new StringBuilder();

                // Handle circular references correctly and avoid them
                if (parentReferences == null)
                    parentReferences = new List<WeakReference>();

                if (parentReferences.Any(p => ReferenceEquals(p.Target, obj)))
                {
                    Result = $"{{ \"$circref\": \"{Escape(obj.GetHashCode().ToStringInvariant())}\" }}";
                    return;
                }

                parentReferences.Add(new WeakReference(obj));

                #endregion

                #region Dictionary Type Handling (IDictionary)
                {
                    if (target is IDictionary)
                    {
                        // Cast the items as an IDictionary
                        var items = (IDictionary)target;

                        // Append the start of an object or empty object
                        if (items.Count > 0)
                        {
                            Append(OpenObjectChar, depth);
                            AppendLine();
                        }
                        else
                        {
                            Result = EmptyObjectLiteral;
                            return;
                        }

                        // Iterate through the elements and output recursively
                        var writeCount = 0;
                        foreach (DictionaryEntry entry in items)
                        {
                            // Serialize and append the key
                            Append($"{StringQuotedChar}{Escape(entry.Key.ToString())}{StringQuotedChar}{ValueSeparatorChar} ", depth + 1);

                            // Serialize and append the value
                            var serializedValue = Serialize(entry.Value, depth + 1, Format, typeSpecifier, includeProperties, excludeProperties, includeNonPublic, parentReferences);
                            if (IsNonEmptyJsonArrayOrObject(serializedValue)) AppendLine();
                            Append(serializedValue, 0);

                            // Add a comma and start a new line -- We will remove the last one when we are done writing the elements
                            Append(FieldSeparatorChar, 0);
                            AppendLine();
                            writeCount++;
                        }

                        // Output the end of the object and set the result
                        RemoveLastComma();
                        Append(CloseObjectChar, writeCount > 0 ? depth : 0);
                        Result = Builder.ToString();
                        return;
                    }
                }

                #endregion

                #region Enumerable Type Handling (IEnumerable)
                {
                    if (target is IEnumerable)
                    {
                        // Special byte array handling
                        if (target is byte[])
                        {
                            Result = Serialize((target as byte[]).ToBase64(), depth, Format, typeSpecifier, includeProperties, excludeProperties, includeNonPublic, parentReferences);
                            return;
                        }

                        // Cast the items as a generic object array
                        var items = ((IEnumerable)target).Cast<object>().ToArray();

                        // Append the start of an array or empty array
                        if (items.Length > 0)
                        {
                            Append(OpenArrayChar, depth);
                            AppendLine();
                        }
                        else
                        {
                            Result = EmptyArrayLiteral;
                            return;
                        }

                        // Iterate through the elements and output recursively
                        var writeCount = 0;
                        foreach (var entry in items)
                        {
                            var serializedValue = Serialize(entry, depth + 1, Format, typeSpecifier, includeProperties, excludeProperties, includeNonPublic, parentReferences);

                            if (IsNonEmptyJsonArrayOrObject(serializedValue))
                                Append(serializedValue, 0);
                            else
                                Append(serializedValue, depth + 1);

                            Append(FieldSeparatorChar, 0);
                            AppendLine();
                            writeCount++;
                        }

                        // Output the end of the array and set the result
                        RemoveLastComma();
                        Append(CloseArrayChar, writeCount > 0 ? depth : 0);
                        Result = Builder.ToString();
                        return;
                    }
                }

                #endregion

                #region All Other Types Handling

                {
                    // If we arrive here, then we convert the object into a 
                    // dictionary of property names and values and call the serialization
                    // function again
                    var objectDictionary = CreateDictionary(typeSpecifier, includeNonPublic, targetType, target);

                    // At this point we either have a dictionary with or without properties
                    // If we have at least one property then we send it through the serialization method
                    // If we don't have any properties we simply call its ToString() method and serialize as string
                    if (objectDictionary.Count > 0)
                        Result = Serialize(objectDictionary, depth, Format, typeSpecifier, includeProperties, excludeProperties, includeNonPublic, parentReferences);
                    else
                        Result = Serialize(target.ToString(), 0, Format, typeSpecifier, includeProperties, excludeProperties, includeNonPublic, parentReferences);
                }
                #endregion
            }

            /// <summary>
            /// Serializes the specified object.
            /// </summary>
            /// <param name="obj">The object.</param>
            /// <param name="depth">The depth.</param>
            /// <param name="format">if set to <c>true</c> [format].</param>
            /// <param name="typeSepcifier">The type sepcifier. Leave empty to avoid setting.</param>
            /// <param name="includeProperties">The include properties.</param>
            /// <param name="excludeProperties">The exclude properties.</param>
            /// <param name="includeNonPublic">if set to true, then non public properties are also retrieved</param>
            /// <param name="parentReferences">The parent references.</param>
            /// <returns></returns>
            public static string Serialize(object obj, int depth, bool format, string typeSepcifier, string[] includeProperties, string[] excludeProperties, bool includeNonPublic, List<WeakReference> parentReferences)
            {
                var serializer = new Serializer(obj, depth, format, typeSepcifier, includeProperties, excludeProperties, includeNonPublic, parentReferences);
                return serializer.Result;
            }

            #endregion

            #region Helper Methods

            /// <summary>
            /// Creates the dictionary of values from a target.
            /// </summary>
            /// <param name="typeSpecifier">The type specifier.</param>
            /// <param name="includeNonPublic">if set to <c>true</c> [include non public].</param>
            /// <param name="targetType">Type of the target.</param>
            /// <param name="target">The target.</param>
            /// <returns></returns>
            private Dictionary<string, object> CreateDictionary(string typeSpecifier, bool includeNonPublic, Type targetType, object target)
            {
                // Create the dictionary and extract the properties
                var objectDictionary = new Dictionary<string, object>();

                var fields = new List<MemberInfo>();

                // If the target is a struct (value type) navigate the fields.
                if (targetType.IsValueType())
                {
                    fields.AddRange(RetrieveFields(targetType));
                }

                // then incorporate the properties
                fields.AddRange(RetrieveProperties(targetType).Where(p => p.CanRead).ToArray());

                // If we set the included properties, then we remove everything that is not listed
                if (IncludeProperties.Count > 0)
                    fields = fields.Where(p => IncludeProperties.Contains(p.Name)).ToList();

                if (string.IsNullOrWhiteSpace(typeSpecifier) == false)
                    objectDictionary[typeSpecifier] = targetType.ToString();

                foreach (var field in fields)
                {
                    // Skip over the excluded properties
                    if (ExcludeProperties.Count > 0 && ExcludeProperties.Contains(field.Name))
                        continue;

                    // Build the dictionary using property names and values
                    // Note: used to be: property.GetValue(target); but we would be reading private properties
                    try
                    {
                        objectDictionary[
                                field.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? field.Name] =
                            field is PropertyInfo
                                ? (field as PropertyInfo).GetGetMethod(includeNonPublic)?.Invoke(target, null)
                                : (field as FieldInfo).GetValue(target);
                    }
                    catch 
                    ////(Exception ex)
                    {
                        /* ignored */
                    }
                }

                return objectDictionary;
            }

            /// <summary>
            /// Gets the indent string given the depth.
            /// </summary>
            /// <param name="depth">The depth.</param>
            /// <returns></returns>
            private string GetIndentString(int depth)
            {
                if (Format == false) return string.Empty;

                if (depth > 0 && IndentStrings.ContainsKey(depth) == false)
                    IndentStrings[depth] = new string(' ', depth * 4);

                var indent = depth > 0 ? IndentStrings[depth] : string.Empty;
                return indent;
            }

            /// <summary>
            /// Determines whether the specified serialized JSON is a non-empty an array or an object
            /// </summary>
            /// <param name="serialized">The serialized.</param>
            /// <returns>
            ///   <c>true</c> if [is set opening] [the specified serialized]; otherwise, <c>false</c>.
            /// </returns>
            private static bool IsNonEmptyJsonArrayOrObject(string serialized)
            {
                if (serialized.Length == EmptyObjectLiteral.Length && serialized.Equals(EmptyObjectLiteral)) return false;
                if (serialized.Length == EmptyArrayLiteral.Length && serialized.Equals(EmptyArrayLiteral)) return false;

                // find the first position the character is not a space
                var startTextIndex = serialized.TakeWhile(c => c == ' ').Count();

                // If the position is opening braces or brackets, then we have an
                // opening set.
                return serialized[startTextIndex] == OpenObjectChar
                    || serialized[startTextIndex] == OpenArrayChar;
            }

            /// <summary>
            /// Removes the last comma in the current string builder.
            /// </summary>
            private void RemoveLastComma()
            {
                if (Builder.Length < LastCommaSearch.Length)
                    return;

                if (LastCommaSearch.Where((t, i) => Builder[Builder.Length - LastCommaSearch.Length + i] != t).Any())
                {
                    return;
                }

                // If we got this far, we simply remove the comma character
                Builder.Remove(Builder.Length - LastCommaSearch.Length, 1);
            }

            /// <summary>
            /// Appends the specified text to the output StringBuilder.
            /// </summary>
            /// <param name="text">The text.</param>
            /// <param name="depth">The depth.</param>
            private void Append(string text, int depth)
            {
                Builder.Append($"{GetIndentString(depth)}{text}");
            }

            /// <summary>
            /// Appends the specified text to the output StringBuilder.
            /// </summary>
            /// <param name="text">The text.</param>
            /// <param name="depth">The depth.</param>
            private void Append(char text, int depth)
            {
                Builder.Append($"{GetIndentString(depth)}{text}");
            }

            /// <summary>
            /// Appends a line to the output stringbuilder.
            /// </summary>
            private void AppendLine()
            {
                if (Format == false) return;
                Builder.Append(Environment.NewLine);
            }

            /// <summary>
            /// Escapes the specified string as a JSON string.
            /// </summary>
            /// <param name="str">The string to escape.</param>
            /// <returns></returns>
            private static string Escape(string str)
            {
                if (string.IsNullOrEmpty(str))
                    return string.Empty;

                var builder = new StringBuilder(str.Length * 2);

                foreach (var currentChar in str)
                {
                    switch (currentChar)
                    {
                        case '\\':
                        case '"':
                        case '/':
                            builder.Append('\\');
                            builder.Append(currentChar);
                            break;
                        case '\b':
                            builder.Append("\\b");
                            break;
                        case '\t':
                            builder.Append("\\t");
                            break;
                        case '\n':
                            builder.Append("\\n");
                            break;
                        case '\f':
                            builder.Append("\\f");
                            break;
                        case '\r':
                            builder.Append("\\r");
                            break;
                        default:
                            if (currentChar < ' ')
                            {
                                var escapeBytes = BitConverter.GetBytes((ushort)currentChar);
                                if (BitConverter.IsLittleEndian == false)
                                    Array.Reverse(escapeBytes);

                                builder.Append("\\u"
                                    + escapeBytes[1].ToString("X").PadLeft(2, '0')
                                    + escapeBytes[0].ToString("X").PadLeft(2, '0'));
                            }
                            else
                            {
                                builder.Append(currentChar);
                            }

                            break;
                    }
                }

                return builder.ToString();
            }

            #endregion
        }
    }
}
