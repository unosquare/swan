namespace Unosquare.Swan.Reflection
{
    using System.Linq;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Components;

    /// <summary>
    /// A thread-safe cache of members belonging to a given type.
    /// 
    /// The Retrieve method is the most useful one in this class as it
    /// calls the retrieval process if the type is not contained
    /// in the cache.
    /// </summary>
    /// <typeparam name="T">The type of Member to be cached.</typeparam>
    public abstract class TypeCache<T> : CollectionCacheRepository<T>
        where T : MemberInfo
    {
        /// <summary>
        /// Determines whether the cache contains the specified type.
        /// </summary>
        /// <typeparam name="TOut">The type of the out.</typeparam>
        /// <returns>
        ///   <c>true</c> if [contains]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains<TOut>() => ContainsKey(typeof(TOut));

        /// <summary>
        /// Retrieves the properties stored for the specified type.
        /// If the properties are not available, it calls the factory method to retrieve them
        /// and returns them as an array of PropertyInfo.
        /// </summary>
        /// <typeparam name="TOut">The type of the out.</typeparam>
        /// <param name="factory">The factory.</param>
        /// <returns>An array of the properties stored for the specified type.</returns>
        public IEnumerable<T> Retrieve<TOut>(Func<Type, IEnumerable<T>> factory)
            => Retrieve(typeof(TOut), factory);
    }

    /// <summary>
    /// A thread-safe cache of properties belonging to a given type.
    /// 
    /// The Retrieve method is the most useful one in this class as it
    /// calls the retrieval process if the type is not contained
    /// in the cache.
    /// </summary>
    public class PropertyTypeCache : TypeCache<PropertyInfo>
    {
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
            Func<PropertyInfo, bool> filter = null)
            => GetPropertiesFunc(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                filter);

        private static Func<Type, IEnumerable<PropertyInfo>> GetAllPublicPropertiesFunc(
            Func<PropertyInfo, bool> filter = null)
            => GetPropertiesFunc(BindingFlags.Public | BindingFlags.Instance, filter);

        private static Func<Type, IEnumerable<PropertyInfo>> GetPropertiesFunc(BindingFlags flags,
            Func<PropertyInfo, bool> filter = null)
            => t => t.GetProperties(flags)
                .Where(filter ?? (p => p.CanRead || p.CanWrite));
    }

    /// <summary>
    /// A thread-safe cache of fields belonging to a given type
    /// The Retrieve method is the most useful one in this class as it
    /// calls the retrieval process if the type is not contained
    /// in the cache.
    /// </summary>
    public class FieldTypeCache : TypeCache<FieldInfo>
    {
        /// <summary>
        /// Retrieves all fields.
        /// </summary>
        /// <typeparam name="T">The type to inspect.</typeparam>
        /// <returns>
        /// A collection with all the fields in the given type.
        /// </returns>
        public IEnumerable<FieldInfo> RetrieveAllFields<T>()
            => Retrieve<T>(GetAllFieldsFunc());

        /// <summary>
        /// Retrieves all fields.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// A collection with all the fields in the given type.
        /// </returns>
        public IEnumerable<FieldInfo> RetrieveAllFields(Type type) 
            => Retrieve(type, GetAllFieldsFunc());

        private static Func<Type, IEnumerable<FieldInfo>> GetAllFieldsFunc()
            => t => t.GetFields(BindingFlags.Public | BindingFlags.Instance);
    }
}