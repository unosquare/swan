using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    public static partial class Json
    {
        private class Converter
        {
            private static readonly ConcurrentDictionary<MemberInfo, string> MemberInfoNameCache =
                new ConcurrentDictionary<MemberInfo, string>();

            private static readonly ConcurrentDictionary<Type, Type?> ListAddMethodCache = new ConcurrentDictionary<Type, Type?>();

            private readonly object? _target;
            private readonly Type _targetType;
            private readonly bool _includeNonPublic;
            private readonly JsonSerializerCase _jsonSerializerCase;

            private Converter(
                object? source,
                Type targetType,
                ref object? targetInstance,
                bool includeNonPublic, 
                JsonSerializerCase jsonSerializerCase)
            {
                _targetType = targetInstance != null ? targetInstance.GetType() : targetType;
                _includeNonPublic = includeNonPublic;
                _jsonSerializerCase = jsonSerializerCase;

                if (source == null)
                {
                    return;
                }

                var sourceType = source.GetType();

                if (_targetType == null || _targetType == typeof(object)) _targetType = sourceType;
                if (sourceType == _targetType)
                {
                    _target = source;
                    return;
                }

                if (!TrySetInstance(targetInstance, source, ref _target))
                    return;

                ResolveObject(source, ref _target);
            }

            internal static object? FromJsonResult(
                object? source,
                JsonSerializerCase jsonSerializerCase,
                Type? targetType = null,
                bool includeNonPublic = false)
            {
                object? nullRef = null;
                return new Converter(source, targetType ?? typeof(object), ref nullRef, includeNonPublic, jsonSerializerCase).GetResult();
            }

            private static object? FromJsonResult(object source,
                Type targetType,
                ref object? targetInstance,
                bool includeNonPublic)
            {
                return new Converter(source, targetType, ref targetInstance, includeNonPublic, JsonSerializerCase.None).GetResult();
            }

            private static Type? GetAddMethodParameterType(Type targetType)
                => ListAddMethodCache.GetOrAdd(targetType,
                    x => x.GetMethods()
                        .FirstOrDefault(
                            m => m.Name == AddMethodName && m.IsPublic && m.GetParameters().Length == 1)?
                        .GetParameters()[0]
                        .ParameterType);

            private static void GetByteArray(string sourceString, ref object? target)
            {
                try
                {
                    target = Convert.FromBase64String(sourceString);
                } // Try conversion from Base 64
                catch (FormatException)
                {
                    target = Encoding.UTF8.GetBytes(sourceString);
                } // Get the string bytes in UTF8
            }

            private object? GetSourcePropertyValue(
                IDictionary<string, object> sourceProperties,
                MemberInfo targetProperty)
            {
                var targetPropertyName = MemberInfoNameCache.GetOrAdd(
                    targetProperty,
                    x => AttributeCache.DefaultCache.Value.RetrieveOne<JsonPropertyAttribute>(x)?.PropertyName ?? x.Name.GetNameWithCase(_jsonSerializerCase));

                return sourceProperties!.GetValueOrDefault(targetPropertyName);
            }

            private bool TrySetInstance(object? targetInstance, object source, ref object? target)
            {
                if (targetInstance == null)
                {
                    // Try to create a default instance
                    try
                    {
                        source.CreateTarget(_targetType, _includeNonPublic, ref target);
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        return false;
                    }
                }
                else
                {
                    target = targetInstance;
                }

                return true;
            }

            private object? GetResult() => _target ?? _targetType.GetDefault();

            private void ResolveObject(object source, ref object? target)
            {
                switch (source)
                {
                    // Case 0: Special Cases Handling (Source and Target are of specific convertible types)
                    // Case 0.1: Source is string, Target is byte[]
                    case string sourceString when _targetType == typeof(byte[]):
                        GetByteArray(sourceString, ref target);
                        break;

                    // Case 1.1: Source is Dictionary, Target is IDictionary
                    case Dictionary<string, object> sourceProperties when target is IDictionary targetDictionary:
                        PopulateDictionary(sourceProperties, targetDictionary);
                        break;

                    // Case 1.2: Source is Dictionary, Target is not IDictionary (i.e. it is a complex type)
                    case Dictionary<string, object> sourceProperties:
                        PopulateObject(sourceProperties);
                        break;

                    // Case 2.1: Source is List, Target is Array
                    case List<object> sourceList when target is Array targetArray:
                        PopulateArray(sourceList, targetArray);
                        break;

                    // Case 2.2: Source is List,  Target is IList
                    case List<object> sourceList when target is IList targetList:
                        PopulateIList(sourceList, targetList);
                        break;

                    // Case 3: Source is a simple type; Attempt conversion
                    default:
                        var sourceStringValue = source.ToStringInvariant();

                        // Handle basic types or enumerations if not
                        if (!_targetType.TryParseBasicType(sourceStringValue, out target))
                            GetEnumValue(sourceStringValue, ref target);

                        break;
                }
            }

            private void PopulateIList(IEnumerable<object> objects, IList list)
            {
                var parameterType = GetAddMethodParameterType(_targetType);
                if (parameterType == null) return;

                foreach (var item in objects)
                {
                    try
                    {
                        list.Add(FromJsonResult(
                            item,
                            _jsonSerializerCase,
                            parameterType,
                            _includeNonPublic));
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        // ignored
                    }
                }
            }

            private void PopulateArray(IList<object> objects, Array array)
            {
                var elementType = _targetType.GetElementType();

                for (var i = 0; i < objects.Count; i++)
                {
                    try
                    {
                        var targetItem = FromJsonResult(
                            objects[i],
                            _jsonSerializerCase,
                            elementType,
                            _includeNonPublic);
                        array.SetValue(targetItem, i);
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        // ignored
                    }
                }
            }

            private void GetEnumValue(string sourceStringValue, ref object? target)
            {
                var enumType = Nullable.GetUnderlyingType(_targetType);
                if (enumType == null && _targetType.IsEnum) enumType = _targetType;
                if (enumType == null) return;

                try
                {
                    target = Enum.Parse(enumType, sourceStringValue);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    // ignored
                }
            }

            private void PopulateDictionary(IDictionary<string, object> sourceProperties, IDictionary targetDictionary)
            {
                // find the add method of the target dictionary
                var addMethod = _targetType.GetMethods()
                    .FirstOrDefault(
                        m => m.Name == AddMethodName && m.IsPublic && m.GetParameters().Length == 2);

                // skip if we don't have a compatible add method
                if (addMethod == null) return;
                var addMethodParameters = addMethod.GetParameters();
                if (addMethodParameters[0].ParameterType != typeof(string)) return;

                // Retrieve the target entry type
                var targetEntryType = addMethodParameters[1].ParameterType;

                // Add the items to the target dictionary
                foreach (var sourceProperty in sourceProperties)
                {
                    try
                    {
                        var targetEntryValue = FromJsonResult(
                            sourceProperty.Value,
                            _jsonSerializerCase,
                            targetEntryType,
                            _includeNonPublic);
                        targetDictionary.Add(sourceProperty.Key, targetEntryValue);
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        // ignored
                    }
                }
            }

            private void PopulateObject(IDictionary<string, object> sourceProperties)
            {
                if (sourceProperties == null)
                    return;

                if (_targetType.IsValueType)
                    PopulateFields(sourceProperties);

                PopulateProperties(sourceProperties);
            }

            private void PopulateProperties(IDictionary<string, object> sourceProperties)
            {
                var properties = PropertyTypeCache.DefaultCache.Value.RetrieveFilteredProperties(_targetType, false, p => p.CanWrite);

                foreach (var property in properties)
                {
                    var sourcePropertyValue = GetSourcePropertyValue(sourceProperties, property);
                    if (sourcePropertyValue == null) continue;

                    try
                    {
                        var currentPropertyValue = !property.PropertyType.IsArray
                            ? _target.ReadProperty(property.Name)
                            : null;

                        var targetPropertyValue = FromJsonResult(
                            sourcePropertyValue,
                            property.PropertyType,
                            ref currentPropertyValue,
                            _includeNonPublic);

                        _target.WriteProperty(property.Name, targetPropertyValue);
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        // ignored
                    }
                }
            }

            private void PopulateFields(IDictionary<string, object> sourceProperties)
            {
                foreach (var field in FieldTypeCache.DefaultCache.Value.RetrieveAllFields(_targetType))
                {
                    var sourcePropertyValue = GetSourcePropertyValue(sourceProperties, field);
                    if (sourcePropertyValue == null) continue;

                    var targetPropertyValue = FromJsonResult(
                        sourcePropertyValue,
                        _jsonSerializerCase,
                        field.FieldType,
                        _includeNonPublic);

                    try
                    {
                        field.SetValue(_target, targetPropertyValue);
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        // ignored
                    }
                }
            }
        }
    }
}
