namespace Unosquare.Swan.Formatters
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    partial class Json
    {
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
            /// Initializes a new instance of the <see cref="Serializer" /> class.
            /// </summary>
            /// <param name="obj">The object.</param>
            /// <param name="depth">The depth.</param>
            /// <param name="format">if set to <c>true</c> [format].</param>
            /// <param name="includeProperties">The include properties.</param>
            /// <param name="excludeProperties">The exclude properties.</param>
            /// <param name="includeNonPublic">if set to <c>true</c> [include non public].</param>
            private Serializer(object obj, int depth, bool format, string[] includeProperties, string[] excludeProperties, bool includeNonPublic)
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
                    Result = $"{StringQuotedChar}{Escape(obj as string)}{StringQuotedChar}";
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
                            var serializedValue = Serialize(entry.Value, depth + 1, Format, includeProperties, excludeProperties, includeNonPublic);
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
                            Result = Serialize((target as byte[]).ToBase64(), depth, Format, includeProperties, excludeProperties, includeNonPublic);
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
                            Result = EmptyArrayLiteral;
                            return;
                        }

                        // Iterate through the elements and output recursively
                        var writeCount = 0;
                        foreach (var entry in items)
                        {
                            var serializedValue = Serialize(entry, depth + 1, Format, includeProperties, excludeProperties, includeNonPublic);

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

                    // Create the dictionary and extract the properties
                    var objectDictionary = new Dictionary<string, object>();
                    var properties = RetrieveProperties(TargetType).Where(p => p.CanRead).ToArray();

                    // If we set the included properties, then we remove everything that is not listed
                    if (IncludeProperties.Count > 0)
                        properties = properties.Where(p => IncludeProperties.Contains(p.Name)).ToArray();

                    foreach (var property in properties)
                    {
                        // Skip over the excluded properties
                        if (ExcludeProperties.Count > 0 && ExcludeProperties.Contains(property.Name))
                            continue;

                        // Build the dictionary using property names and values
                        // Note: used to be: property.GetValue(target); but we would be reading private properties
                        try { objectDictionary[property.Name] = property.GetGetMethod(includeNonPublic)?.Invoke(target, null); }
                        catch { /* ignored */ }
                    }

                    // At this point we either have a dictionary with or without properties
                    // If we have at least one property then we send it through the serialization method
                    // If we don't have any properties we simply call its tostring method and serialize as string
                    if (objectDictionary.Count > 0)
                        Result = Serialize(objectDictionary, depth, Format, includeProperties, excludeProperties, includeNonPublic);
                    else
                        Result = Serialize(target.ToString(), 0, Format, includeProperties, excludeProperties, includeNonPublic);
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
            /// <param name="includeNonPublic">if set to true, then non public properties are also retrieved</param>
            /// <returns></returns>
            static public string Serialize(object obj, int depth, bool format, string[] includeProperties, string[] excludeProperties, bool includeNonPublic)
            {
                var serializer = new Serializer(obj, depth, format, includeProperties, excludeProperties, includeNonPublic);
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

    }
}
