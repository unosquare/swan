using Swan.Reflection;
using System;
using System.Collections.Generic;

namespace Swan.Mapping
{
    /// <summary>
    /// A dictionary containing a case-insensitive lookup of <see cref="MapPath"/> elements.
    /// </summary>
    public sealed class MapPathSet : Dictionary<string, MapPath>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="MapPathSet"/> class.
        /// </summary>
        internal MapPathSet()
            : base(128, StringComparer.InvariantCultureIgnoreCase)
        {
            // placeholder
        }

        /// <summary>
        /// Gets or sets an item in the collection by means of target property proxy lookup.
        /// </summary>
        /// <param name="targetProperty">The target property proxy to find.</param>
        /// <returns>Returns the corresponding <see cref="MapPath.SourcePath"/></returns>
        public IReadOnlyList<IPropertyProxy> this[IPropertyProxy targetProperty]
        {
            get
            {
                if (!TryGetValue(targetProperty, out var value) || value is null)
                    throw new KeyNotFoundException($"Target property {targetProperty} was not found.");

                return value.SourcePath;
            }
            set
            {
                if (targetProperty is null || value is null)
                {
                    throw new ArgumentException(
                        $"The {nameof(MapPath)} cannot be set as it requires both, target and source.", nameof(value));
                }

                this[targetProperty.PropertyName] = new MapPath(targetProperty, value);
            }
        }

        /// <summary>
        /// Tries to get the 
        /// </summary>
        /// <param name="targetProperty"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGetValue(IPropertyProxy targetProperty, out MapPath? value)
        {
            value = default;

            if (targetProperty is null)
                return false;

            var targetPropertyName = targetProperty.PropertyName;
            if (string.IsNullOrWhiteSpace(targetPropertyName))
                return false;

            return TryGetValue(targetPropertyName, out value);
        }

        /// <summary>
        /// Gets a value indicating whether the provided target property is found in the map.
        /// </summary>
        /// <param name="targetProperty"></param>
        /// <returns>Returns true if the the target property exists in the map.</returns>
        public bool ContainsKey(IPropertyProxy targetProperty) =>
            ContainsKey(targetProperty?.PropertyName ?? string.Empty);

        /// <summary>
        /// Removes the target property from the path lookup.
        /// </summary>
        /// <param name="targetProperty">The target property to remove.</param>
        /// <returns>True if the element was found a successfully removed.</returns>
        public bool Remove(IPropertyProxy targetProperty) =>
            Remove(targetProperty?.PropertyName ?? string.Empty);
    }
}
