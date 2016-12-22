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
    /// A very simple JSON library written by Mario
    /// to teach Geo how things are done
    /// </summary>
    public class JsonEx
    {
        #region Constants 

        private const char OpenObjectChar = '{';
        private const char CloseObjectChar = '}';

        private const char OpenArrayChar = '[';
        private const char CloseArrayChar = ']';

        private const char FieldSeparatorChar = ',';
        private const char ValueSeparatorChar = ':';

        private const char StringEscapeChar = '\\';
        private const char StringQuotedChar = '"';
        private const char MinusNumberChar = '-';

        private const string EmptyObjectValue = "{ }";
        private const string EmtpyArrayValue = "[ ]";
        private const string TrueValue = "true";
        private const string FalseValue = "false";
        private const string NullValue = "null";

        #endregion

        #region Private Declarations

        private static readonly Dictionary<int, string> IndentStrings = new Dictionary<int, string>();
        private static readonly PropertyTypeCache TypeCache = new PropertyTypeCache();

        private readonly string Result = null;
        private readonly Type TargetType;
        private readonly StringBuilder Builder;
        private readonly bool Format = true;
        private readonly List<string> ExcludeProperties = new List<string>();
        private readonly List<string> IncludeProperties = new List<string>();

        #endregion

        #region Constructors

        private JsonEx(object obj, int depth, bool format, string[] includeProperties, string[] excludeProperties)
        {
            #region Property Settings

            if (includeProperties != null && includeProperties.Length > 0)
                IncludeProperties.AddRange(includeProperties);

            if (excludeProperties != null && excludeProperties.Length > 0)
                ExcludeProperties.AddRange(excludeProperties);

            Format = format;

            #endregion

            #region Basic Type Handling (nulls, strings and bools)

            if (obj == null)
            {
                Result = depth == 0 ? EmptyObjectValue : NullValue;
                return;
            }

            if (obj is string)
            {
                Result = $"\"{Escape(obj as string)}\"";
                return;
            }

            if (obj is bool)
            {
                Result = ((bool)obj) ? TrueValue : FalseValue;
                return;
            }

            #endregion

            #region Extended Type Handling (numbers and other fundamental types)

            var target = obj;
            TargetType = obj.GetType();

            if (Constants.BasicTypesInfo.ContainsKey(TargetType))
            {
                var literalValue = Escape(Constants.BasicTypesInfo[TargetType].ToStringInvariant(target));
                decimal val;

                if (decimal.TryParse(literalValue, out val))
                    Result = $"{literalValue}";
                else
                    Result = $"{StringQuotedChar}{Escape(literalValue)}{StringQuotedChar}";

                return;
            }

            // At this point, we will need to construct the object with a stringbuilder.
            Builder = new StringBuilder();

            #endregion

            #region Dictionary Type Handling (IDictionary)
            {
                if (target is IDictionary)
                {
                    // Cast the items as an IDictionary
                    var items = target as IDictionary;

                    // Append the start of an object or empty object
                    if (items.Count > 0)
                    {
                        Append(OpenObjectChar, depth);
                        AppendLine();
                    }
                    else
                    {
                        Result = EmptyObjectValue;
                        return;
                    }

                    // Iterate through the elements and output recursively
                    var writeCount = 0;
                    foreach (DictionaryEntry entry in items)
                    {
                        // Serialize and append the key
                        Append($"{StringQuotedChar}{Escape(entry.Key.ToString())}{StringQuotedChar}{ValueSeparatorChar} ", depth + 1);

                        // Serialize and append the value
                        var serializedValue = Serialize(entry.Value, depth + 1, Format, includeProperties, excludeProperties);
                        if (IsSetOpening(serializedValue)) AppendLine();
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
                        Result = Serialize((target as byte[]).ToBase64(), depth, Format, includeProperties, excludeProperties);
                        return;
                    }

                    // Cast the items as a generic object array
                    var items = (target as IEnumerable).Cast<object>().ToArray();

                    // Append the start of an array or empty array
                    if (items.Length > 0)
                    {
                        Append(OpenArrayChar, depth);
                        AppendLine();
                    }
                    else
                    {
                        Result = EmtpyArrayValue;
                        return;
                    }

                    // Iterate through the elements and output recursively
                    var writeCount = 0;
                    foreach (var entry in items)
                    {
                        var serializedValue = Serialize(entry, depth + 1, Format, includeProperties, excludeProperties);

                        if (IsSetOpening(serializedValue))
                        {
                            Append(serializedValue, 0);
                        }
                        else
                        {
                            Append(serializedValue, depth + 1);
                        }

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

                // Create the dictionary and extract the properties
                var objectDictionary = new Dictionary<string, object>();
                var properties = TypeCache.Retrieve(TargetType, () =>
                {
                    return
                    TargetType.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.CanRead).ToArray();
                });

                // If we set the included properties, then we remove everything that is not listed
                if (IncludeProperties.Count > 0)
                    properties = properties.Where(p => IncludeProperties.Contains(p.Name)).ToArray();

                foreach (var property in properties)
                {
                    // Skip over the excluded properties
                    if (ExcludeProperties.Count > 0 && ExcludeProperties.Contains(property.Name))
                        continue;

                    // Build the dictionary using property names and values
                    try { objectDictionary[property.Name] = property.GetValue(target); }
                    catch { /* ignored */ }
                }

                // At this point we either have a dictionary with or without properties
                // If we have at least one property then we send it through the serialization method
                // If we don't have any properties we simply call its tostring method and serialize as string
                if (objectDictionary.Count > 0)
                    Result = Serialize(objectDictionary, depth, Format, includeProperties, excludeProperties);
                else
                    Result = Serialize(target.ToString(), 0, Format, includeProperties, excludeProperties);
            }
            #endregion
        }

        private static string Serialize(object obj, int depth, bool format, string[] includeProperties, string[] excludeProperties)
        {
            var serializer = new JsonEx(obj, depth, format, includeProperties, excludeProperties);
            return serializer.Result;
        }

        #endregion

        #region Helper Methods

        private string GetIndent(int depth)
        {
            if (Format == false) return string.Empty;

            if (depth > 0 && IndentStrings.ContainsKey(depth) == false)
                IndentStrings[depth] = new string(' ', depth * 4);

            var indent = depth > 0 ? IndentStrings[depth] : string.Empty;
            return indent;
        }

        private static bool IsSetOpening(string serialized)
        {
            // find the first position the character is not a space
            var startTextIndex = serialized.TakeWhile(c => c == ' ').Count();

            // If the position is opening braces or brackets, then we have an
            // opening set.
            return serialized[startTextIndex] == OpenObjectChar
                || serialized[startTextIndex] == OpenArrayChar;
        }

        private bool  RemoveLastComma()
        {
            var search = FieldSeparatorChar + (Format ? Environment.NewLine : string.Empty);

            if (Builder.Length < search.Length)
                return false;

            for (var i = 0; i < search.Length; i++)
                if (Builder[Builder.Length - search.Length + i] != search[i])
                    return false;

            // If we got this far, we simply remove the comma character
            Builder.Remove(Builder.Length - search.Length, 1);
            return true;
        }

        private void Append(string text, int depth)
        {
            Builder.Append($"{GetIndent(depth)}{text}");
        }

        private void Append(char text, int depth)
        {
            Builder.Append($"{GetIndent(depth)}{text}");
        }

        private void AppendLine()
        {
            if (Format == false) return;
            Builder.Append(Environment.NewLine);
        }

        private static string Escape(string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            var builder = new StringBuilder(s.Length * 2);

            foreach (var currentChar in s)
            {
                switch (currentChar)
                {
                    case '\\':
                    case '"':
                        builder.Append('\\');
                        builder.Append(currentChar);
                        break;
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
                            var escapeSequence = ((int)currentChar).ToString("X");
                            builder.Append("\\u" + escapeSequence.PadLeft(4, '0'));
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

        #region Public API

        /// <summary>
        /// Serializes the specified object. All properties are serialized
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="format">if set to <c>true</c> it formats and indents the output.</param>
        /// <returns></returns>
        public static string Serialize(object obj, bool format = false)
        {
            return Serialize(obj, 0, format, null, null);
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
            return Serialize(obj, 0, format, includeNames, null);
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
            return Serialize(obj, 0, format, null, excludeNames);
        }

        #endregion

    }
}