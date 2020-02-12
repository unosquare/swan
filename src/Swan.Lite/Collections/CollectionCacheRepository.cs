using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Swan.Collections
{
    /// <summary>
    /// A thread-safe collection cache repository for types.
    /// </summary>
    /// <typeparam name="TValue">The type of member to cache.</typeparam>
    public class CollectionCacheRepository<TValue>
    {
        private readonly Lazy<ConcurrentDictionary<Type, IEnumerable<TValue>>> _data =
            new Lazy<ConcurrentDictionary<Type, IEnumerable<TValue>>>(() =>
                new ConcurrentDictionary<Type, IEnumerable<TValue>>(), true);

        /// <summary>
        /// Determines whether the cache contains the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><c>true</c> if the cache contains the key, otherwise <c>false</c>.</returns>
        public bool ContainsKey(Type key) => _data.Value.ContainsKey(key);

        /// <summary>
        /// Retrieves the properties stored for the specified type.
        /// If the properties are not available, it calls the factory method to retrieve them
        /// and returns them as an array of PropertyInfo.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="factory">The factory.</param>
        /// <returns>
        /// An array of the properties stored for the specified type.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// key
        /// or
        /// factory.
        /// </exception>
        public IEnumerable<TValue> Retrieve(Type key, Func<Type, IEnumerable<TValue>> factory)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            return _data.Value.GetOrAdd(key, k => factory.Invoke(k).Where(item => item != null));
        }
    }
}
