namespace Unosquare.Swan.Reflection
{
    using System.Linq;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Components;

    /// <summary>
    /// A thread-safe cache of members belonging to a given type
    /// The Retrieve method is the most useful one in this class as it
    /// calls the retrieval process if the type is not contained
    /// in the cache.
    /// </summary>
    /// <typeparam name="T">The type of Member to be cached</typeparam>
    public abstract class TypeCache<T> : CacheRepository<Type, T>
        where T : MemberInfo
    {
        /// <summary>
        /// Determines whether the cache contains the specified type.
        /// </summary>
        /// <typeparam name="TOut">The type of the out.</typeparam>
        /// <returns>
        ///   <c>true</c> if [contains]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains<TOut>() => Contains(typeof(TOut));

        /// <summary>
        /// Retrieves the properties stored for the specified type.
        /// If the properties are not available, it calls the factory method to retrieve them
        /// and returns them as an array of PropertyInfo
        /// </summary>
        /// <typeparam name="TOut">The type of the out.</typeparam>
        /// <param name="factory">The factory.</param>
        /// <returns>An array of the properties stored for the specified type</returns>
        public T[] Retrieve<TOut>(Func<IEnumerable<T>> factory) => Retrieve(typeof(TOut), factory);
    }

    /// <summary>
    /// A thread-safe cache of properties belonging to a given type
    /// The Retrieve method is the most useful one in this class as it
    /// calls the retrieval process if the type is not contained
    /// in the cache.
    /// </summary>
    public class PropertyTypeCache : TypeCache<PropertyInfo>
    {
        /// <summary>
        /// Gets all properties function.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A function to retrieve all properties</returns>
        public static Func<IEnumerable<PropertyInfo>> GetAllPropertiesFunc(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return () => type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => p.CanRead || p.CanWrite)
                .ToArray();
        }

        /// <summary>
        /// Gets all public properties function.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A function to retrieve all public properties</returns>
        public static Func<IEnumerable<PropertyInfo>> GetAllPublicPropertiesFunc(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return () => type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead || p.CanWrite)
                .ToArray();
        }
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
        /// Gets all fields function.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>A function to retrieve all fields</returns>
        public static Func<IEnumerable<FieldInfo>> GetAllFieldsFunc(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return () => type.GetFields(BindingFlags.Public | BindingFlags.Instance).ToArray();
        }
    }
}