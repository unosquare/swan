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
            #region State Variables

            private readonly object _result;
            private readonly Dictionary<string, object> _resultObject;
            private readonly List<object> _resultArray;

            private readonly ReadState _state = ReadState.WaitingForRootOpen;
            private readonly string _currentFieldName;
            private readonly int _endIndex;

            private int _index;

            #endregion

            private Deserializer(string json, int startIndex)
            {
                for (_index = startIndex; _index < json.Length; _index++)
                {
                    #region Wait for { or [

                    if (_state == ReadState.WaitingForRootOpen)
                    {
                        if (char.IsWhiteSpace(json, _index)) continue;

                        if (json[_index] == OpenObjectChar)
                        {
                            _resultObject = new Dictionary<string, object>();
                            _state = ReadState.WaitingForField;
                            continue;
                        }

                        if (json[_index] == OpenArrayChar)
                        {
                            _resultArray = new List<object>();
                            _state = ReadState.WaitingForValue;
                            continue;
                        }

                        throw CreateParserException(json, $"'{OpenObjectChar}' or '{OpenArrayChar}'");
                    }

                    #endregion

                    #region Wait for opening field " (only applies for object results)

                    if (_state == ReadState.WaitingForField)
                    {
                        if (char.IsWhiteSpace(json, _index)) continue;

                        // Handle empty arrays and empty objects
                        if ((_resultObject != null && json[_index] == CloseObjectChar)
                            || (_resultArray != null && json[_index] == CloseArrayChar))
                        {
                            _endIndex = _index;
                            _result = _resultObject ?? _resultArray as object;
                            return;
                        }

                        if (json[_index] != StringQuotedChar)
                            throw CreateParserException(json, $"'{StringQuotedChar}'");

                        var charCount = GetFieldNameCount(json);

                        _currentFieldName = Unescape(json.SliceLength(_index + 1, charCount));
                        _index += charCount + 1;
                        _state = ReadState.WaitingForColon;
                        continue;
                    }

                    #endregion

                    #region Wait for field-value separator : (only applies for object results

                    if (_state == ReadState.WaitingForColon)
                    {
                        if (char.IsWhiteSpace(json, _index)) continue;

                        if (json[_index] != ValueSeparatorChar)
                            throw CreateParserException(json, $"'{ValueSeparatorChar}'");

                        _state = ReadState.WaitingForValue;
                        continue;
                    }

                    #endregion

                    #region Wait for and Parse the value

                    if (_state == ReadState.WaitingForValue)
                    {
                        if (char.IsWhiteSpace(json, _index)) continue;

                        // Handle empty arrays and empty objects
                        if ((_resultObject != null && json[_index] == CloseObjectChar)
                            || (_resultArray != null && json[_index] == CloseArrayChar))
                        {
                            _endIndex = _index;
                            _result = _resultObject ?? _resultArray as object;
                            return;
                        }

                        // determine the value based on what it starts with
                        switch (json[_index])
                        {
                            case StringQuotedChar: // expect a string
                            {
                                // Update state variables
                                ExtractStringQuoted(json);
                                _currentFieldName = null;
                                _state = ReadState.WaitingForNextOrRootClose;
                                continue;
                            }

                            case OpenObjectChar: // expect object
                            case OpenArrayChar: // expect array
                            {
                                // Update state variables
                                 ExtractObject(json);
                                _currentFieldName = null;
                                _state = ReadState.WaitingForNextOrRootClose;
                                continue;
                            }

                            case 't': // expect true
                            {
                                // Update state variables
                                ExtractConstant(json, TrueLiteral, true);
                                _currentFieldName = null;
                                _state = ReadState.WaitingForNextOrRootClose;
                                continue;
                            }

                            case 'f': // expect false
                            {
                                // Update state variables
                                ExtractConstant(json, FalseLiteral, false);
                                _currentFieldName = null;
                                _state = ReadState.WaitingForNextOrRootClose;
                                continue;
                            }

                            case 'n': // expect null
                            {
                                // Update state variables
                                ExtractConstant(json, NullLiteral, null);
                                _currentFieldName = null;
                                _state = ReadState.WaitingForNextOrRootClose;
                                continue;
                            }

                            default: // expect number
                            {
                                // Update state variables
                                ExtractNumber(json);
                                _currentFieldName = null;
                                _state = ReadState.WaitingForNextOrRootClose;
                                continue;
                            }
                        }
                    }

                    #endregion

                    #region Wait for closing ], } or an additional field or value ,

                    if (_state != ReadState.WaitingForNextOrRootClose) continue;

                    if (char.IsWhiteSpace(json, _index)) continue;

                    if (json[_index] == FieldSeparatorChar)
                    {
                        if (_resultObject != null)
                        {
                            _state = ReadState.WaitingForField;
                            _currentFieldName = null;
                            continue;
                        }

                        _state = ReadState.WaitingForValue;
                        continue;
                    }

                    if ((_resultObject != null && json[_index] == CloseObjectChar) ||
                        (_resultArray != null && json[_index] == CloseArrayChar))
                    {
                        _endIndex = _index;
                        _result = _resultObject ?? _resultArray as object;
                        return;
                    }

                    throw CreateParserException(json, $"'{FieldSeparatorChar}' '{CloseObjectChar}' or '{CloseArrayChar}'");

                    #endregion
                }
            }

            /// <summary>
            /// Deserializes specified JSON string
            /// </summary>
            /// <param name="json">The json.</param>
            /// <returns>Type of the current deserializes specified JSON string</returns>
            public static object DeserializeInternal(string json) => new Deserializer(json, 0)._result;

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

            private int GetFieldNameCount(string json)
            {
                var charCount = 0;
                for (var j = _index + 1; j < json.Length; j++)
                {
                    if (json[j] == StringQuotedChar && json[j - 1] != StringEscapeChar)
                        break;

                    charCount++;
                }

                return charCount;
            }

            private void ExtractObject(string json)
            {
                // Extract and set the value
                var deserializer = new Deserializer(json, _index);

                if (_currentFieldName != null)
                    _resultObject[_currentFieldName] = deserializer._result;
                else
                    _resultArray.Add(deserializer._result);

                _index = deserializer._endIndex;
            }

            private void ExtractNumber(string json)
            {
                var charCount = 0;
                for (var j = _index; j < json.Length; j++)
                {
                    if (char.IsWhiteSpace(json[j]) || json[j] == FieldSeparatorChar
                        || (_resultObject != null && json[j] == CloseObjectChar)
                        || (_resultArray != null && json[j] == CloseArrayChar))
                        break;

                    charCount++;
                }

                // Extract and set the value
                var stringValue = json.SliceLength(_index, charCount);

                if (decimal.TryParse(stringValue, out var value) == false)
                    throw CreateParserException(json, "[number]");

                if (_currentFieldName != null)
                    _resultObject[_currentFieldName] = value;
                else
                    _resultArray.Add(value);

                _index += charCount - 1;
            }

            private void ExtractConstant(string json, string boolValue, bool? value)
            {
                if (!json.SliceLength(_index, boolValue.Length).Equals(boolValue))
                    throw CreateParserException(json, $"'{ValueSeparatorChar}'");

                // Extract and set the value
                if (_currentFieldName != null)
                    _resultObject[_currentFieldName] = value;
                else
                    _resultArray.Add(value);

                _index += boolValue.Length - 1;
            }

            private void ExtractStringQuoted(string json)
            {
                var charCount = 0;
                var escapeCharFound = false;
                for (var j = _index + 1; j < json.Length; j++)
                {
                    if (json[j] == StringQuotedChar && !escapeCharFound)
                        break;

                    if (json[j] == StringEscapeChar)
                        escapeCharFound = !escapeCharFound;
                    else
                        escapeCharFound = false;

                    charCount++;
                }

                // Extract and set the value
                var value = Unescape(json.SliceLength(_index + 1, charCount));
                if (_currentFieldName != null)
                    _resultObject[_currentFieldName] = value;
                else
                    _resultArray.Add(value);

                _index += charCount + 1;
            }
            
            private FormatException CreateParserException(
                string json, 
                string expected)
            {
                var textPosition = json.TextPositionAt(_index);
                return new FormatException(
                    $"Parser error (Line {textPosition.Item1}, Col {textPosition.Item2}, State {_state}): Expected {expected} but got '{json[_index]}'.");
            }

            /// <summary>
            /// Defines the different JSON read states
            /// </summary>
            private enum ReadState
            {
                WaitingForRootOpen,
                WaitingForField,
                WaitingForColon,
                WaitingForValue,
                WaitingForNextOrRootClose,
            }
        }
    }
}