namespace Unosquare.Swan.Components
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
        private sealed class Validator
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

            public IEnumerable<ArgumentOptionAttribute> GetPropertiesOptions()
                => _properties.Select(p => Runtime.AttributeCache.RetrieveOne<ArgumentOptionAttribute>(p))
                    .Where(x => x != null);

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
                const char dash = '-';
                var propertyName = string.Empty;

                foreach (var arg in _args)
                {
                    var ignoreSetValue = string.IsNullOrWhiteSpace(propertyName);

                    if (ignoreSetValue)
                    {
                        if (string.IsNullOrWhiteSpace(arg) || arg[0] != dash) continue;

                        propertyName = arg.Substring(1);

                        if (!string.IsNullOrWhiteSpace(propertyName) && propertyName[0] == dash)
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

            private bool SetPropertyValue(PropertyInfo targetProperty, string propertyValueString, object result)
            {
                if (targetProperty.PropertyType.GetTypeInfo().IsEnum)
                {
                    var parsedValue = Enum.Parse(
                        targetProperty.PropertyType,
                        propertyValueString,
                        _settings.CaseInsensitiveEnumValues);

                    targetProperty.SetValue(result, Enum.ToObject(targetProperty.PropertyType, parsedValue));

                    return true;
                }

                return targetProperty.PropertyType.IsArray
                    ? PopulateArray(targetProperty, propertyValueString, result)
                    : targetProperty.TrySetBasicType(propertyValueString, result);
            }

            private static bool PopulateArray(
                PropertyInfo targetProperty,
                string propertyValueString,
                object result)
            {
                var optionAttr = Runtime.AttributeCache.RetrieveOne<ArgumentOptionAttribute>(targetProperty);

                var source = propertyValueString.Split(optionAttr.Separator);
                return targetProperty.TrySetArray(source, result);
            }

            private PropertyInfo TryGetProperty(string propertyName)
                => _properties.FirstOrDefault(p =>
                    string.Equals(Runtime.AttributeCache.RetrieveOne<ArgumentOptionAttribute>(p)?.LongName, propertyName, _settings.NameComparer) ||
                    string.Equals(Runtime.AttributeCache.RetrieveOne<ArgumentOptionAttribute>(p)?.ShortName, propertyName, _settings.NameComparer));
        }
    }
}