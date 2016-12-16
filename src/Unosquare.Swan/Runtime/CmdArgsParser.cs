using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unosquare.Swan.Reflection;

namespace Unosquare.Swan.Runtime
{
    /// <summary>
    /// Provides methods to parse command line arguments.
    /// Based on CommandLine (Copyright 2005-2015 Giacomo Stelluti Scala and Contributors.)
    /// </summary>
    public class CmdArgsParser
    {
        const char Dash = '-';

        private bool disposed;
        private static readonly Lazy<CmdArgsParser> DefaultParser = new Lazy<CmdArgsParser>(() => new CmdArgsParser(new ParserSettings { HelpWriter = Console.Error }));

        private static readonly object SyncLock = new object();
        private static readonly PropertyTypeCache TypeCache = new PropertyTypeCache();

        /// <summary>
        /// Initializes a new instance of the <see cref="CmdArgsParser"/> class.
        /// </summary>
        public CmdArgsParser()
        {
            Settings = new ParserSettings();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CmdArgsParser" /> class,
        /// configurable with <see cref="ParserSettings" /> using a delegate.
        /// </summary>
        /// <param name="parseSettings">The parse settings.</param>
        public CmdArgsParser(ParserSettings parseSettings)
        {
            Settings = parseSettings;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="CmdArgsParser"/> class.
        /// </summary>
        ~CmdArgsParser()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets the singleton instance created with basic defaults.
        /// </summary>
        public static CmdArgsParser Default => DefaultParser.Value;

        /// <summary>
        /// Gets the instance that implements <see cref="ParserSettings"/> in use.
        /// </summary>
        public ParserSettings Settings { get; }

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
            if (args == null) throw new ArgumentNullException("args");

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
            
            var result = true;

            if (Settings.IgnoreUnknownArguments && unknownList.Any())
            {
                result = false;
                
                if (Settings.WriteBanner) Terminal.WriteBanner();

                Terminal.WriteUsage(properties);
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
                var optionAttr = targetProperty.GetCustomAttribute<OptionAttribute>();

                if (optionAttr == null) return false;

                if (targetProperty.PropertyType.GetTypeInfo().IsEnum)
                {
                    // TODO: How to handle an enum?

                    //var enumInstance = Activator.CreateInstance(property.PropertyType);

                    //if (Enum.TryParse(value, true, out enumInstance))
                    //    property.SetValue(instance, Enum.ToObject(property.PropertyType, enumInstance));
                    
                    return true;
                }
                else if (targetProperty.IsCollection())
                {
                    var itemType = targetProperty.PropertyType.GetElementType();
                    var primitiveValue = Constants.AllBasicTypes.Contains(itemType);
                    var propertyValue = propertyValueString.ToString().Split(optionAttr.Separator);

                    var arr = Array.CreateInstance(itemType, propertyValue.Cast<object>().Count());

                    var i = 0;
                    foreach (var value in propertyValue)
                    {
                        if (primitiveValue)
                        {
                            object itemvalue;
                            if (Constants.BasicTypesInfo[itemType].TryParse(value.ToString(), out itemvalue))
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
                    if (Constants.BasicTypesInfo[targetProperty.PropertyType].TryParse(propertyValueString,
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
                    p.GetCustomAttribute<OptionAttribute>()?.LongName == propertyName ||
                    p.GetCustomAttribute<OptionAttribute>()?.ShortName == propertyName);
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
        /// Frees resources owned by the instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }
        
        private void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                Settings?.Dispose();

                disposed = true;
            }
        }
    }
}
