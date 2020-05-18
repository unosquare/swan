using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Swan.Reflection;

namespace Swan.Formatters
{
    /// <summary>
    /// A very simple, light-weight JSON library written by Mario
    /// to teach Geo how things are done
    /// 
    /// This is an useful helper for small tasks but it doesn't represent a full-featured
    /// serializer such as the beloved Json.NET.
    /// </summary>
    public partial class Json
    {
        /// <summary>
        /// A simple JSON serializer.
        /// </summary>
        private class Serializer
        {
            #region Private Declarations

            private static readonly Dictionary<int, string> IndentStrings = new Dictionary<int, string>();

            private readonly SerializerOptions _options;
            private readonly string _result;
            private readonly StringBuilder _builder;
            private readonly string _lastCommaSearch;
            private readonly string[]? _excludedNames = null;

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="Serializer" /> class.
            /// </summary>
            /// <param name="obj">The object.</param>
            /// <param name="depth">The depth.</param>
            /// <param name="options">The options.</param>
            private Serializer(object? obj, int depth, SerializerOptions options, string[]? excludedNames = null)
            {
                if (depth > 20)
                {
                    throw new InvalidOperationException(
                        "The max depth (20) has been reached. Serializer can not continue.");
                }

                // Basic Type Handling (nulls, strings, number, date and bool)
                _result = ResolveBasicType(obj);

                if (!string.IsNullOrWhiteSpace(_result))
                    return;

                _options = options;
                _excludedNames ??= excludedNames;
                _options.ExcludeProperties = GetExcludedNames(obj?.GetType(), _excludedNames);

                // Handle circular references correctly and avoid them
                if (options.IsObjectPresent(obj!))
                {
                    _result = $"{{ \"$circref\": \"{Escape(obj!.GetHashCode().ToStringInvariant(), false)}\" }}";
                    return;
                }

                // At this point, we will need to construct the object with a StringBuilder.
                _lastCommaSearch = FieldSeparatorChar + (_options.Format ? Environment.NewLine : string.Empty);
                _builder = new StringBuilder();

                _result = obj switch
                {
                    IDictionary itemsZero when itemsZero.Count == 0 => EmptyObjectLiteral,
                    IDictionary items => ResolveDictionary(items, depth),
                    IEnumerable enumerableZero when !enumerableZero.Cast<object>().Any() => EmptyArrayLiteral,
                    IEnumerable enumerableBytes when enumerableBytes is byte[] bytes => Serialize(bytes.ToBase64(), depth, _options, _excludedNames),
                    IEnumerable enumerable => ResolveEnumerable(enumerable, depth),
                    _ => ResolveObject(obj!, depth)
                };
            }

            internal static string Serialize(object? obj, int depth, SerializerOptions options, string[]? excludedNames = null) => new Serializer(obj, depth, options, excludedNames)._result;

            #endregion

            #region Helper Methods
            internal static string[]? GetExcludedNames(Type? type, string[]? excludedNames)
            {
                if (type == null)
                    return excludedNames;

                var excludedByAttr = IgnoredPropertiesCache.Retrieve(type, t => t.GetProperties()
                    .Where(x => AttributeCache.DefaultCache.Value.RetrieveOne<JsonPropertyAttribute>(x)?.Ignored == true)
                    .Select(x => x.Name));

                if (excludedByAttr?.Any() != true)
                    return excludedNames;

                return excludedNames?.Any(string.IsNullOrWhiteSpace) == true
                    ? excludedByAttr.Intersect(excludedNames.Where(y => !string.IsNullOrWhiteSpace(y))).ToArray()
                    : excludedByAttr.ToArray();
            }

            private static string ResolveBasicType(object? obj)
            {
                switch (obj)
                {
                    case null:
                        return NullLiteral;
                    case string s:
                        return Escape(s, true);
                    case bool b:
                        return b ? TrueLiteral : FalseLiteral;
                    case Type _:
                    case Assembly _:
                    case MethodInfo _:
                    case PropertyInfo _:
                    case EventInfo _:
                        return Escape(obj.ToString(), true);
                    case DateTime d:
                        return $"{StringQuotedChar}{d:s}{StringQuotedChar}";
                    default:
                        var targetType = obj.GetType();

                        if (!Definitions.BasicTypesInfo.Value.ContainsKey(targetType))
                            return string.Empty;

                        var escapedValue = Escape(Definitions.BasicTypesInfo.Value[targetType].ToStringInvariant(obj), false);

                        return decimal.TryParse(escapedValue, out _)
                            ? $"{escapedValue}"
                            : $"{StringQuotedChar}{escapedValue}{StringQuotedChar}";
                }
            }

