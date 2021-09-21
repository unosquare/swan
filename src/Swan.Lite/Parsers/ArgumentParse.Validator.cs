using Swan.Reflection;
using System.Collections.Generic;
using System.Linq;

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
            private readonly List<IPropertyProxy> _updatedList = new();
            private readonly ArgumentParserSettings _settings;

            private readonly IPropertyProxy[] _properties;

            public Validator(
                IPropertyProxy[] properties,
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

            public List<string> UnknownList { get; } = new();
            public List<string> RequiredList { get; } = new();

            public bool IsValid() => (_settings.IgnoreUnknownArguments || !UnknownList.Any()) && !RequiredList.Any();

            public IEnumerable<ArgumentOptionAttribute> GetPropertiesOptions() =>
                _properties
                    .Select(p =>p.Attribute<ArgumentOptionAttribute>())
                    .Where(x => x != null);

            private void GetRequiredList()
            {
                foreach (var targetProperty in _properties)
                {
                    var optionAttr = targetProperty.Attribute<ArgumentOptionAttribute>();

                    if (optionAttr == null || optionAttr.IsRequired == false)
                        continue;

                    if (targetProperty.Read(_instance) == null)
                    {
                        RequiredList.Add(optionAttr.LongName ?? optionAttr.ShortName);
                    }
                }
            }

            private void SetDefaultValues()
            {
                foreach (var targetProperty in _properties.Except(_updatedList))
                {
                    var optionAttr = targetProperty.Attribute<ArgumentOptionAttribute>();

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
                    var optionAttr = targetProperty.Attribute<ArgumentOptionAttribute>();
                    if (optionAttr is null || !optionAttr.IsDefault)
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
                    else if (targetProperty.PropertyType.NativeType == typeof(bool))
                    {
                        // If the arg is a boolean property set it to true.
                        targetProperty.TryWrite(_instance, true);

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
                IPropertyProxy targetProperty,
                string propertyValueString,
                object result,
                ArgumentOptionAttribute? optionAttr = null)
            {
                return targetProperty.PropertyType.IsArray
                    ? targetProperty.PropertyInfo.TrySetArray(propertyValueString.Split(optionAttr?.Separator ?? ','), result)
                    : targetProperty.TryWrite(result, propertyValueString);
            }

            private IPropertyProxy? TryGetProperty(string propertyName) =>
                _properties.FirstOrDefault(p =>
                    string.Equals(p.Attribute<ArgumentOptionAttribute>()?.LongName, propertyName, _settings.NameComparer) ||
                    string.Equals(p.Attribute<ArgumentOptionAttribute>()?.ShortName, propertyName, _settings.NameComparer));
        }
    }
}