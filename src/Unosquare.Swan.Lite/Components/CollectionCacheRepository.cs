namespace Unosquare.Swan.Components
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A thread-safe collection cache repository.
    /// </summary>
    /// <typeparam name="TType">The type of parent class.</typeparam>
    /// <typeparam name="T">The type of member to cache.</typeparam>
    public class CollectionCacheRepository<TType, T> : ConcurrentDictionary<TType, T[]>
        where TType : class
    {
        /// <summary>
        /// Gets or sets the <see cref="IEnumerable{T}"/> with the specified type.
        /// </summary>
        /// <value>
        /// The <see cref="IEnumerable{T}"/>.
        /// </value>
        /// <param name="type">The type.</param>
        /// <returns>The cache of the type.</returns>
        public new IEnumerable<T> this[TType type]
        {
            get => Contains(type) && TryGetValue(type, out var value) ? value : default;
            private set
            {
                if (value == null)
                    return;

                TryAdd(type, value.ToArray());
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

            return ContainsKey(type);
        }

        /// <summary>
        /// Retrieves the properties stored for the specified type.
        /// If the properties are not available, it calls the factory method to retrieve them
        /// and returns them as an array of PropertyInfo.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="factory">The factory.</param>
        /// <returns>
        /// An array of the properties stored for the specified type.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">type.</exception>
        public T[] Retrieve(TType type, Func<IEnumerable<T>> factory)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            if (TryGetValue(type, out var value)) return value;

            var factoryValue = factory.Invoke().Where(item => item != null);
            this[type] = factoryValue;

            return factoryValue.ToArray();
        }
    }
}
