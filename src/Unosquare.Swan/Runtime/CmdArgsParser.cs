using System;
using System.Collections.Generic;
using System.IO;
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
        /// Parses a string array of command line arguments constructing values in an instance of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="args">The arguments.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">args</exception>
        /// <exception cref="System.InvalidOperationException"></exception>
        public Tuple<bool, T> ParseArguments<T>(IEnumerable<string> args)
        {
            if (args == null) throw new ArgumentNullException("args");

            var properties = GetTypeProperties(typeof(T));

            if (properties.Any() == false)
                throw new InvalidOperationException($"Type {typeof(T).Name} is not valid");

            var instance = Activator.CreateInstance<T>();
            var unknownList = new List<string>();

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

                    SetPropertyValue(targetProperty, arg, instance);
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
                            SetPropertyValue(targetProperty, true.ToString(), instance);
                            propertyName = string.Empty;
                        }
                    }
                }
            }

            // TODO: Set default values
            var result = true;

            if (unknownList.Any())
            {
                result = false;

                if (Settings.WriteBanner)
                { 
                    Terminal.WriteBanner();
                }

                Terminal.WriteUsage(properties);
            }

            return new Tuple<bool, T>(result, instance);
        }

        private void SetPropertyValue<T>(PropertyInfo property, string value, T instance)
        {
            // Parse and assign the basic type value to the property
            try
            {
                if (property.PropertyType.GetTypeInfo().IsEnum)
                {
                    // TODO: How to handle an enum?

                    //var enumInstance = Activator.CreateInstance(property.PropertyType);

                    //if (Enum.TryParse(value, true, out enumInstance))
                    //    property.SetValue(instance, Enum.ToObject(property.PropertyType, enumInstance));
                }
                else
                {
                    object propertyValue;
                    if (Constants.BasicTypesInfo[property.PropertyType].TryParse(value,
                        out propertyValue))
                        property.SetValue(instance, propertyValue);
                }
            }
            catch
            {
                // ignored
            }
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
