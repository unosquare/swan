namespace Unosquare.Swan.Reflection
{
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
        private readonly object SyncLock = new object();
        private readonly Dictionary<Type, PropertyInfo[]> Cache = new Dictionary<Type, PropertyInfo[]>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyTypeCache"/> class.
        /// </summary>
        public PropertyTypeCache()
        {
            // placeholder
        }

        /// <summary>
        /// Determines whether the cache contains the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        public bool Contains(Type type)
        {
            lock (SyncLock)
            {
                return this[type] != null;
            }
        }

        /// <summary>
        /// Determines whether the cache contains the specified type.
        /// </summary>
        public bool Contains<T>()
        {
            return Contains(typeof(T));
        }

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
            lock (SyncLock)
            {
                if (Contains(type)) return Cache[type];
                this[type] = factory.Invoke();
                return Cache[type];
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
        public PropertyInfo[] Retrieve<T>(Func<IEnumerable<PropertyInfo>> factory)
        {
            return Retrieve(typeof(T), factory);
        }

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
                lock (SyncLock)
                {
                    return Cache.ContainsKey(type) ? Cache[type] : null;
                }
            }
            set
            {
                lock (SyncLock)
                {
                    if (value == null)
                        return;

                    var propertyList = new List<PropertyInfo>();

                    foreach (var item in value)
                    {
                        if (item != null)
                            propertyList.Add(item);
                    }

                    Cache[type] = propertyList.ToArray();
                }
            }
        }
    }
}
