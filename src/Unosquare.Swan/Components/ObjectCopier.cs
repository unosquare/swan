namespace Unosquare.Swan.Components
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Unosquare.Swan.Reflection;

    /// <summary>
    /// Copy from one object to other one
    /// </summary>
    public static class ObjectCopier
    {
        private static readonly Lazy<PropertyTypeCache> CopyPropertiesTargets = new Lazy<PropertyTypeCache>(() => new PropertyTypeCache());
        private static readonly Lazy<PropertyTypeCache> CopyPropertiesSources = new Lazy<PropertyTypeCache>(() => new PropertyTypeCache());

        /// <summary>
        /// Copies the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="propertiesToCopy">The properties to copy.</param>
        /// <param name="ignoreProperties">The ignore properties.</param>
        /// <returns>Copied properties count</returns>
        public static int Copy(
            object source,
            object target,
            string[] propertiesToCopy = null,
            string[] ignoreProperties = null)
        {
            var copiedProperties = 0;

            // Sources
            var sourceType = source.GetType();

            var sourceProperties = CopyPropertiesSources.Value.Retrieve(sourceType, () =>
            {
                return sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(x => x.CanRead);
            });

            // Targets
            var targetType = target.GetType();
            var targetProperties = CopyPropertiesTargets.Value.Retrieve(targetType, () =>
            {
                return targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanWrite);
            });

            // Filter properties
            var targetPropertyNames = targetProperties.Select(t => t.Name.ToLowerInvariant());
            var filteredSourceProperties = sourceProperties
                .Where(s => targetPropertyNames.Contains(s.Name.ToLowerInvariant()))
                .ToArray();

            var requiredProperties = propertiesToCopy?.Where(p => string.IsNullOrWhiteSpace(p) == false)
                                        .Select(p => p.ToLowerInvariant())
                                        .ToArray() ?? null;

            var ignoredProperties = ignoreProperties?.Where(p => string.IsNullOrWhiteSpace(p) == false)
                                        .Select(p => p.ToLowerInvariant())
                                        .ToArray() ?? null;

            // Copy source properties
            foreach (var sourceProperty in filteredSourceProperties)
            {
                
                var targetProperty = targetProperties.SingleOrDefault(s => s.Name.ToLowerInvariant() == sourceProperty.Name.ToLowerInvariant());
                if (targetProperty == null) continue;

                if (requiredProperties != null && requiredProperties.Contains(targetProperty.Name.ToLowerInvariant()) == false)
                    continue;

                if (ignoredProperties != null && ignoredProperties.Contains(targetProperty.Name.ToLowerInvariant()))
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
    }
}
