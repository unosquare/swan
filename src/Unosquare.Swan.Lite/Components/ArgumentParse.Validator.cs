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
            private bool _result;
            private readonly T _instance;
            private readonly IEnumerable<string> _args;
            private readonly ArgumentParserSettings _settings;
            private readonly PropertyInfo[] _properties;
            private readonly string _verbName;

            public Validator(IEnumerable<string> args, T instance, ArgumentParserSettings settings)
            {
                if (instance == null)
                    throw new ArgumentNullException(nameof(instance));
                
                _instance = instance;
                _args = args ?? throw new ArgumentNullException(nameof(args));

                _settings = settings;

                _properties = Runtime.PropertyTypeCache.RetrieveAllProperties<T>(true).ToArray();
                _verbName = string.Empty;

                if (_properties.Any(x => x.GetCustomAttributes(typeof(VerbOptionAttribute), false).Any()))
                {
                    var selectedVerb = !args.Any()
                        ? null
                        : _properties.FirstOrDefault(x =>
                            Runtime.AttributeCache.RetrieveOne<VerbOptionAttribute>(x).Name.Equals(args.First()));

                    if (selectedVerb == null)
                    {
                        ReportUnknownVerb();
                        return;
                    }

                    _verbName = selectedVerb.Name;
                    if (instance.GetType().GetProperty(_verbName).GetValue(instance) == null)
                    {
                        var propertyInstance = Activator.CreateInstance(selectedVerb.PropertyType);
                        instance.GetType().GetProperty(_verbName).SetValue(instance, propertyInstance);
                    }

                    _properties = Runtime.PropertyTypeCache.RetrieveAllProperties(selectedVerb.PropertyType, true).ToArray();
                }

                if (_properties.Any() == false)
                    throw new InvalidOperationException($"Type {typeof(T).Name} is not valid");

                var requiredList = new List<string>();
                var updatedList = new List<PropertyInfo>();
                var unknownList = PopulateInstance(updatedList);

                foreach (var targetProperty in _properties.Except(updatedList))
                {
                    var defaultValue = Runtime.AttributeCache.RetrieveOne<ArgumentOptionAttribute>(targetProperty)?.DefaultValue;

                    if (defaultValue == null)
                        continue;

                    if (string.IsNullOrEmpty(_verbName))
                    {
                        SetPropertyValue(targetProperty, defaultValue.ToString(), instance);
                    }
                    else
                    {
                        var property = instance.GetType().GetProperty(_verbName);
                        if (SetPropertyValue(targetProperty, defaultValue.ToString(), property.GetValue(instance, null)))
                            updatedList.Add(targetProperty);
                    }
                }

                foreach (var targetProperty in _properties)
                {
                    var optionAttr = Runtime.AttributeCache.RetrieveOne<ArgumentOptionAttribute>(targetProperty);

                    if (optionAttr == null || optionAttr.Required == false)
                        continue;

                    if (string.IsNullOrWhiteSpace(_verbName))
                    {
                        if (targetProperty.GetValue(instance) == null)
                        {
                            requiredList.Add(optionAttr.LongName ?? optionAttr.ShortName);
                        }
                    }
                    else
                    {
                        var property = instance.GetType().GetProperty(_verbName);

                        if (targetProperty.GetValue(property.GetValue(instance)) == null)
                        {
                            requiredList.Add(optionAttr.LongName ?? optionAttr.ShortName);
                        }
                    }
                }

                if ((settings.IgnoreUnknownArguments || !unknownList.Any()) && !requiredList.Any())
                {
                    _result = true;
                    return;
                }

#if !NETSTANDARD1_3 && !UWP
                if (settings.WriteBanner)
                    Runtime.WriteWelcomeBanner();
#endif

                WriteUsage(_properties);

                if (unknownList.Any())
                    $"Unknown arguments: {string.Join(", ", unknownList)}".WriteLine(ConsoleColor.Red);

                if (requiredList.Any())
                    $"Required arguments: {string.Join(", ", requiredList)}".WriteLine(ConsoleColor.Red);
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

            public bool IsValid() => _result;

            private List<string> PopulateInstance(List<PropertyInfo> updatedList)
            {
                var unknownList = new List<string>();
                var propertyName = string.Empty;

                foreach (var arg in _args)
                {
                    if (string.IsNullOrWhiteSpace(propertyName) == false)
                    {
                        var targetProperty = TryGetProperty(propertyName);

                        // Skip if the property is not found
                        if (targetProperty == null)
                        {
                            unknownList.Add(propertyName);
                            propertyName = string.Empty;
                            continue;
                        }

                        if (string.IsNullOrEmpty(_verbName))
                        {
                            if (SetPropertyValue(targetProperty, arg, _instance))
                                updatedList.Add(targetProperty);
                        }
                        else
                        {
                            var property = _instance.GetType().GetProperty(_verbName);

                            if (property != null && SetPropertyValue(targetProperty, arg, property.GetValue(_instance, null)))
                                updatedList.Add(targetProperty);
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
                                updatedList.Add(targetProperty);
                        }
                        else
                        {
                            var property = _instance.GetType().GetProperty(_verbName);

                            if (property != null && SetPropertyValue(targetProperty, true.ToString(), property.GetValue(_instance, null)))
                                updatedList.Add(targetProperty);
                        }

                        propertyName = string.Empty;
                    }
                }

                if (string.IsNullOrEmpty(propertyName) == false)
                {
                    unknownList.Add(propertyName);
                }

                return unknownList;
            }

            private static void WriteUsage(IEnumerable<PropertyInfo> properties)
            {
                var options = properties.Select(p => Runtime.AttributeCache.RetrieveOne<ArgumentOptionAttribute>(p))
                    .Where(x => x != null);

                foreach (var option in options)
                {
                    string.Empty.WriteLine();

                    // TODO: If Enum list values
                    var shortName = string.IsNullOrWhiteSpace(option.ShortName) ? string.Empty : $"-{option.ShortName}";
                    var longName = string.IsNullOrWhiteSpace(option.LongName) ? string.Empty : $"--{option.LongName}";
                    var comma = string.IsNullOrWhiteSpace(shortName) || string.IsNullOrWhiteSpace(longName)
                        ? string.Empty
                        : ", ";
                    var defaultValue = option.DefaultValue == null ? string.Empty : $"(Default: {option.DefaultValue}) ";

                    $"  {shortName}{comma}{longName}\t\t{defaultValue}{option.HelpText}".WriteLine(ConsoleColor.Cyan);
                }

                string.Empty.WriteLine();
                "  --help\t\tDisplay this help screen.".WriteLine(ConsoleColor.Cyan);
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