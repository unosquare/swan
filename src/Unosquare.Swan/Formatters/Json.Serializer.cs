namespace Unosquare.Swan.Formatters
{
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
    public partial class Json
    {
        private class SerializerOptions
        {
            public string TypeSpecifier { get; set; }
            public string[] IncludeProperties { get; set; }
            public string[] ExcludeProperties { get; set; }
            public bool IncludeNonPublic { get; set; }
            public List<WeakReference> ParentReferences { get; set; } = new List<WeakReference>();
        }

        /// <summary>
        /// A simple JSON serializer
        /// </summary>
        private class Serializer
        {
            #region Private Declarations

            private static readonly Dictionary<int, string> IndentStrings = new Dictionary<int, string>();

            private readonly string _result;
            private readonly StringBuilder _builder;
            private readonly bool _format;
            private readonly string _lastCommaSearch;
            private readonly List<string> _excludeProperties = new List<string>();
            private readonly List<string> _includeProperties = new List<string>();

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="Serializer" /> class.
            /// </summary>
            /// <param name="obj">The object.</param>
            /// <param name="depth">The depth.</param>
            /// <param name="format">if set to <c>true</c> [format].</param>
            /// <param name="options">The options.</param>
            private Serializer(object obj, int depth, bool format, SerializerOptions options)
            {
                // Basic Type Handling (nulls, strings and bool)
                _result = ResolveBasicType(obj, depth);

                if (string.IsNullOrWhiteSpace(_result) == false)
                    return;

                if (options.IncludeProperties != null && options.IncludeProperties.Length > 0)
                    _includeProperties.AddRange(options.IncludeProperties);

                if (options.ExcludeProperties != null && options.ExcludeProperties.Length > 0)
                    _excludeProperties.AddRange(options.ExcludeProperties);

                _format = format;
                _lastCommaSearch = FieldSeparatorChar + (_format ? Environment.NewLine : string.Empty);

                var targetType = obj.GetType();

                // Number or DateTime
                _result = ResolveDateTimeOrNumber(obj, targetType);

                if (string.IsNullOrWhiteSpace(_result) == false)
                    return;

                var target = obj;

                // At this point, we will need to construct the object with a StringBuilder.
                _builder = new StringBuilder();

                // Handle circular references correctly and avoid them
                if (options.ParentReferences.Any(p => ReferenceEquals(p.Target, obj)))
                {
                    _result = $"{{ \"$circref\": \"{Escape(obj.GetHashCode().ToStringInvariant())}\" }}";
                    return;
                }

                options.ParentReferences.Add(new WeakReference(obj));

                // Dictionary Type Handling (IDictionary)
                _result = ResolveDictionary(obj, depth, options);

                if (string.IsNullOrWhiteSpace(_result) == false)
                    return;

                // Enumerable Type Handling (IEnumerable)
                _result = ResolveEnumerable(obj, depth, options);

                if (string.IsNullOrWhiteSpace(_result) == false)
                    return;

                // If we arrive here, then we convert the object into a 
                // dictionary of property names and values and call the serialization
                // function again
                var objectDictionary = CreateDictionary(options.TypeSpecifier, options.IncludeNonPublic, targetType, target);

                // At this point we either have a dictionary with or without properties
                // If we have at least one property then we send it through the serialization method
                // If we don't have any properties we return empty object
                if (objectDictionary.Count == 0)
                {
                    _result = EmptyObjectLiteral;
                    return;
                }

                _result = Serialize(objectDictionary, depth, _format, options);
            }
            
            /// <summary>
            /// Serializes the specified object.
            /// </summary>
            /// <param name="obj">The object.</param>
            /// <param name="depth">The depth.</param>
            /// <param name="format">if set to <c>true</c> [format].</param>
            /// <param name="typeSpecifier">The type specifier. Leave empty to avoid setting.</param>
            /// <param name="includeProperties">The include properties.</param>
            /// <param name="excludeProperties">The exclude properties.</param>
            /// <param name="includeNonPublic">if set to true, then non public properties are also retrieved</param>
            /// <param name="parentReferences">The parent references.</param>
            /// <returns>A <see cref="System.String" /> that represents the current object</returns>
            internal static string Serialize(
                object obj, 
                int depth, 
                bool format, 
                string typeSpecifier, 
                string[] includeProperties, 
                string[] excludeProperties, 
                bool includeNonPublic, 
                List<WeakReference> parentReferences = null)
            {
                var options = new SerializerOptions
                {
                    TypeSpecifier = typeSpecifier,
                    IncludeProperties = includeProperties,
                    ExcludeProperties = excludeProperties,
                    IncludeNonPublic = includeNonPublic
                };

                if (parentReferences != null)
                    options.ParentReferences = parentReferences;

                return Serialize(obj, depth, format, options);
            }

            private static string Serialize(object obj, int depth, bool format, SerializerOptions options)
            {
                return new Serializer(obj, depth, format, options)._result;
            }

            #endregion

            #region Helper Methods

            private static string ResolveBasicType(object obj, int depth)
            {
                if (obj == null)
                {
                    return depth == 0 ? EmptyObjectLiteral : NullLiteral;
                }

                if (obj is string s)
                {
                    return $"{StringQuotedChar}{Escape(s)}{StringQuotedChar}";
                }

                if (obj is bool b)
                {
                    return b ? TrueLiteral : FalseLiteral;
                }

                if (obj is Type || obj is Assembly || obj is MethodInfo || obj is PropertyInfo || obj is EventInfo)
                {
                    return $"{StringQuotedChar}{Escape(obj.ToString())}{StringQuotedChar}";
                }

                return string.Empty;
            }

            private static string ResolveDateTimeOrNumber(object obj, Type targetType)
            {
                if (!Definitions.BasicTypesInfo.ContainsKey(targetType))
                    return string.Empty;

                if (targetType == typeof(DateTime) || targetType == typeof(DateTime?))
                {
                    var date = targetType == typeof(DateTime) ? (DateTime)obj : ((DateTime?)obj).Value;

                    return $"{StringQuotedChar}{date:s}{StringQuotedChar}";
                }

                var escapedValue = Escape(Definitions.BasicTypesInfo[targetType].ToStringInvariant(obj));
                
                return decimal.TryParse(escapedValue, out var val)
                    ? $"{escapedValue}"
                    : $"{StringQuotedChar}{escapedValue}{StringQuotedChar}";
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
            /// Escapes the specified string as a JSON string.
            /// </summary>
            /// <param name="str">The string to escape.</param>
            /// <returns>A <see cref="System.String" /> that represents the current object</returns>
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
                            builder
                                .Append('\\')
                                .Append(currentChar);
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

            private string ResolveDictionary(object target, int depth, SerializerOptions options)
            {
                if (target is IDictionary == false)
                    return string.Empty;

                // Cast the items as an IDictionary
                var items = (IDictionary)target;

                // Append the start of an object or empty object
                if (items.Count <= 0)
                {
                    return EmptyObjectLiteral;
                }

                Append(OpenObjectChar, depth);
                AppendLine();

                // Iterate through the elements and output recursively
                var writeCount = 0;
                foreach (DictionaryEntry entry in items)
                {
                    // Serialize and append the key
                    Append(
                        $"{StringQuotedChar}{Escape(entry.Key.ToString())}{StringQuotedChar}{ValueSeparatorChar} ",
                        depth + 1);

                    // Serialize and append the value
                    var serializedValue = Serialize(entry.Value, depth + 1, _format, options);

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
                return _builder.ToString();
            }

            private string ResolveEnumerable(object target, int depth, SerializerOptions options)
            {
                if (target is IEnumerable == false) return string.Empty;

                // Special byte array handling
                if (target is byte[])
                {
                    return Serialize((target as byte[]).ToBase64(), depth, _format, options);
                }

                // Cast the items as a generic object array
                var items = ((IEnumerable)target).Cast<object>().ToArray();

                // Append the start of an array or empty array
                if (items.Length <= 0)
                {
                    return EmptyArrayLiteral;
                }

                Append(OpenArrayChar, depth);
                AppendLine();

                // Iterate through the elements and output recursively
                var writeCount = 0;
                foreach (var entry in items)
                {
                    var serializedValue = Serialize(entry, depth + 1, _format, options);

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
                return _builder.ToString();
            }

            /// <summary>
            /// Creates the dictionary of values from a target.
            /// </summary>
            /// <param name="typeSpecifier">The type specifier.</param>
            /// <param name="includeNonPublic">if set to <c>true</c> [include non public].</param>
            /// <param name="targetType">Type of the target.</param>
            /// <param name="target">The target.</param>
            /// <returns>Object of the type of the elements in the collection of key/value pairs</returns>
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
                if (_includeProperties.Count > 0)
                    fields = fields.Where(p => _includeProperties.Contains(p.Name)).ToList();

                if (string.IsNullOrWhiteSpace(typeSpecifier) == false)
                    objectDictionary[typeSpecifier] = targetType.ToString();

                foreach (var field in fields)
                {
                    // Skip over the excluded properties
                    if (_excludeProperties.Count > 0 && _excludeProperties.Contains(field.Name))
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
            /// <returns>A <see cref="System.String" /> that represents the current object</returns>
            private string GetIndentString(int depth)
            {
                if (_format == false) return string.Empty;

                if (depth > 0 && IndentStrings.ContainsKey(depth) == false)
                    IndentStrings[depth] = new string(' ', depth * 4);

                return depth > 0 ? IndentStrings[depth] : string.Empty;
            }

            /// <summary>
            /// Removes the last comma in the current string builder.
            /// </summary>
            private void RemoveLastComma()
            {
                if (_builder.Length < _lastCommaSearch.Length)
                    return;

                if (_lastCommaSearch.Where((t, i) => _builder[_builder.Length - _lastCommaSearch.Length + i] != t).Any())
                {
                    return;
                }

                // If we got this far, we simply remove the comma character
                _builder.Remove(_builder.Length - _lastCommaSearch.Length, 1);
            }

            /// <summary>
            /// Appends the specified text to the output StringBuilder.
            /// </summary>
            /// <param name="text">The text.</param>
            /// <param name="depth">The depth.</param>
            private void Append(string text, int depth)
            {
                _builder.Append($"{GetIndentString(depth)}{text}");
            }

            /// <summary>
            /// Appends the specified text to the output StringBuilder.
            /// </summary>
            /// <param name="text">The text.</param>
            /// <param name="depth">The depth.</param>
            private void Append(char text, int depth)
            {
                _builder.Append($"{GetIndentString(depth)}{text}");
            }

            /// <summary>
            /// Appends a line to the output StringBuilder.
            /// </summary>
            private void AppendLine()
            {
                if (_format == false) return;
                _builder.Append(Environment.NewLine);
            }

            #endregion
        }
    }
}
