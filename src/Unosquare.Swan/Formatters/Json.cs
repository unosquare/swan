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
        private const char MinusNumberChar = '-';

        private const string EmptyObjectValue = "{ }";
        private const string EmtpyArrayValue = "[ ]";
        private const string TrueValue = "true";
        private const string FalseValue = "false";
        private const string NullValue = "null";

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

        public static object Deserialize(string json)
        {
            return Deserializer.Deserialize(json);
        }

        #endregion

        /// <summary>
        /// A simple JSON serializer
        /// </summary>
        private class Serializer
        {

            #region Private Declarations

            private static readonly Dictionary<int, string> IndentStrings = new Dictionary<int, string>();

            private readonly string Result = null;
            private readonly Type TargetType;
            private readonly StringBuilder Builder;
            private readonly bool Format = true;
            private readonly string LastCommaSearch;
            private readonly List<string> ExcludeProperties = new List<string>();
            private readonly List<string> IncludeProperties = new List<string>();

            #endregion

            #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="Serializer"/> class.
            /// </summary>
            /// <param name="obj">The object.</param>
            /// <param name="depth">The depth.</param>
            /// <param name="format">if set to <c>true</c> [format].</param>
            /// <param name="includeProperties">The include properties.</param>
            /// <param name="excludeProperties">The exclude properties.</param>
            private Serializer(object obj, int depth, bool format, string[] includeProperties, string[] excludeProperties)
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
                    Result = depth == 0 ? EmptyObjectValue : NullValue;
                    return;
                }

                if (obj is string)
                {
                    Result = $"{StringQuotedChar}{Escape(obj as string)}{StringQuotedChar}";
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
                            if (IsJsonArrayOrObject(serializedValue)) AppendLine();
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

                            if (IsJsonArrayOrObject(serializedValue))
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

            /// <summary>
            /// Serializes the specified object.
            /// </summary>
            /// <param name="obj">The object.</param>
            /// <param name="depth">The depth.</param>
            /// <param name="format">if set to <c>true</c> [format].</param>
            /// <param name="includeProperties">The include properties.</param>
            /// <param name="excludeProperties">The exclude properties.</param>
            /// <returns></returns>
            static public string Serialize(object obj, int depth, bool format, string[] includeProperties, string[] excludeProperties)
            {
                var serializer = new Serializer(obj, depth, format, includeProperties, excludeProperties);
                return serializer.Result;
            }

            #endregion

            #region Helper Methods

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
            /// Determines whether the specified serialized JSON is an array or an object
            /// </summary>
            /// <param name="serialized">The serialized.</param>
            /// <returns>
            ///   <c>true</c> if [is set opening] [the specified serialized]; otherwise, <c>false</c>.
            /// </returns>
            private static bool IsJsonArrayOrObject(string serialized)
            {
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
            /// <returns></returns>
            private bool RemoveLastComma()
            {
                if (Builder.Length < LastCommaSearch.Length)
                    return false;

                for (var i = 0; i < LastCommaSearch.Length; i++)
                    if (Builder[Builder.Length - LastCommaSearch.Length + i] != LastCommaSearch[i])
                        return false;

                // If we got this far, we simply remove the comma character
                Builder.Remove(Builder.Length - LastCommaSearch.Length, 1);
                return true;
            }

            /// <summary>
            /// Appends the specified text to the output stringbuilder.
            /// </summary>
            /// <param name="text">The text.</param>
            /// <param name="depth">The depth.</param>
            private void Append(string text, int depth)
            {
                Builder.Append($"{GetIndentString(depth)}{text}");
            }

            /// <summary>
            /// Appends the specified text to the output stringbuilder.
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

        }

        private class Deserializer
        {
            private enum ReadState
            {
                WaitingForRootOpen,
                WaitingForField,
                WaitingForColon,
                WaitingForValue,
                WaitingForNextOrRootClose,

            }

            private object Result = null;
            private int EndIndex = 0;

            private Deserializer(string json, int startIndex)
            {

                var state = ReadState.WaitingForRootOpen;
                Dictionary<string, object> resultObject = null;
                List<object> resultArray = null;
                string currentFieldName = null;

                for (var i = startIndex; i < json.Length; i++)
                {

                    #region Wait for { or [
                    if (state == ReadState.WaitingForRootOpen)
                    {
                        if (char.IsWhiteSpace(json, i)) continue;

                        if (json[i] == OpenObjectChar)
                        {
                            resultObject = new Dictionary<string, object>();
                            state = ReadState.WaitingForField;
                            continue;
                        }

                        if (json[i] == OpenArrayChar)
                        {
                            resultArray = new List<object>();
                            state = ReadState.WaitingForValue;
                            continue;
                        }

                        throw new FormatException($"Parser error (char {i}, state {state}): Expected '{OpenObjectChar}' or '{OpenArrayChar}' but got '{json[i]}'.");
                    }

                    #endregion

                    #region Wait for opening field " (only applies for object results)

                    if (state == ReadState.WaitingForField)
                    {
                        if (char.IsWhiteSpace(json, i)) continue;

                        if (json[i] == StringQuotedChar)
                        {

                            var charCount = 0;
                            for (var j = i + 1; j < json.Length; j++)
                            {
                                if (json[j] == StringQuotedChar && json[j - 1] != StringEscapeChar)
                                    break;

                                charCount++;
                            }

                            currentFieldName = Unescape(json.SafeSubstring(i + 1, charCount));
                            i += charCount + 1;
                            state = ReadState.WaitingForColon;
                            continue;
                        }

                        throw new FormatException($"Parser error (char {i}, state {state}): Expected '{StringQuotedChar}' but got '{json[i]}'.");
                    }

                    #endregion

                    #region Wait for field-value separator : (only applies for object results

                    if (state == ReadState.WaitingForColon)
                    {
                        if (char.IsWhiteSpace(json, i)) continue;

                        if (json[i] == ValueSeparatorChar)
                        {
                            state = ReadState.WaitingForValue;
                            continue;
                        }

                        throw new FormatException($"Parser error (char {i}, state {state}): Expected '{ValueSeparatorChar}' but got '{json[i]}'.");
                    }

                    #endregion

                    #region Wait for and Parse the value

                    if (state == ReadState.WaitingForValue)
                    {
                        if (char.IsWhiteSpace(json, i)) continue;

                        // determine the value based on what it starts with
                        switch (json[i])
                        {
                            case StringQuotedChar: // expect a string
                                {
                                    var charCount = 0;
                                    for (var j = i + 1; j < json.Length; j++)
                                    {
                                        if (json[j] == StringQuotedChar && json[j - 1] != StringEscapeChar)
                                            break;

                                        charCount++;
                                    }

                                    // Extract and set the value
                                    var value = Unescape(json.SafeSubstring(i + 1, charCount));
                                    if (currentFieldName != null)
                                        resultObject[currentFieldName] = value;
                                    else
                                        resultArray.Add(value);

                                    // Update state variables
                                    i += charCount + 1;
                                    currentFieldName = null;
                                    state = ReadState.WaitingForNextOrRootClose;
                                    continue;
                                }
                            case OpenObjectChar: // expect object
                            case OpenArrayChar: // expect array
                                {
                                    // Extract and set the value
                                    var deserializer = new Deserializer(json, i);
                                    if (currentFieldName != null)
                                        resultObject[currentFieldName] = deserializer.Result;
                                    else
                                        resultArray.Add(deserializer.Result);

                                    // Update state variables
                                    i = deserializer.EndIndex;
                                    currentFieldName = null;
                                    state = ReadState.WaitingForNextOrRootClose;
                                    continue;
                                }
                            case 't': // expect true
                                {
                                    if (json.SafeSubstring(i, TrueValue.Length).Equals(TrueValue))
                                    {
                                        // Extract and set the value
                                        if (currentFieldName != null)
                                            resultObject[currentFieldName] = true;
                                        else
                                            resultArray.Add(true);

                                        // Update state variables
                                        i = TrueValue.Length - 1;
                                        currentFieldName = null;
                                        state = ReadState.WaitingForNextOrRootClose;
                                        continue;
                                    }

                                    throw new FormatException($"Parser error (char {i}, state {state}): Expected '{ValueSeparatorChar}' but got '{json.SafeSubstring(i, TrueValue.Length)}'.");
                                }
                            case 'f': // expect false
                                {
                                    if (json.SafeSubstring(i, FalseValue.Length).Equals(FalseValue))
                                    {
                                        // Extract and set the value
                                        if (currentFieldName != null)
                                            resultObject[currentFieldName] = false;
                                        else
                                            resultArray.Add(false);

                                        // Update state variables
                                        i = FalseValue.Length - 1;
                                        currentFieldName = null;
                                        state = ReadState.WaitingForNextOrRootClose;
                                        continue;
                                    }

                                    throw new FormatException($"Parser error (char {i}, state {state}): Expected '{ValueSeparatorChar}' but got '{json.SafeSubstring(i, FalseValue.Length)}'.");
                                }
                            case 'n': // expect null
                                {
                                    if (json.SafeSubstring(i, NullValue.Length).Equals(NullValue))
                                    {
                                        // Extract and set the value
                                        if (currentFieldName != null)
                                            resultObject[currentFieldName] = null;
                                        else
                                            resultArray.Add(null);

                                        // Update state variables
                                        i = NullValue.Length - 1;
                                        currentFieldName = null;
                                        state = ReadState.WaitingForNextOrRootClose;
                                        continue;
                                    }

                                    throw new FormatException($"Parser error (char {i}, state {state}): Expected '{ValueSeparatorChar}' but got '{json.SafeSubstring(i, NullValue.Length)}'.");
                                }
                            default: // expect number
                                {
                                    var charCount = 0;
                                    for (var j = i; j < json.Length; j++)
                                    {
                                        if (char.IsWhiteSpace(json[j]) || json[j] == FieldSeparatorChar)
                                            break;

                                        charCount++;
                                    }

                                    // Extract and set the value
                                    var stringValue = json.SafeSubstring(i, charCount);
                                    decimal value = 0M;

                                    if (decimal.TryParse(stringValue, out value) == false)
                                        throw new FormatException($"Parser error (char {i}, state {state}): Expected [number] but got '{stringValue}'.");

                                    if (currentFieldName != null)
                                        resultObject[currentFieldName] = value;
                                    else
                                        resultArray.Add(value);

                                    // Update state variables
                                    i += charCount;
                                    currentFieldName = null;
                                    state = ReadState.WaitingForNextOrRootClose;
                                    continue;
                                }
                        }

                    }

                    #endregion

                    #region Wait for closing ], } or an additional field or value ,

                    if (state == ReadState.WaitingForNextOrRootClose)
                    {
                        if (char.IsWhiteSpace(json, i)) continue;

                        if (json[i] == FieldSeparatorChar)
                        {
                            if (resultObject != null)
                            {
                                state = ReadState.WaitingForField;
                                currentFieldName = null;
                                continue;
                            }
                            else
                            {
                                state = ReadState.WaitingForValue;
                                continue;
                            }
                        }

                        if ((resultObject != null && json[i] == CloseObjectChar) || (resultArray != null && json[i] == CloseArrayChar))
                        {
                            EndIndex = i;
                            Result = (resultObject == null) ? resultArray as object : resultObject;
                            return;
                        }

                        throw new FormatException($"Parser error (char {i}, state {state}): Expected '{FieldSeparatorChar}' '{CloseObjectChar}' or '{CloseArrayChar}' but got '{json[i]}'.");

                    }

                    #endregion

                }



            }

            static private string Unescape(string str)
            {
                // check if we need to unescape at all
                if (str.IndexOf(StringEscapeChar) < 0)
                    return str;

                // TODO: Unescape string here
                return str;
            }

            static public object Deserialize(string json)
            {
                var deserializer = new Deserializer(json, 0);
                return deserializer.Result;
            }

        }

    }
}