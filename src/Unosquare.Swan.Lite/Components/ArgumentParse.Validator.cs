namespace Unosquare.Swan.Components
{
    using System.Linq;
    using System.Reflection;
    using Attributes;
    using System;
    using System.Collections.Generic;

    public partial class ArgumentParser
    {
        internal class Validator<T>
        {
            public readonly List<string> UnknownList = new List<string>();
            public readonly List<string> RequiredList = new List<string>();

            private readonly T _instance;
            private readonly Type _type;
            private readonly string[] _args;
            private readonly List<PropertyInfo> _updatedList = new List<PropertyInfo>();
            private readonly ArgumentParserSettings _settings;

            private bool _result;
            private PropertyInfo[] _properties;
            private string _verbName;

            public Validator(IEnumerable<string> args, T instance, ArgumentParserSettings settings)
            {
                _args = args?.ToArray() ?? throw new ArgumentNullException(nameof(args));

                if (instance == null)
                    throw new ArgumentNullException(nameof(instance));

                _instance = instance;
                _type = instance.GetType();

                _settings = settings;
                _properties = Runtime.PropertyTypeCache.RetrieveAllProperties<T>(true).ToArray();
                _verbName = string.Empty;

                if (!ValidateVerb()) return;

                if (_properties.Any() == false)
                    throw new InvalidOperationException($"Type {typeof(T).Name} is not valid");

                PopulateInstance();
                SetDefaultValues();
                GetRequiredList();

                _result = (settings.IgnoreUnknownArguments || !UnknownList.Any()) && !RequiredList.Any();
            }

            public bool IsValid() => _result;
            
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

                    if (string.IsNullOrWhiteSpace(_verbName))
                    {
                        if (targetProperty.GetValue(_instance) == null)
                        {
                            RequiredList.Add(optionAttr.LongName ?? optionAttr.ShortName);
                        }
                    }
                    else
                    {
                        var property = _type.GetProperty(_verbName);

                        if (targetProperty.GetValue(property.GetValue(_instance)) == null)
                        {
                            RequiredList.Add(optionAttr.LongName ?? optionAttr.ShortName);
                        }
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

                    if (string.IsNullOrEmpty(_verbName))
                    {
                        SetPropertyValue(targetProperty, defaultValue.ToString(), _instance);
                    }
                    else
                    {
                        var property = _type.GetProperty(_verbName);
                        if (SetPropertyValue(targetProperty, defaultValue.ToString(), property.GetValue(_instance, null)))
                            _updatedList.Add(targetProperty);
                    }
                }
            }

            private bool ValidateVerb()
            {
                if (!_properties.Any(x => x.GetCustomAttributes(typeof(VerbOptionAttribute), false).Any()))
                    return true;

                var selectedVerb = !_args.Any()
                    ? null
                    : _properties.FirstOrDefault(x =>
                        Runtime.AttributeCache.RetrieveOne<VerbOptionAttribute>(x).Name.Equals(_args.First()));

                if (selectedVerb == null)
                {
                    ReportUnknownVerb();
                    return false;
                }

                _verbName = selectedVerb.Name;
                if (_type.GetProperty(_verbName).GetValue(_instance) == null)
                {
                    var propertyInstance = Activator.CreateInstance(selectedVerb.PropertyType);
                    _type.GetProperty(_verbName).SetValue(_instance, propertyInstance);
                }

                _properties = Runtime.PropertyTypeCache.RetrieveAllProperties(selectedVerb.PropertyType, true)
                    .ToArray();

                return true;
            }

            private void ReportUnknownVerb()
            {
                "No verb was specified".WriteLine(ConsoleColor.Red);
                "Valid verbs:".WriteLine(ConsoleColor.Cyan);
                _properties.Select(x => Runtime.AttributeCache.RetrieveOne<VerbOptionAttribute>(x)).Where(x => x != null)
                    .Select(x => $"  {x.Name}\t\t{x.HelpText}")
                    .ToList()
                    .ForEach(x => x.WriteLine(ConsoleColor.Cyan));

                _result = false;
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

                        if (string.IsNullOrEmpty(_verbName))
                        {
                            if (SetPropertyValue(targetProperty, arg, _instance))
                                _updatedList.Add(targetProperty);
                        }
                        else
                        {
                            var property = _type.GetProperty(_verbName);

                            if (property != null && SetPropertyValue(targetProperty, arg, property.GetValue(_instance, null)))
                                _updatedList.Add(targetProperty);
                        }

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

                        if (string.IsNullOrEmpty(_verbName))
                        {
                            if (SetPropertyValue(targetProperty, true.ToString(), _instance))
                                _updatedList.Add(targetProperty);
                        }
                        else
                        {
                            var property = _type.GetProperty(_verbName);

                            if (property != null && SetPropertyValue(targetProperty, true.ToString(), property.GetValue(_instance, null)))
                                _updatedList.Add(targetProperty);
                        }

                        propertyName = string.Empty;
                    }
                }

                if (string.IsNullOrEmpty(propertyName) == false)
                {
                    UnknownList.Add(propertyName);
                }
            }

            private bool SetPropertyValue<T>(PropertyInfo targetProperty, string propertyValueString, T result)
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