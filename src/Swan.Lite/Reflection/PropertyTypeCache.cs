using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swan.Reflection
{
    /// <summary>
    /// A thread-safe cache of properties belonging to a given type.
    /// </summary>
    public class PropertyTypeCache : TypeCache<PropertyInfo>
    {
        /// <summary>
        /// Gets the default cache.
        /// </summary>
        /// <value>
        /// The default cache.
        /// </value>
        public static Lazy<PropertyTypeCache> DefaultCache { get; } = new Lazy<PropertyTypeCache>(() => new PropertyTypeCache());

        /// <summary>
        /// Retrieves all properties.
        /// </summary>
        /// <typeparam name="T">The type to inspect.</typeparam>
        /// <param name="onlyPublic">if set to <c>true</c> [only public].</param>
        /// <returns>
        /// A collection with all the properties in the given type.
        /// </returns>
        public IEnumerable<PropertyInfo> RetrieveAllProperties<T>(bool onlyPublic = false)
            => Retrieve<T>(onlyPublic ? GetAllPublicPropertiesFunc() : GetAllPropertiesFunc());

        /// <summary>
        /// Retrieves all properties.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="onlyPublic">if set to <c>true</c> [only public].</param>
        /// <returns>
        /// A collection with all the properties in the given type.
        /// </returns>
        public IEnumerable<PropertyInfo> RetrieveAllProperties(Type type, bool onlyPublic = false)
            => Retrieve(type, onlyPublic ? GetAllPublicPropertiesFunc() : GetAllPropertiesFunc());

        /// <summary>
        /// Retrieves the filtered properties.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="onlyPublic">if set to <c>true</c> [only public].</param>
        /// <param name="filter">The filter.</param>
        /// <returns>
        /// A collection with all the properties in the given type.
        /// </returns>
        public IEnumerable<PropertyInfo> RetrieveFilteredProperties(
            Type type,
            bool onlyPublic,
            Func<PropertyInfo, bool> filter)
            => Retrieve(type,
                onlyPublic ? GetAllPublicPropertiesFunc(filter) : GetAllPropertiesFunc(filter));

        private static Func<Type, IEnumerable<PropertyInfo>> GetAllPropertiesFunc(
            Func<PropertyInfo, bool>? filter = null)
            => GetPropertiesFunc(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                filter);

        private static Func<Type, IEnumerable<PropertyInfo>> GetAllPublicPropertiesFunc(
            Func<PropertyInfo, bool>? filter = null)
            => GetPropertiesFunc(BindingFlags.Public | BindingFlags.Instance, filter);

        private static Func<Type, IEnumerable<PropertyInfo>> GetPropertiesFunc(BindingFlags flags,
            Func<PropertyInfo, bool>? filter = null)
            => t => t.GetProperties(flags)
                .Where(filter ?? (p => p.CanRead || p.CanWrite));
    }
}
