﻿namespace Unosquare.Swan.Formatters
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// A very simple, light-weight JSON library written by Mario
    /// to teach Geo how things are done
    /// 
    /// This is an useful helper for small tasks but it doesn't represent a full-featured
    /// serializer such as the beloved Json.NET
    /// </summary>
    public partial class Json
    {
        /// <summary>
        /// A simple JSON Deserializer
        /// </summary>
        private class Deserializer
        {
            /// <summary>
            /// Defines the different JSOn read states
            /// </summary>
            private enum ReadState
            {
                WaitingForRootOpen,
                WaitingForField,
                WaitingForColon,
                WaitingForValue,
                WaitingForNextOrRootClose,
            }

            #region State Variables

            private readonly object _result;
            private readonly Dictionary<string, object> _resultObject;
            private readonly List<object> _resultArray;

            private readonly ReadState State = ReadState.WaitingForRootOpen;
            private readonly string CurrentFieldName;
            private readonly int EndIndex;

            #endregion

            private Deserializer(string json, int startIndex)
            {
                for (var i = startIndex; i < json.Length; i++)
                {
                    #region Wait for { or [
                    if (State == ReadState.WaitingForRootOpen)
                    {
                        if (char.IsWhiteSpace(json, i)) continue;

                        if (json[i] == OpenObjectChar)
                        {
                            _resultObject = new Dictionary<string, object>();
                            State = ReadState.WaitingForField;
                            continue;
                        }

                        if (json[i] == OpenArrayChar)
                        {
                            _resultArray = new List<object>();
                            State = ReadState.WaitingForValue;
                            continue;
                        }

                        throw CreateParserException(json, i, State, $"'{OpenObjectChar}' or '{OpenArrayChar}'");
                    }

                    #endregion

                    #region Wait for opening field " (only applies for object results)

                    if (State == ReadState.WaitingForField)
                    {
                        if (char.IsWhiteSpace(json, i)) continue;

                        // Handle empty arrays and empty objects
                        if ((_resultObject != null && json[i] == CloseObjectChar)
                            || (_resultArray != null && json[i] == CloseArrayChar))
                        {
                            EndIndex = i;
                            _result = _resultObject ?? _resultArray as object;
                            return;
                        }

                        if (json[i] != StringQuotedChar)
                            throw CreateParserException(json, i, State, $"'{StringQuotedChar}'");

                        var charCount = GetFieldNameCount(json, i);

                        CurrentFieldName = Unescape(json.SliceLength(i + 1, charCount));
                        i += charCount + 1;
                        State = ReadState.WaitingForColon;
                        continue;
                    }

                    #endregion

                    #region Wait for field-value separator : (only applies for object results

                    if (State == ReadState.WaitingForColon)
                    {
                        if (char.IsWhiteSpace(json, i)) continue;

                        if (json[i] != ValueSeparatorChar)
                            throw CreateParserException(json, i, State, $"'{ValueSeparatorChar}'");

                        State = ReadState.WaitingForValue;
                        continue;
                    }

                    #endregion

                    #region Wait for and Parse the value

                    if (State == ReadState.WaitingForValue)
                    {
                        if (char.IsWhiteSpace(json, i)) continue;

                        // Handle empty arrays and empty objects
                        if ((_resultObject != null && json[i] == CloseObjectChar)
                            || (_resultArray != null && json[i] == CloseArrayChar))
                        {
                            EndIndex = i;
                            _result = _resultObject ?? _resultArray as object;
                            return;
                        }

                        // determine the value based on what it starts with
                        switch (json[i])
                        {
                            case StringQuotedChar: // expect a string
                                {
                                    // Update state variables
                                    i = ExtractStringQuoted(json, i);
                                    CurrentFieldName = null;
                                    State = ReadState.WaitingForNextOrRootClose;
                                    continue;
                                }

                            case OpenObjectChar: // expect object
                            case OpenArrayChar: // expect array
                                {
                                    // Update state variables
                                    i = ExtractObject(json, i);
                                    CurrentFieldName = null;
                                    State = ReadState.WaitingForNextOrRootClose;
                                    continue;
                                }

                            case 't': // expect true
                                {
                                    // Update state variables
                                    i = ExtractTrueValue(json, i);
                                    CurrentFieldName = null;
                                    State = ReadState.WaitingForNextOrRootClose;
                                    continue;
                                }

                            case 'f': // expect false
                                {
                                    // Update state variables
                                    i = ExtractFalseValue(json, i);
                                    CurrentFieldName = null;
                                    State = ReadState.WaitingForNextOrRootClose;
                                    continue;
                                }

                            case 'n': // expect null
                                {
                                    // Update state variables
                                    i = ExtractNullValue(json, i);
                                    CurrentFieldName = null;
                                    State = ReadState.WaitingForNextOrRootClose;
                                    continue;
                                }

                            default: // expect number
                                {
                                    // Update state variables
                                    i = ExtractNumber(json, i);
                                    CurrentFieldName = null;
                                    State = ReadState.WaitingForNextOrRootClose;
                                    continue;
                                }
                        }
                    }

                    #endregion

                    #region Wait for closing ], } or an additional field or value ,

                    if (State != ReadState.WaitingForNextOrRootClose) continue;

                    if (char.IsWhiteSpace(json, i)) continue;

                    if (json[i] == FieldSeparatorChar)
                    {
                        if (_resultObject != null)
                        {
                            State = ReadState.WaitingForField;
                            CurrentFieldName = null;
                            continue;
                        }

                        State = ReadState.WaitingForValue;
                        continue;
                    }

                    if ((_resultObject != null && json[i] == CloseObjectChar) || (_resultArray != null && json[i] == CloseArrayChar))
                    {
                        EndIndex = i;
                        _result = _resultObject ?? _resultArray as object;
                        return;
                    }

                    throw CreateParserException(json, i, State, $"'{FieldSeparatorChar}' '{CloseObjectChar}' or '{CloseArrayChar}'");

                    #endregion
                }
            }

            private static int GetFieldNameCount(string json, int i)
            {
                var charCount = 0;
                for (var j = i + 1; j < json.Length; j++)
                {
                    if (json[j] == StringQuotedChar && json[j - 1] != StringEscapeChar)
                        break;

                    charCount++;
                }

                return charCount;
            }

            private int ExtractObject(string json, int i)
            {
                // Extract and set the value
                var deserializer = new Deserializer(json, i);

                if (CurrentFieldName != null)
                    _resultObject[CurrentFieldName] = deserializer._result;
                else
                    _resultArray.Add(deserializer._result);

                return deserializer.EndIndex;
            }

            private int ExtractNumber(string json, int i)
            {
                var charCount = 0;
                for (var j = i; j < json.Length; j++)
                {
                    if (char.IsWhiteSpace(json[j]) || json[j] == FieldSeparatorChar
                        || (_resultObject != null && json[j] == CloseObjectChar)
                        || (_resultArray != null && json[j] == CloseArrayChar))
                        break;

                    charCount++;
                }

                // Extract and set the value
                var stringValue = json.SliceLength(i, charCount);
                decimal value;

                if (decimal.TryParse(stringValue, out value) == false)
                    throw CreateParserException(json, i, State, "[number]");

                if (CurrentFieldName != null)
                    _resultObject[CurrentFieldName] = value;
                else
                    _resultArray.Add(value);

                i += charCount - 1;
                return i;
            }

            private int ExtractNullValue(string json, int i)
            {
                if (!json.SliceLength(i, NullLiteral.Length).Equals(NullLiteral))
                    throw CreateParserException(json, i, State, $"'{ValueSeparatorChar}'");

                // Extract and set the value
                if (CurrentFieldName != null)
                    _resultObject[CurrentFieldName] = null;
                else
                    _resultArray.Add(null);

                i += NullLiteral.Length - 1;
                return i;
            }

            private int ExtractFalseValue(string json, int i)
            {
                if (!json.SliceLength(i, FalseLiteral.Length).Equals(FalseLiteral))
                    throw CreateParserException(json, i, State, $"'{ValueSeparatorChar}'");

                // Extract and set the value
                if (CurrentFieldName != null)
                    _resultObject[CurrentFieldName] = false;
                else
                    _resultArray.Add(false);

                i += FalseLiteral.Length - 1;
                return i;
            }

            private int ExtractTrueValue(string json, int i)
            {
                if (!json.SliceLength(i, TrueLiteral.Length).Equals(TrueLiteral))
                    throw CreateParserException(json, i, State, $"'{ValueSeparatorChar}'");

                // Extract and set the value
                if (CurrentFieldName != null)
                    _resultObject[CurrentFieldName] = true;
                else
                    _resultArray.Add(true);

                i += TrueLiteral.Length - 1;
                return i;
            }

            private int ExtractStringQuoted(string json, int i)
            {
                var charCount = 0;
                for (var j = i + 1; j < json.Length; j++)
                {
                    if (json[j] == StringQuotedChar && json[j - 1] != StringEscapeChar)
                        break;

                    charCount++;
                }

                // Extract and set the value
                var value = Unescape(json.SliceLength(i + 1, charCount));
                if (CurrentFieldName != null)
                    _resultObject[CurrentFieldName] = value;
                else
                    _resultArray.Add(value);

                i += charCount + 1;
                return i;
            }

            private static FormatException CreateParserException(string json, int charIndex, ReadState state, string expected)
            {
                var textPosition = json.TextPositionAt(charIndex);
                return new FormatException($"Parser error (Line {textPosition.Item1}, Col {textPosition.Item2}, State {state}): Expected {expected} but got '{json[charIndex]}'.");
            }

            private static string Unescape(string str)
            {
                // check if we need to unescape at all
                if (str.IndexOf(StringEscapeChar) < 0)
                    return str;

                var builder = new StringBuilder(str.Length);
                for (var i = 0; i < str.Length; i++)
                {
                    if (str[i] != StringEscapeChar)
                    {
                        builder.Append(str[i]);
                        continue;
                    }
                        
                    if (i + 1 > str.Length - 1)
                        break;

                    // escape sequence begins here
                    switch (str[i + 1])
                    {
                        case 'u':
                            {
                                var startIndex = i + 2;
                                var endIndex = i + 5;
                                if (endIndex > str.Length - 1)
                                {
                                    builder.Append(str[i + 1]);
                                    i += 1;
                                    break;
                                }

                                var hexCode = str.Slice(startIndex, endIndex).ConvertHexadecimalToBytes();
                                builder.Append(Encoding.BigEndianUnicode.GetChars(hexCode));
                                i += 5;
                                break;
                            }

                        case 'b':
                            builder.Append('\b');
                            i += 1;
                            break;
                        case 't':
                            builder.Append('\t');
                            i += 1;
                            break;
                        case 'n':
                            builder.Append('\n');
                            i += 1;
                            break;
                        case 'f':
                            builder.Append('\f');
                            i += 1;
                            break;
                        case 'r':
                            builder.Append('\r');
                            i += 1;
                            break;
                        default:
                            builder.Append(str[i + 1]);
                            i += 1;
                            break;
                    }
                }

                return builder.ToString();
            }

            /// <summary>
            /// Deserializes specified JSON string
            /// </summary>
            /// <param name="json">The json.</param>
            /// <returns>Type of the current deserializes specified JSON string</returns>
            public static object DeserializeInternal(string json)
            {
                var deserializer = new Deserializer(json, 0);
                return deserializer._result;
            }
        }
    }
}
