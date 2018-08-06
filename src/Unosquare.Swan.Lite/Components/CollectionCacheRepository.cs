namespace Unosquare.Swan.Components
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A thread-safe collection cache repository
    /// </summary>
    /// <typeparam name="TType">The type of parent class.</typeparam>
    /// <typeparam name="T">The type of member to cache.</typeparam>
    public class CollectionCacheRepository<TType, T>
        where TType : class
    {
        private readonly ConcurrentDictionary<TType, T[]> _cache = new ConcurrentDictionary<TType, T[]>();

        /// <summary>
        /// Gets or sets the <see cref="IEnumerable{T}"/> with the specified type.
        /// </summary>
        /// <value>
        /// The <see cref="IEnumerable{T}"/>.
        /// </value>
        /// <param name="type">The type.</param>
        /// <returns>The cache of the type</returns>
        public IEnumerable<T> this[TType type]
        {
            get => _cache.ContainsKey(type) ? _cache[type] : null;
            private set
            {
                if (value == null)
                    return;

                _cache.TryAdd(type, value.Where(item => item != null).ToArray());
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
            if (Equals(default(TType), type))
                throw new ArgumentNullException(nameof(type));

            return _cache.ContainsKey(type);
        }

        /// <summary>
        /// Retrieves the properties stored for the specified type.
        /// If the properties are not available, it calls the factory method to retrieve them
        /// and returns them as an array of PropertyInfo
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="factory">The factory.</param>
        /// <returns>
        /// An array of the properties stored for the specified type
        /// </returns>
        /// <exception cref="System.ArgumentNullException">type</exception>
        public T[] Retrieve(TType type, Func<IEnumerable<T>> factory)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            if (_cache.TryGetValue(type, out var value)) return value;

            var factoryValue = factory.Invoke();
            this[type] = factoryValue;

            return factoryValue.Where(item => item != null).ToArray();
        }
    }
}
