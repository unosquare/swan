namespace Unosquare.Swan.Components
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <inheritdoc />
    /// <summary>
    /// A thread-safe abstract cache repository.
    /// </summary>
    /// <typeparam name="TType">The type of parent class.</typeparam>
    /// <typeparam name="T">The type of object to cache.</typeparam>
    public abstract class CacheRepository<TType, T> : ConcurrentDictionary<TType, T>
        where TType : class
    {
        /// <summary>
        /// Gets or sets the value with the specified type.
        /// </summary>
        /// <value>
        /// The value of the cache.
        /// </value>
        /// <param name="type">The type.</param>
        /// <returns>
        /// The value in the cache repository.
        /// </returns>
        public new T this[TType type]
        {
            get => Contains(type) && TryGetValue(type, out var value) ? value : default;
            set
            {
                if (value == null)
                    return;

                TryAdd(type, value);
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

            return ContainsKey(type);
        }
        
        /// <summary>
        /// Retrieves the element stored for the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// An object for the specified type.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">type.</exception>
        public virtual T Retrieve(TType type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return TryGetValue(type, out var value) ? value : throw new KeyNotFoundException();
        }
    }
}