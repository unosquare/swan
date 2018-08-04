﻿namespace Unosquare.Swan.Components
{
    using System.Linq;
    using System.Reflection;
    using Attributes;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides methods to parse command line arguments.
    /// Based on CommandLine (Copyright 2005-2015 Giacomo Stelluti Scala and Contributors.)
    /// </summary>
    public partial class ArgumentParser
    {
        internal class Validator
        {
            public readonly List<string> UnknownList = new List<string>();
            public readonly List<string> RequiredList = new List<string>();

            private readonly object _instance;
            private readonly IEnumerable<string> _args;
            private readonly List<PropertyInfo> _updatedList = new List<PropertyInfo>();
            private readonly ArgumentParserSettings _settings;

            private readonly PropertyInfo[] _properties;

            public Validator(
                PropertyInfo[] properties,
                IEnumerable<string> args,
                object instance,
                ArgumentParserSettings settings)
            {
                _args = args;
                _instance = instance;
                _settings = settings;
                _properties = properties;

                PopulateInstance();
                SetDefaultValues();
                GetRequiredList();
            }

            public bool IsValid() => (_settings.IgnoreUnknownArguments || !UnknownList.Any()) && !RequiredList.Any();

            public ArgumentOptionAttribute[] GetPropertiesOptions()
                => _properties.Select(p => Runtime.AttributeCache.RetrieveOne<ArgumentOptionAttribute>(p))
                    .Where(x => x != null)
                    .ToArray();

            private void GetRequiredList()
            {
                foreach (var targetProperty in _properties)
                {
                    var optionAttr = Runtime.AttributeCache.RetrieveOne<ArgumentOptionAttribute>(targetProperty);

                    if (optionAttr == null || optionAttr.Required == false)
                        continue;

                    if (targetProperty.GetValue(_instance) == null)
                    {
                        RequiredList.Add(optionAttr.LongName ?? optionAttr.ShortName);
                    }
                }
            }

            private void SetDefaultValues()
            {
                foreach (var targetProperty in _properties.Except(_updatedList))
                {
                    var defaultValue = Runtime.AttributeCache.RetrieveOne<ArgumentOptionAttribute>(targetProperty)?.DefaultValue;

                    if (defaultValue == null)
                        continue;

                    if (SetPropertyValue(targetProperty, defaultValue.ToString(), _instance))
                        _updatedList.Add(targetProperty);
                }
            }

            private void PopulateInstance()
            {
                var propertyName = string.Empty;

                foreach (var arg in _args)
                {
                    if (string.IsNullOrWhiteSpace(propertyName) == false)
                    {
                        var targetProperty = TryGetProperty(propertyName);

                        // Skip if the property is not found
                        if (targetProperty == null)
                        {
                            UnknownList.Add(propertyName);
                            propertyName = string.Empty;
                            continue;
                        }

                        if (SetPropertyValue(targetProperty, arg, _instance))
                            _updatedList.Add(targetProperty);

                        propertyName = string.Empty;
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(arg) || arg[0] != Dash) continue;

                        propertyName = arg.Substring(1);
                        if (propertyName[0] == Dash) propertyName = propertyName.Substring(1);

                        var targetProperty = TryGetProperty(propertyName);

                        // If the arg is a boolean property set it to true.
                        if (targetProperty == null || targetProperty.PropertyType != typeof(bool)) continue;

                        if (SetPropertyValue(targetProperty, true.ToString(), _instance))
                            _updatedList.Add(targetProperty);

                        propertyName = string.Empty;
                    }
                }

                if (string.IsNullOrEmpty(propertyName) == false)
                {
                    UnknownList.Add(propertyName);
                }
            }

            private bool SetPropertyValue(PropertyInfo targetProperty, string propertyValueString, object result)
            {
                var optionAttr = Runtime.AttributeCache.RetrieveOne<ArgumentOptionAttribute>(targetProperty);

                if (targetProperty.PropertyType.GetTypeInfo().IsEnum)
                {
                    var parsedValue = Enum.Parse(
                        targetProperty.PropertyType,
                        propertyValueString,
                        _settings.CaseInsensitiveEnumValues);
                    targetProperty.SetValue(result, Enum.ToObject(targetProperty.PropertyType, parsedValue));

                    return true;
                }

                if (targetProperty.PropertyType.IsCollection())
                {
                    var itemType = targetProperty.PropertyType.GetElementType();

                    if (itemType == null)
                    {
                        throw new InvalidOperationException(
                            $"The option collection {optionAttr.ShortName ?? optionAttr.LongName} should be an array");
                    }

                    var propertyArrayValue = propertyValueString.Split(optionAttr.Separator);
                    var arr = Array.CreateInstance(itemType, propertyArrayValue.Cast<object>().Count());

                    var i = 0;
                    foreach (var value in propertyArrayValue)
                    {
                        if (itemType.TryParseBasicType(value, out var itemvalue))
                            arr.SetValue(itemvalue, i++);
                    }

                    targetProperty.SetValue(result, arr);

                    return true;
                }

                if (!targetProperty.PropertyType.TryParseBasicType(propertyValueString, out var propertyValue))
                    return false;

                targetProperty.SetValue(result, propertyValue);
                return true;
            }

            private PropertyInfo TryGetProperty(string propertyName)
                => _properties.FirstOrDefault(p =>
                    string.Equals(Runtime.AttributeCache.RetrieveOne<ArgumentOptionAttribute>(p)?.LongName, propertyName, _settings.NameComparer) ||
                    string.Equals(Runtime.AttributeCache.RetrieveOne<ArgumentOptionAttribute>(p)?.ShortName, propertyName, _settings.NameComparer));
        }
    }
}