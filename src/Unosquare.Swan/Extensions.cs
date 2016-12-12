namespace Unosquare.Swan
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using Unosquare.Swan.Reflection;

    public static partial class Extensions
    {
        private static readonly Lazy<PropertyTypeCache> CopyPropertiesTargets = new Lazy<PropertyTypeCache>(() => { return new PropertyTypeCache(); });
        private static readonly Lazy<PropertyTypeCache> CopyPropertiesSources = new Lazy<PropertyTypeCache>(() => { return new PropertyTypeCache(); });

        /// <summary>
        /// Iterates over the public, instance, readable properties of the source and
        /// tries to write a compatible value to a public, instance, writeable property in the destination
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
        /// tries to write a compatible value to a public, instance, writeable property in the destination
        /// This method only supports basic types and it is not multi level
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The destination.</param>
        /// <param name="ignoreProperties">The ignore properties.</param>
        /// <returns>Returns the number of properties that were successfully copied</returns>
        public static int CopyPropertiesTo(this object source, object target, string[] ignoreProperties)
        {

            var copiedProperties = 0;

            // Sources
            var sourceType = source.GetType();
            var sourceProperties = CopyPropertiesSources.Value.Retrieve(sourceType, () =>
            {
                return sourceType.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.CanRead && Constants.AllBasicTypes.Contains(x.PropertyType));
            });

            // Targets
            var targetType = target.GetType();
            var targetProperties = CopyPropertiesTargets.Value.Retrieve(targetType, () =>
            {
                return targetType.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanWrite && Constants.AllBasicTypes.Contains(x.PropertyType));
            });

            // Filter properties
            var targetPropertyNames = targetProperties.Select(t => t.Name.ToLowerInvariant());
            var filteredSourceProperties = sourceProperties
                .Where(s => targetPropertyNames.Contains(s.Name.ToLowerInvariant()))
                .ToArray();

            var ignoredProperties = ignoreProperties == null ? 
                new string[] { } : 
                ignoreProperties
                    .Where(p => string.IsNullOrWhiteSpace(p) == false)
                    .Select(p => p.ToLowerInvariant())
                    .ToArray();

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
                    object targetValue = null;
                    if (Constants.BasicTypesInfo[targetProperty.PropertyType].TryParse(sourceStringValue, out targetValue))
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

    }
}