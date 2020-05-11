using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Swan.Reflection;

namespace Swan.Parsers
{
    /// <summary>
    /// Provides methods to parse command line arguments.
    /// 
    /// Based on CommandLine (Copyright 2005-2015 Giacomo Stelluti Scala and Contributors).
    /// </summary>
    public partial class ArgumentParser
    {
        private sealed class Validator
        {
            private const char OptionSwitchChar = '-';
            private readonly object _instance;
            private readonly IEnumerable<string> _args;
            private readonly List<PropertyInfo> _updatedList = new List<PropertyInfo>();
            private readonly ArgumentParserSettings _settings;

            private readonly PropertyInfo[] _properties;

            public Validator(
                PropertyInfo[] properties,
                IEnumerable<string> args,
                object instance,
                ArgumentParserSettings settings,
                bool hasVerb = false)
            {
                _args = args;
                _instance = instance;
                _settings = settings;
                _properties = properties;

                PopulateInstance();
                if (!hasVerb) SetDefaultArgument();
                SetDefaultValues();
                GetRequiredList();
            }

            public List<string> UnknownList { get; } = new List<string>();
            public List<string> RequiredList { get; } = new List<string>();

            public bool IsValid() => (_settings.IgnoreUnknownArguments || !UnknownList.Any()) && !RequiredList.Any();

            public IEnumerable<ArgumentOptionAttribute> GetPropertiesOptions()
                => _properties.Select(p => AttributeCache.DefaultCache.Value.RetrieveOne<ArgumentOptionAttribute>(p))
                    .Where(x => x != null);

            private void GetRequiredList()
            {
                foreach (var targetProperty in _properties)
                {
                    var optionAttr = AttributeCache.DefaultCache.Value.RetrieveOne<ArgumentOptionAttribute>(targetProperty);

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
                    var optionAttr = AttributeCache.DefaultCache.Value.RetrieveOne<ArgumentOptionAttribute>(targetProperty);

                    var defaultValue = optionAttr?.DefaultValue;

                    if (defaultValue == null)
                        continue;

                    if (SetPropertyValue(targetProperty, defaultValue.ToString(), _instance, optionAttr))
                        _updatedList.Add(targetProperty);
                }
            }

            private void SetDefaultArgument()
            {
                foreach (var targetProperty in _properties.Except(_updatedList))
                {
                    var optionAttr = AttributeCache.DefaultCache.Value.RetrieveOne<ArgumentOptionAttribute>(targetProperty);

                    if (!optionAttr.IsDefault)
                        continue;

                    var defaultArgValue = _args.FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(defaultArgValue) || defaultArgValue[0] == OptionSwitchChar)
                        continue;

                    if (SetPropertyValue(targetProperty, defaultArgValue, _instance, optionAttr))
                        _updatedList.Add(targetProperty);
                }
            }

            private void PopulateInstance()
            {
                var propertyName = string.Empty;

                foreach (var arg in _args)
                {
                    var ignoreSetValue = string.IsNullOrWhiteSpace(propertyName);

                    if (ignoreSetValue)
                    {
                        if (string.IsNullOrWhiteSpace(arg) || arg[0] != OptionSwitchChar) continue;

                        propertyName = arg.Substring(1);

                        if (!string.IsNullOrWhiteSpace(propertyName) && propertyName[0] == OptionSwitchChar)
                            propertyName = propertyName.Substring(1);
                    }

                    var targetProperty = TryGetProperty(propertyName);

                    if (targetProperty == null)
                    {
                        // Skip if the property is not found
                        UnknownList.Add(propertyName);
                        continue;
                    }

                    if (!ignoreSetValue && SetPropertyValue(targetProperty, arg, _instance))
                    {
                        _updatedList.Add(targetProperty);
                        propertyName = string.Empty;
                    }
                    else if (targetProperty.PropertyType == typeof(bool))
                    {
                        // If the arg is a boolean property set it to true.
                        targetProperty.SetValue(_instance, true);

                        _updatedList.Add(targetProperty);
                        propertyName = string.Empty;
                    }
                }

                if (!string.IsNullOrEmpty(propertyName))
                {
                    UnknownList.Add(propertyName);
                }
            }

            private bool SetPropertyValue(
                PropertyInfo targetProperty,
                string propertyValueString,
                object result,
                ArgumentOptionAttribute? optionAttr = null)
            {
                if (!targetProperty.PropertyType.IsEnum)
                {
                    return targetProperty.PropertyType.IsArray
                        ? targetProperty.TrySetArray(propertyValueString.Split(optionAttr?.Separator ?? ','), result)
                        : targetProperty.TrySetBasicType(propertyValueString, result);
                }

                var parsedValue = Enum.Parse(
                    targetProperty.PropertyType,
                    propertyValueString,
                    _settings.CaseInsensitiveEnumValues);

                targetProperty.SetValue(result, Enum.ToObject(targetProperty.PropertyType, parsedValue));

                return true;
            }

            private PropertyInfo TryGetProperty(string propertyName)
                => _properties.FirstOrDefault(p =>
                    string.Equals(AttributeCache.DefaultCache.Value.RetrieveOne<ArgumentOptionAttribute>(p)?.LongName, propertyName, _settings.NameComparer) ||
                    string.Equals(AttributeCache.DefaultCache.Value.RetrieveOne<ArgumentOptionAttribute>(p)?.ShortName, propertyName, _settings.NameComparer));
        }
    }
}
