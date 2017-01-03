namespace Unosquare.Swan.Runtime
{
    using Reflection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Provides methods to parse command line arguments.
    /// Based on CommandLine (Copyright 2005-2015 Giacomo Stelluti Scala and Contributors.)
    /// </summary>
    public class ArgumentParser
    {
        private const char Dash = '-';

        private static readonly Lazy<ArgumentParser> DefaultParser = new Lazy<ArgumentParser>(() => new ArgumentParser());

        private static readonly object SyncLock = new object();
        private static readonly PropertyTypeCache TypeCache = new PropertyTypeCache();

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentParser"/> class.
        /// </summary>
        public ArgumentParser() : this(new ArgumentParserSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentParser" /> class,
        /// configurable with <see cref="ArgumentParserSettings" /> using a delegate.
        /// </summary>
        /// <param name="parseSettings">The parse settings.</param>
        public ArgumentParser(ArgumentParserSettings parseSettings)
        {
            Settings = parseSettings;
        }

        /// <summary>
        /// Gets the singleton instance created with basic defaults.
        /// </summary>
        public static ArgumentParser Default => DefaultParser.Value;

        /// <summary>
        /// Gets the instance that implements <see cref="ArgumentParserSettings"/> in use.
        /// </summary>
        public ArgumentParserSettings Settings { get; }

        /// <summary>
        /// Parses a string array of command line arguments constructing values in an instance of type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args">The arguments.</param>
        /// <param name="instance">The instance.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">args</exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        public bool ParseArguments<T>(IEnumerable<string> args, T instance)
        {
            if (args == null) throw new ArgumentNullException(nameof(args));

            var properties = GetTypeProperties(typeof(T));

            if (properties.Any() == false)
                throw new InvalidOperationException($"Type {typeof(T).Name} is not valid");

            var unknownList = new List<string>();
            var updatedList = new List<PropertyInfo>();
            var propertyName = string.Empty;

            foreach (var arg in args)
            {
                if (string.IsNullOrWhiteSpace(propertyName) == false)
                {
                    var targetProperty = TryGetProperty(properties, propertyName);

                    // Skip if the property is not found
                    if (targetProperty == null)
                    {
                        unknownList.Add(propertyName);
                        propertyName = string.Empty;
                        continue;
                    }

                    if (SetPropertyValue(targetProperty, arg, instance)) updatedList.Add(targetProperty);
                    propertyName = string.Empty;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(arg)) continue;

                    if (arg[0] == Dash) // TODO: Check double dash
                    {
                        propertyName = arg.Substring(1);
                        if (propertyName[0] == Dash) propertyName = propertyName.Substring(1);

                        var targetProperty = TryGetProperty(properties, propertyName);

                        // If the arg is a boolean property set it to true.
                        if (targetProperty != null && targetProperty.PropertyType == typeof(bool))
                        {
                            if (SetPropertyValue(targetProperty, true.ToString(), instance)) updatedList.Add(targetProperty);
                            propertyName = string.Empty;
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(propertyName) == false)
                unknownList.Add(propertyName);

            var result = true;

            if (Settings.IgnoreUnknownArguments && unknownList.Any())
            {
                result = false;

                if (Settings.WriteBanner) CurrentApp.WriteWelcomeBanner();

                WriteUsage(properties);
                $"Unknown arguments: {string.Join(", ", unknownList)}".WriteLine();
            }

            if (result)
            {
                // TODO: Set default values
            }

            return result;
        }

        private bool SetPropertyValue<T>(PropertyInfo targetProperty, string propertyValueString, T result)
        {
            // Parse and assign the basic type value to the property
            try
            {
                var optionAttr = targetProperty.GetCustomAttribute<ArgumentOptionAttribute>();

                if (optionAttr == null) return false;

                if (targetProperty.PropertyType.GetTypeInfo().IsEnum)
                {
                    var parsedValue = Enum.Parse(targetProperty.PropertyType, propertyValueString, Settings.CaseInsensitiveEnumValues);
                    targetProperty.SetValue(result, Enum.ToObject(targetProperty.PropertyType, parsedValue));

                    return true;
                }
                else if (targetProperty.PropertyType.IsCollection())
                {
                    var itemType = targetProperty.PropertyType.GetElementType();
                    var primitiveValue = Definitions.AllBasicTypes.Contains(itemType);
                    var propertyValue = propertyValueString.Split(optionAttr.Separator);

                    var arr = Array.CreateInstance(itemType, propertyValue.Cast<object>().Count());

                    var i = 0;
                    foreach (var value in propertyValue)
                    {
                        if (primitiveValue)
                        {
                            object itemvalue;
                            if (Definitions.BasicTypesInfo[itemType].TryParse(value, out itemvalue))
                                arr.SetValue(itemvalue, i++);
                        }
                        else
                        {
                            arr.SetValue(value, i++);
                        }
                    }

                    targetProperty.SetValue(result, arr);
                }
                else
                {
                    object propertyValue;
                    if (Definitions.BasicTypesInfo[targetProperty.PropertyType].TryParse(propertyValueString,
                        out propertyValue))
                    {
                        targetProperty.SetValue(result, propertyValue);
                        return true;
                    }
                }
            }
            catch
            {
                // ignored
            }


            return false;
        }

        private PropertyInfo TryGetProperty(IEnumerable<PropertyInfo> properties, string propertyName)
        {
            return properties.FirstOrDefault(p =>
                    p.GetCustomAttribute<ArgumentOptionAttribute>()?.LongName == propertyName ||
                    p.GetCustomAttribute<ArgumentOptionAttribute>()?.ShortName == propertyName);
        }

        private static IEnumerable<PropertyInfo> GetTypeProperties(Type type)
        {
            lock (SyncLock)
            {
                return TypeCache.Retrieve(type, () =>
                {
                    return type.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.CanRead || p.CanWrite)
                        .ToArray();
                });
            }
        }

        /// <summary>
        /// Writes the application usage.
        /// </summary>
        /// <param name="properties">The properties.</param>
        private static void WriteUsage(IEnumerable<PropertyInfo> properties)
        {
            var options = properties.Select(p => p.GetCustomAttribute<ArgumentOptionAttribute>()).Where(x => x != null);

            foreach (var option in options)
            {
                "".WriteLine();
                // TODO: If Enum list values
                var shorName = string.IsNullOrWhiteSpace(option.ShortName) ? string.Empty : $"-{option.ShortName}";
                var longName = string.IsNullOrWhiteSpace(option.LongName) ? string.Empty : $"--{option.LongName}";
                var comma = string.IsNullOrWhiteSpace(shorName) || string.IsNullOrWhiteSpace(longName) ? string.Empty : ", ";
                var defaultValue = option.DefaultValue == null ? string.Empty : $"(Default: {option.DefaultValue}) ";
                $"  {shorName}{comma}{longName}\t\t{defaultValue}{option.HelpText}".WriteLine(ConsoleColor.Cyan);
            }

            "".WriteLine();
            "  --help\t\tDisplay this help screen.".WriteLine(ConsoleColor.Cyan);
        }
    }
}
