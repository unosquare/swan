using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swan.Reflection
{
    /// <summary>
    /// A thread-safe cache of properties belonging to a given type.
    /// </summary>
    public class PropertyTypeCache : TypeCache<IPropertyProxy>
    {
        /// <summary>
        /// Gets the default cache.
        /// </summary>
        /// <value>
        /// The default cache.
        /// </value>
        public static Lazy<PropertyTypeCache> DefaultCache { get; } = new(() => new PropertyTypeCache());

        /// <summary>
        /// Retrieves all properties.
        /// </summary>
        /// <typeparam name="T">The type to inspect.</typeparam>
        /// <param name="onlyPublic">if set to <c>true</c> [only public].</param>
        /// <returns>
        /// A collection with all the properties in the given type.
        /// </returns>
        public IEnumerable<IPropertyProxy> RetrieveAllProperties<T>(bool onlyPublic = false)
            => onlyPublic
            ? Retrieve<T>(GetPropertiesFunc()).Where(c => c.HasPublicGetter)
            : Retrieve<T>(GetPropertiesFunc());

        /// <summary>
        /// Retrieves all properties.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="onlyPublic">if set to <c>true</c> [only public].</param>
        /// <returns>
        /// A collection with all the properties in the given type.
        /// </returns>
        public IEnumerable<IPropertyProxy> RetrieveAllProperties(Type type, bool onlyPublic = false)
            => onlyPublic
            ? Retrieve(type, GetPropertiesFunc()).Where(c => c.HasPublicGetter)
            : Retrieve(type, GetPropertiesFunc());

        /// <summary>
        /// Retrieves the filtered properties.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="onlyPublic">if set to <c>true</c> [only public].</param>
        /// <param name="filter">The filter.</param>
        /// <returns>
        /// A collection with all the properties in the given type.
        /// </returns>
        public IEnumerable<IPropertyProxy> RetrieveFilteredProperties(
            Type type,
            bool onlyPublic,
            Func<IPropertyProxy, bool> filter)
            => RetrieveAllProperties(type, onlyPublic).Where(c => filter(c));

        private static Func<Type, IEnumerable<IPropertyProxy>> GetPropertiesFunc()
        {
            return (t) =>
            {
                var properties = t.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                var result = new List<IPropertyProxy>(properties.Length);
                foreach (var propertyInfo in properties)
                    result.Add(new PropertyProxy(t, propertyInfo));

                return result;
            };
        }
    }
}