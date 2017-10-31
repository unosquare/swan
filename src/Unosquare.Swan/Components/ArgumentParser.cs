namespace Unosquare.Swan.Components
{
    using Reflection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Attributes;

    /// <summary>
    /// Provides methods to parse command line arguments.
    /// Based on CommandLine (Copyright 2005-2015 Giacomo Stelluti Scala and Contributors.)
    /// </summary>
    public class ArgumentParser
    {
        private const char Dash = '-';

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentParser"/> class.
        /// </summary>
        public ArgumentParser()
            : this(new ArgumentParserSettings())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentParser" /> class,
        /// configurable with <see cref="ArgumentParserSettings" /> using a delegate.
        /// </summary>
        /// <param name="parseSettings">The parse settings.</param>
        public ArgumentParser(ArgumentParserSettings parseSettings)
        {
            Settings = parseSettings ?? throw new ArgumentNullException(nameof(parseSettings));
        }

        /// <summary>
        /// Gets the instance that implements <see cref="ArgumentParserSettings"/> in use.
        /// </summary>
        public ArgumentParserSettings Settings { get; }

        /// <summary>
        /// Parses a string array of command line arguments constructing values in an instance of type <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T">The type of the options</typeparam>
        /// <param name="args">The arguments.</param>
        /// <param name="instance">The instance.</param>
        /// <returns>True if was converted successfully; otherwise, false</returns>
        /// <exception cref="ArgumentNullException">
        /// The exception that is thrown when a null reference (Nothing in Visual Basic) 
        /// is passed to a method that does not accept it as a valid argument
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// The exception that is thrown when a method call is invalid for the object's current state
        /// </exception>
        public bool ParseArguments<T>(IEnumerable<string> args, T instance)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            var properties = GetTypeProperties(typeof(T)).ToArray();

            if (properties.Any() == false)
                throw new InvalidOperationException($"Type {typeof(T).Name} is not valid");

            var unknownList = new List<string>();
            var requiredList = new List<string>();
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

                    if (SetPropertyValue(targetProperty, arg, instance))
                        updatedList.Add(targetProperty);

                    propertyName = string.Empty;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(arg) || arg[0] != Dash) continue;

                    propertyName = arg.Substring(1);
                    if (propertyName[0] == Dash) propertyName = propertyName.Substring(1);

                    var targetProperty = TryGetProperty(properties, propertyName);

                    // If the arg is a boolean property set it to true.
                    if (targetProperty == null || targetProperty.PropertyType != typeof(bool)) continue;

                    if (SetPropertyValue(targetProperty, true.ToString(), instance))
                        updatedList.Add(targetProperty);

                    propertyName = string.Empty;
                }
            }

            if (string.IsNullOrEmpty(propertyName) == false)
            {
                unknownList.Add(propertyName);
            }

            foreach (var targetProperty in properties.Except(updatedList))
            {
                var defaultValue = targetProperty.GetCustomAttribute<ArgumentOptionAttribute>()?.DefaultValue;

                if (defaultValue == null)
                    continue;

                SetPropertyValue(targetProperty, defaultValue.ToString(), instance);
            }

            foreach (var targetProperty in properties)
            {
                var optionAttr = targetProperty.GetCustomAttribute<ArgumentOptionAttribute>();

                if (optionAttr == null || optionAttr.Required == false)
                    continue;

                if (targetProperty.GetValue(instance) == null)
                {
                    requiredList.Add(optionAttr.LongName ?? optionAttr.ShortName);
                }
            }

            if ((Settings.IgnoreUnknownArguments || !unknownList.Any()) && !requiredList.Any()) return true;

#if !NETSTANDARD1_3 && !UWP
            if (Settings.WriteBanner)
                Runtime.WriteWelcomeBanner();
#endif

            WriteUsage(properties);

            if (unknownList.Any())
                $"Unknown arguments: {string.Join(", ", unknownList)}".WriteLine();

            if (requiredList.Any())
                $"Required arguments: {string.Join(", ", requiredList)}".WriteLine();

            return false;
        }

        private static IEnumerable<PropertyInfo> GetTypeProperties(Type type)
            => Runtime.PropertyTypeCache.Value.Retrieve(type, PropertyTypeCache.GetAllPublicPropertiesFunc(type));

        private static void WriteUsage(IEnumerable<PropertyInfo> properties)
        {
            var options = properties.Select(p => p.GetCustomAttribute<ArgumentOptionAttribute>())
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
            var optionAttr = targetProperty.GetCustomAttribute<ArgumentOptionAttribute>();

            if (targetProperty.PropertyType.GetTypeInfo().IsEnum)
            {
                var parsedValue = Enum.Parse(
                    targetProperty.PropertyType,
                    propertyValueString,
                    Settings.CaseInsensitiveEnumValues);
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

            if (targetProperty.PropertyType.TryParseBasicType(propertyValueString,
                out var propertyValue))
            {
                targetProperty.SetValue(result, propertyValue);
                return true;
            }

            return false;
        }

        private PropertyInfo TryGetProperty(IEnumerable<PropertyInfo> properties, string propertyName)
            => properties.FirstOrDefault(p =>
                string.Equals(p.GetCustomAttribute<ArgumentOptionAttribute>()?.LongName, propertyName,
                    Settings.NameComparer) ||
                string.Equals(p.GetCustomAttribute<ArgumentOptionAttribute>()?.ShortName, propertyName,
                    Settings.NameComparer));
    }
}