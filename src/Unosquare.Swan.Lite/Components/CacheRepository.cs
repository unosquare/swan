namespace Unosquare.Swan.Components
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// A thread-safe cache repository
    /// </summary>
    /// <typeparam name="TType">The type of parent class.</typeparam>
    /// <typeparam name="T">The type of object to cache.</typeparam>
    public class CacheRepository<TType, T>
    {
        private readonly ConcurrentDictionary<TType, T> _cache = new ConcurrentDictionary<TType, T>();

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
            get => _cache.ContainsKey(type) ? _cache[type] : default;
            set
            {
                if (value == null)
                    return;

                _cache.TryAdd(type, value);
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

            return _cache.ContainsKey(type);
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

            return _cache.TryGetValue(type, out var value) ? value : throw new KeyNotFoundException();
        }
    }
}