namespace Unosquare.Swan.Reflection
{
    using System.Linq;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// A thread-safe cache of properties belonging to a given type
    /// The Retrieve method is the most useful one in this class as it
    /// calls the retrieval process if the type is not contained
    /// in the cache.
    /// </summary>
    public class PropertyTypeCache
    {
        private readonly object _syncLock = new object();
        private readonly Dictionary<Type, PropertyInfo[]> _cache = new Dictionary<Type, PropertyInfo[]>();

        /// <summary>
        /// Determines whether the cache contains the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        public bool Contains(Type type)
        {
            lock (_syncLock)
            {
                return this[type] != null;
            }
        }

        /// <summary>
        /// Determines whether the cache contains the specified type.
        /// </summary>
        public bool Contains<T>() => Contains(typeof(T));

        /// <summary>
        /// Retrieves the properties stored for the specified type.
        /// If the properties are not available, it calls the factory method to retrieve them
        /// and returns them as an array of PropertyInfo
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="factory">The factory.</param>
        /// <returns></returns>
        public PropertyInfo[] Retrieve(Type type, Func<IEnumerable<PropertyInfo>> factory)
        {
            lock (_syncLock)
            {
                if (Contains(type)) return _cache[type];
                this[type] = factory.Invoke();
                return _cache[type];
            }
        }

        /// <summary>
        /// Retrieves the properties stored for the specified type.
        /// If the properties are not available, it calls the factory method to retrieve them
        /// and returns them as an array of PropertyInfo
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="factory">The factory.</param>
        /// <returns></returns>
        public PropertyInfo[] Retrieve<T>(Func<IEnumerable<PropertyInfo>> factory) => Retrieve(typeof(T), factory);

        /// <summary>
        /// Gets or sets the <see cref="IEnumerable{PropertyInfo}"/> with the specified type.
        /// If the properties are not available, it returns null.
        /// </summary>
        /// <value>
        /// The <see cref="IEnumerable{PropertyInfo}"/>.
        /// </value>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public IEnumerable<PropertyInfo> this[Type type]
        {
            get
            {
                lock (_syncLock)
                {
                    return _cache.ContainsKey(type) ? _cache[type] : null;
                }
            }
            set
            {
                lock (_syncLock)
                {
                    if (value == null)
                        return;

                    _cache[type] = value.Where(item => item != null).ToArray();
                }
            }
        }
    }
}