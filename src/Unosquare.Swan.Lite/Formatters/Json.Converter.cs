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
    public static partial class Json
    {
        private class Converter
        {
            private static readonly Dictionary<MemberInfo, string> MemberInfoNameCache =
                new Dictionary<MemberInfo, string>();
            private static readonly Dictionary<Type, Type> ListAddMethodCache = new Dictionary<Type, Type>();

            private readonly object _target;
            private readonly Type _targetType;
            private readonly bool _includeNonPublic;

            private Converter(
                object source,
                Type targetType,
                ref object targetInstance,
                bool includeNonPublic)
            {
                _targetType = targetType;

                if (source == null)
                {
                    _target = _targetType.GetDefault();
                    return;
                }

                _includeNonPublic = includeNonPublic;
                var sourceType = source.GetType();

                if (targetInstance != null) _targetType = targetInstance.GetType();
                if (_targetType == null || _targetType == typeof(object)) _targetType = sourceType;
                if (sourceType == _targetType)
                {
                    _target = source;
                    return;
                }

                if (targetInstance == null)
                {
                    // Try to create a default instance
                    try
                    {
                        source.CreateTarget(_targetType, _includeNonPublic, ref _target);
                    }
                    catch
                    {
                        _target = _targetType.GetDefault();
                        return;
                    }
                }
                else
                {
                    _target = targetInstance;
                }

                switch (source)
                {
                    // Case 0: Special Cases Handling (Source and Target are of specific convertible types)
                    // Case 0.1: Source is string, Target is byte[]
                    case string sourceString when _targetType == typeof(byte[]):
                        _target = GetByteArray(sourceString);
                        break;

                    // Case 1.1: Source is Dictionary, Target is IDictionary
                    case Dictionary<string, object> sourceProperties when _target is IDictionary targetDictionary:
                        PopulateDictionary(sourceProperties, targetDictionary);
                        break;

                    // Case 1.2: Source is Dictionary, Target is not IDictionary (i.e. it is a complex type)
                    case Dictionary<string, object> sourceProperties:
                        PopulateObject(sourceProperties);
                        break;

                    // Case 2.1: Source is List, Target is Array
                    case List<object> sourceList when _target is Array targetArray:
                        PopulateArray(sourceList, targetArray);
                        break;

                    // Case 2.2: Source is List,  Target is IList
                    case List<object> sourceList when _target is IList targetList:

                        PopulateIList(sourceList, targetList);
                        break;

                    // Case 3: Source is a simple type; Attempt conversion
                    default:
                        var sourceStringValue = source.ToStringInvariant();

                        if (Definitions.BasicTypesInfo.ContainsKey(_targetType))
                        {
                            // Handle basic types
                            if (!_targetType.TryParseBasicType(sourceStringValue, out _target))
                                _target = _targetType.GetDefault();
                        }
                        else
                        {
                            // Handle Enumerations
                            GetEnumValue(sourceStringValue, ref _target);
                        }

                        break;
                }
            }

            /// <summary>
            /// Converts a json deserialized object (simple type, dictionary or list) to a new instance of the specified target type.
            /// </summary>
            /// <param name="source">The source.</param>
            /// <param name="targetType">Type of the target.</param>
            /// <param name="includeNonPublic">if set to <c>true</c> [include non public].</param>
            /// <returns>The target object.</returns>
            internal static object FromJsonResult(object source,
                Type targetType,
                bool includeNonPublic)
            {
                object nullRef = null;
                return new Converter(source, targetType, ref nullRef, includeNonPublic)._target;
            }

            private static object FromJsonResult(object source,
                Type targetType,
                ref object targetInstance,
                bool includeNonPublic)
            {
                return new Converter(source, targetType, ref targetInstance, includeNonPublic)._target;
            }

            private static Type GetAddMethodParameterType(Type targetType)
            {
                if (!ListAddMethodCache.ContainsKey(targetType))
                {
                    ListAddMethodCache[targetType] = targetType
                        .GetMethods()
                        .FirstOrDefault(m => m.Name.Equals(AddMethodName) && m.IsPublic && m.GetParameters().Length == 1)?
                        .GetParameters()[0]
                        .ParameterType;
                }

                return ListAddMethodCache[targetType];
            }

            private static byte[] GetByteArray(string sourceString)
            {
                try
                {
                    return Convert.FromBase64String(sourceString);
                } // Try conversion from Base 64
                catch
                {
                    return Encoding.UTF8.GetBytes(sourceString);
                } // Get the string bytes in UTF8
            }

            private static object GetSourcePropertyValue(Dictionary<string, object> sourceProperties, MemberInfo targetProperty)
            {
                if (!MemberInfoNameCache.ContainsKey(targetProperty))
                {
                    MemberInfoNameCache[targetProperty] =
                        targetProperty.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ??
                        targetProperty.Name;
                }

                var targetPropertyName = MemberInfoNameCache[targetProperty];

                return sourceProperties.ContainsKey(targetPropertyName)
                    ? sourceProperties[targetPropertyName]
                    : null;
            }

            private void PopulateIList(List<object> objects, IList list)
            {
                var parameterType = GetAddMethodParameterType(_targetType);
                if (parameterType == null) return;

                foreach (var item in objects)
                {
                    try
                    {
                        list.Add(FromJsonResult(
                            item,
                            parameterType,
                            _includeNonPublic));
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            private void PopulateArray(List<object> objects, Array array)
            {
                var elementType = _targetType.GetElementType();

                for (var i = 0; i < objects.Count; i++)
                {
                    try
                    {
                        var targetItem = FromJsonResult(
                            objects[i],
                            elementType,
                            _includeNonPublic);
                        array.SetValue(targetItem, i);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            private void GetEnumValue(string sourceStringValue, ref object target)
            {
                var enumType = Nullable.GetUnderlyingType(_targetType);
                if (enumType == null && _targetType.GetTypeInfo().IsEnum) enumType = _targetType;
                if (enumType == null) return;

                try
                {
                    target = Enum.Parse(enumType, sourceStringValue);
                }
                catch
                {
                    // ignored
                }
            }

            private void PopulateDictionary(Dictionary<string, object> sourceProperties, IDictionary targetDictionary)
            {
                // find the add method of the target dictionary
                var addMethod = _targetType.GetMethods()
                    .FirstOrDefault(
                        m => m.Name.Equals(AddMethodName) && m.IsPublic && m.GetParameters().Length == 2);

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
                            targetEntryType,
                            _includeNonPublic);
                        targetDictionary.Add(sourceProperty.Key, targetEntryValue);
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            private void PopulateObject(Dictionary<string, object> sourceProperties)
            {
                void SetPropertyValue(MemberInfo targetProperty)
                {
                    var sourcePropertyValue = GetSourcePropertyValue(sourceProperties, targetProperty);
                    if (sourcePropertyValue == null) return;

                    SetValue(sourcePropertyValue, targetProperty);
                }

                if (_targetType.IsValueType())
                {
                    foreach (var targetProperty in FieldTypeCache.RetrieveAllFields(_targetType))
                    {
                        SetPropertyValue(targetProperty);
                    }
                }

                foreach (var targetProperty in PropertyTypeCache.RetrieveFilteredProperties(_targetType, false, p => p.CanWrite))
                {
                    SetPropertyValue(targetProperty);
                }
            }

            private void SetValue(
                object sourcePropertyValue,
                MemberInfo targetProperty)
            {
                var currentPropertyValue = GetCurrentPropertyValue(targetProperty);

                switch (targetProperty)
                {
                    case FieldInfo field:
                        {
                            var targetPropertyValue = FromJsonResult(
                                sourcePropertyValue,
                                field.FieldType,
                                ref currentPropertyValue,
                                _includeNonPublic);

                            try
                            {
                                field.SetValue(_target, targetPropertyValue);
                            }
                            catch
                            {
                                // ignored
                            }

                            break;
                        }

                    case PropertyInfo property:
                        {
                            // Try to write properties to the current property value as a reference to the current property value
                            var targetPropertyValue = FromJsonResult(
                                sourcePropertyValue,
                                property.PropertyType,
                                ref currentPropertyValue,
                                _includeNonPublic);

                            try
                            {
                                property.GetSetMethod(_includeNonPublic).Invoke(_target, new[] { targetPropertyValue });
                            }
                            catch
                            {
                                // ignored
                            }

                            break;
                        }
                }
            }

            private object GetCurrentPropertyValue(MemberInfo targetProperty)
            {
                try
                {
                    if (targetProperty is PropertyInfo property && !property.PropertyType.IsArray)
                        return property.GetGetMethod(_includeNonPublic).Invoke(_target, null);
                }
                catch
                {
                    // ignored
                }

                return null;
            }
        }
    }
}