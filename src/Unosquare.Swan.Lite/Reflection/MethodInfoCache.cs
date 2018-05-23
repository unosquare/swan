namespace Unosquare.Swan.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Represents a Method Info Cache
    /// </summary>
    public static class MethodInfoCache
    {
        private static readonly object SyncLock = new object();
        private static readonly Dictionary<string, MethodInfo> Cache = new Dictionary<string, MethodInfo>();

        /// <summary>
        /// Determines whether the cache contains the specified type.
        /// </summary>
        /// <param name="name">The type.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            lock (SyncLock)
            {
                return Cache.ContainsKey(name);
            }
        }

        /// <summary>
        /// Retrieves the properties stored for the specified type.
        /// If the properties are not available, it calls the factory method to retrieve them
        /// and returns them as an array of PropertyInfo
        /// </summary>
        /// <typeparam name="T">The type of type</typeparam>
        /// <param name="name">The name.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="types">The types.</param>
        /// <returns>
        /// The cached MethodInfo
        /// </returns>
        /// <exception cref="ArgumentNullException">name
        /// or
        /// factory</exception>
        /// <exception cref="System.ArgumentNullException">type</exception>
        public static MethodInfo Retrieve<T>(string name, string alias, params Type[] types)
            => Retrieve(typeof(T), name, alias, types);

        /// <summary>
        /// Retrieves the specified name.
        /// </summary>
        /// <typeparam name="T">The type of type</typeparam>
        /// <param name="name">The name.</param>
        /// <param name="types">The types.</param>
        /// <returns>
        /// The cached MethodInfo
        /// </returns>
        public static MethodInfo Retrieve<T>(string name, params Type[] types)
            => Retrieve(typeof(T), name, name, types);

        /// <summary>
        /// Retrieves the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <param name="types">The types.</param>
        /// <returns>
        /// An array of the properties stored for the specified type
        /// </returns>
        public static MethodInfo Retrieve(Type type, string name, params Type[] types)
            => Retrieve(type, name, name, types);

        /// <summary>
        /// Retrieves the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <param name="alias">The alias.</param>
        /// <param name="types">The types.</param>
        /// <returns>
        /// The cached MethodInfo
        /// </returns>
        public static MethodInfo Retrieve(Type type, string name, string alias, params Type[] types)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (alias == null)
                throw new ArgumentNullException(nameof(alias));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            lock (SyncLock)
            {
                if (!Contains(alias))
                    Cache[alias] = type.GetMethod(name, types ?? new Type[0]);

                return Cache[alias];
            }
        }

        /// <summary>
        /// Retrieves the specified name.
        /// </summary>
        /// <typeparam name="T">The type of type</typeparam>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The cached MethodInfo
        /// </returns>
        public static MethodInfo Retrieve<T>(string name)
            => Retrieve(typeof(T), name);

        /// <summary>
        /// Retrieves the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The cached MethodInfo
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// type
        /// or
        /// name
        /// </exception>
        public static MethodInfo Retrieve(Type type, string name)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            lock (SyncLock)
            {
                if (!Contains(name))
                    Cache[name] = type.GetMethod(name);

                return Cache[name];
            }
        }
    }
}
