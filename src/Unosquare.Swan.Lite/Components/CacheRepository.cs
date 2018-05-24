namespace Unosquare.Swan.Components
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A thread-safe cache repository
    /// </summary>
    /// <typeparam name="TType">The type of parent class.</typeparam>
    /// <typeparam name="T">The type of object to cache.</typeparam>
    public class CacheRepository<TType, T>
    {
        private readonly object _syncLock = new object();
        private readonly Dictionary<TType, T> _cache = new Dictionary<TType, T>();

        /// <summary>
        /// Gets or sets the <see cref="T"/> with the specified type.
        /// </summary>
        /// <value>
        /// The value of the cache.
        /// </value>
        /// <param name="type">The type.</param>
        /// <returns>The value of the cache</returns>
        public T this[TType type]
        {
            get
            {
                lock (_syncLock)
                {
                    return _cache.ContainsKey(type) ? _cache[type] : default;
                }
            }
            set
            {
                lock (_syncLock)
                {
                    if (value == null)
                        return;

                    _cache[type] = value;
                }
            }
        }

        /// <summary>
        /// Determines whether the cache contains the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(TType type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            lock (_syncLock)
            {
                return this[type] != null;
            }
        }

        /// <summary>
        /// Retrieves the properties stored for the specified type.
        /// If the properties are not available, it calls the factory method to retrieve them
        /// and returns them as an array of PropertyInfo
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// An array of the properties stored for the specified type
        /// </returns>
        /// <exception cref="System.ArgumentNullException">type</exception>
        public T Retrieve(TType type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            lock (_syncLock)
            {
                return Contains(type) ? _cache[type] : throw new KeyNotFoundException();
            }
        }
    }
}