            private static bool IsNonEmptyJsonArrayOrObject(string serialized)
            {
                if (serialized == EmptyObjectLiteral || serialized == EmptyArrayLiteral) return false;

                // find the first position the character is not a space
                return serialized.Where(c => c != ' ').Select(c => c == OpenObjectChar || c == OpenArrayChar).FirstOrDefault();
            }

            private static string Escape(string str, bool quoted)
            {
                if (str == null)
                    return string.Empty;

                var builder = new StringBuilder(str.Length * 2);
                if (quoted) builder.Append(StringQuotedChar);
                Escape(str, builder);
                if (quoted) builder.Append(StringQuotedChar);
                return builder.ToString();
            }

            private static void Escape(string str, StringBuilder builder)
            {
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

                                builder.Append("\\u")
                                    .Append(escapeBytes[1].ToString("X", CultureInfo.InvariantCulture).PadLeft(2, '0'))
                                    .Append(escapeBytes[0].ToString("X", CultureInfo.InvariantCulture).PadLeft(2, '0'));
                            }
                            else
                            {
                                builder.Append(currentChar);
                            }

                            break;
                    }
                }
            }

            private Dictionary<string, object?> CreateDictionary(
                Dictionary<string, MemberInfo> fields,
                string targetType,
                object target)
            {
                // Create the dictionary and extract the properties
                var objectDictionary = new Dictionary<string, object?>();

                if (!string.IsNullOrWhiteSpace(_options.TypeSpecifier))
                    objectDictionary[_options.TypeSpecifier!] = targetType;

                foreach (var field in fields)
                {
                    // Build the dictionary using property names and values
                    // Note: used to be: property.GetValue(target); but we would be reading private properties
                    try
                    {
                        objectDictionary[field.Key] = field.Value is PropertyInfo property
                            ? target.ReadProperty(property.Name)
                            : (field.Value as FieldInfo)?.GetValue(target);
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        /* ignored */
                    }
                }

                return objectDictionary;
            }

            private string ResolveDictionary(IDictionary items, int depth)
            {
                Append(OpenObjectChar, depth);
                AppendLine();

                // Iterate through the elements and output recursively
                var writeCount = 0;
                foreach (var key in items.Keys)
                {
                    // Serialize and append the key (first char indented)
                    Append(StringQuotedChar, depth + 1);
                    Escape(key.ToString(), _builder);
                    _builder
                        .Append(StringQuotedChar)
                        .Append(ValueSeparatorChar)
                        .Append(" ");

                    // Serialize and append the value
                    var serializedValue = Serialize(items[key], depth + 1, _options, _excludedNames);

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

            private string ResolveObject(object target, int depth)
            {
                var targetType = target.GetType();

                if (targetType.IsEnum)
                    return Convert.ToInt64(target, CultureInfo.InvariantCulture).ToStringInvariant();

                var fields = _options.GetProperties(targetType);

                if (fields.Count == 0 && string.IsNullOrWhiteSpace(_options.TypeSpecifier))
                    return EmptyObjectLiteral;

                // If we arrive here, then we convert the object into a 
                // dictionary of property names and values and call the serialization
                // function again
                var objectDictionary = CreateDictionary(fields, targetType.ToString(), target);

                return Serialize(objectDictionary, depth, _options, _excludedNames);
            }

            private string ResolveEnumerable(IEnumerable target, int depth)
            {
                // Cast the items as a generic object array
                var items = target.Cast<object>();

                Append(OpenArrayChar, depth);
                AppendLine();

                // Iterate through the elements and output recursively
                var writeCount = 0;
                foreach (var entry in items)
                {
                    var serializedValue = Serialize(entry, depth + 1, _options, _excludedNames);

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

            private void SetIndent(int depth)
            {
                if (_options.Format == false || depth <= 0) return;

                _builder.Append(IndentStrings.GetOrAdd(depth, x => new string(' ', x * 4)));
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

            private void Append(string text, int depth)
            {
                SetIndent(depth);
                _builder.Append(text);
            }

            private void Append(char text, int depth)
            {
                SetIndent(depth);
                _builder.Append(text);
            }

            private void AppendLine()
            {
                if (_options.Format == false) return;
                _builder.Append(Environment.NewLine);
            }
            #endregion
        }
    }
}
