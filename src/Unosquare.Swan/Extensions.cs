namespace Unosquare.Swan
{
    using Reflection;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    public static partial class Extensions
    {
        private static readonly Lazy<PropertyTypeCache> CopyPropertiesTargets = new Lazy<PropertyTypeCache>(() => new PropertyTypeCache());
        private static readonly Lazy<PropertyTypeCache> CopyPropertiesSources = new Lazy<PropertyTypeCache>(() => new PropertyTypeCache());

        /// <summary>
        /// Iterates over the public, instance, readable properties of the source and
        /// tries to write a compatible value to a public, instance, writable property in the destination
        /// This method only supports basic types and it is not multi level
        /// </summary>
        /// <typeparam name="T">The type of the source.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public static int CopyPropertiesTo<T>(this T source, object target)
        {
            return CopyPropertiesTo(source, target, null);
        }

        /// <summary>
        /// Iterates over the public, instance, readable properties of the source and
        /// tries to write a compatible value to a public, instance, writable property in the destination
        /// This method only supports basic types and it is not multi level
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The destination.</param>
        /// <param name="ignoreProperties">The ignore properties.</param>
        /// <returns>Returns the number of properties that were successfully copied</returns>
        public static int CopyPropertiesTo(this object source, object target, string[] ignoreProperties)
        {
            // TODO: Add recursive so child objects can be copied also

            var copiedProperties = 0;

            // Sources
            var sourceType = source.GetType();
            var sourceProperties = CopyPropertiesSources.Value.Retrieve(sourceType, () =>
            {
                return sourceType.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.CanRead && Definitions.AllBasicTypes.Contains(x.PropertyType));
            });

            // Targets
            var targetType = target.GetType();
            var targetProperties = CopyPropertiesTargets.Value.Retrieve(targetType, () =>
            {
                return targetType.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanWrite && Definitions.AllBasicTypes.Contains(x.PropertyType));
            });

            // Filter properties
            var targetPropertyNames = targetProperties.Select(t => t.Name.ToLowerInvariant());
            var filteredSourceProperties = sourceProperties
                .Where(s => targetPropertyNames.Contains(s.Name.ToLowerInvariant()))
                .ToArray();

            var ignoredProperties = ignoreProperties?.Where(p => string.IsNullOrWhiteSpace(p) == false)
                                        .Select(p => p.ToLowerInvariant())
                                        .ToArray() ?? new string[] { };

            // Copy source properties
            foreach (var sourceProperty in filteredSourceProperties)
            {
                var targetProperty = targetProperties.SingleOrDefault(s => s.Name.ToLowerInvariant() == sourceProperty.Name.ToLowerInvariant());
                if (targetProperty == null) continue;

                // Skip over ignored properties
                if (ignoredProperties.Contains(targetProperty.Name.ToLowerInvariant()))
                    continue;

                try
                {
                    // Direct Copy
                    if (targetProperty.PropertyType == sourceProperty.PropertyType)
                    {
                        targetProperty.SetValue(target, sourceProperty.GetValue(source));
                        copiedProperties++;
                        continue;
                    }

                    // String to target type conversion
                    var sourceStringValue = sourceProperty.GetValue(source).ToStringInvariant();
                    object targetValue;
                    if (Definitions.BasicTypesInfo[targetProperty.PropertyType].TryParse(sourceStringValue, out targetValue))
                    {
                        targetProperty.SetValue(target, targetValue);
                        copiedProperties++;
                    }
                }
                catch
                {
                    // swallow
                }
            }

            return copiedProperties;
        }

        /// <summary>
        /// Copies the properties to new.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="ignoreProperties">The ignore properties.</param>
        /// <returns></returns>
        public static object CopyPropertiesToNew<T>(this object source, string[] ignoreProperties = null)
        {
            var target = Activator.CreateInstance<T>();
            CopyPropertiesTo(source, target, ignoreProperties);
            return target;
        }

        /// <summary>
        /// Iterates over the keys of the source and tries to write a compatible value to a public, 
        /// instance, writable property in the destination.
        /// 
        /// This method only supports basic types and it is not multi level
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="ignoreProperties">The ignore properties.</param>
        /// <returns></returns>
        public static int CopyPropertiesTo(this IDictionary<string, object> source, object target,
            string[] ignoreProperties)
        {
            var copiedProperties = 0;

            var targetType = target.GetType();
            var targetProperties = CopyPropertiesTargets.Value.Retrieve(targetType, () =>
            {
                return targetType.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.CanWrite && Definitions.AllBasicTypes.Contains(x.PropertyType));
            });

            var targetPropertyNames = targetProperties.Select(t => t.Name.ToLowerInvariant());
            var filteredSourceKeys = source
                .Where(s => targetPropertyNames.Contains(s.Key.ToLowerInvariant()) && s.Value != null)
                .ToArray();

            var ignoredProperties = ignoreProperties?.Where(p => string.IsNullOrWhiteSpace(p) == false)
                                        .Select(p => p.ToLowerInvariant())
                                        .ToArray() ?? new string[] {};

            foreach (var sourceKey in filteredSourceKeys)
            {
                var targetProperty =
                    targetProperties.SingleOrDefault(s => s.Name.ToLowerInvariant() == sourceKey.Key.ToLowerInvariant());
                if (targetProperty == null) continue;

                if (ignoredProperties.Contains(targetProperty.Name.ToLowerInvariant()))
                    continue;

                try
                {
                    if (targetProperty.PropertyType == sourceKey.Value.GetType())
                    {
                        targetProperty.SetValue(target, sourceKey.Value);
                        copiedProperties++;
                        continue;
                    }

                    var sourceStringValue = sourceKey.Value.ToStringInvariant();

                    if (targetProperty.PropertyType == typeof(bool))
                        sourceStringValue = sourceStringValue == "1" ? "true" : "false";

                    object targetValue;
                    if (Definitions.BasicTypesInfo[targetProperty.PropertyType].TryParse(sourceStringValue,
                        out targetValue))
                    {
                        targetProperty.SetValue(target, targetValue);
                        copiedProperties++;
                    }
                }
                catch
                {
                    // swallow
                }
            }

            return copiedProperties;
        }

        /// <summary>
        /// Measures the elapsed time of the given action as a TimeSpan
        /// This method uses a high precision Stopwatch.
        /// </summary>
        /// <param name="target">The target.</param>
        /// <returns></returns>
        public static TimeSpan Benchmark(this Action target)
        {
            var sw = new Stopwatch();

            try
            {
                sw.Start();
                target.Invoke();
            }
            catch
            {
                // swallow
            }
            finally
            {
                sw.Stop();
            }

            return TimeSpan.FromTicks(sw.ElapsedTicks);
        }

        /// <summary>
        /// Retrieves the exception message, plus all the inner exception messages separated by new lines
        /// </summary>
        /// <param name="ex">The ex.</param>
        /// <param name="priorMessage">The prior message.</param>
        /// <returns></returns>
        public static string ExceptionMessage(this Exception ex, string priorMessage = "")
        {
            while (true)
            {
                var fullMessage = string.IsNullOrWhiteSpace(priorMessage) ? ex.Message : priorMessage + "\r\n" + ex.Message;

                if (string.IsNullOrWhiteSpace(ex.InnerException?.Message))
                    return fullMessage;

                ex = ex.InnerException;
                priorMessage = fullMessage;
            }
        }
    }
}