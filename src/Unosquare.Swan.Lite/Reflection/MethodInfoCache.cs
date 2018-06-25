namespace Unosquare.Swan.Reflection
{
    using System;
    using System.Reflection;
    using Components;

    /// <summary>
    /// Represents a Method Info Cache
    /// </summary>
    public class MethodInfoCache : CacheRepository<string, MethodInfo>
    {
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
        public MethodInfo Retrieve<T>(string name, string alias, params Type[] types)
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
        public MethodInfo Retrieve<T>(string name, params Type[] types)
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
        public MethodInfo Retrieve(Type type, string name, params Type[] types)
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
        public MethodInfo Retrieve(Type type, string name, string alias, params Type[] types)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (alias == null)
                throw new ArgumentNullException(nameof(alias));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (!Contains(alias))
                this[alias] = type.GetMethod(name, types ?? new Type[0]);

            return Retrieve(alias);
        }

        /// <summary>
        /// Retrieves the specified name.
        /// </summary>
        /// <typeparam name="T">The type of type</typeparam>
        /// <param name="name">The name.</param>
        /// <returns>
        /// The cached MethodInfo
        /// </returns>
        public MethodInfo Retrieve<T>(string name)
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
        public MethodInfo Retrieve(Type type, string name)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (!Contains(name))
                this[name] = type.GetMethod(name);

            return this[name];
        }
    }
}
