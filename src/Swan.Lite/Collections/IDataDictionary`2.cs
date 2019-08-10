using System;
using System.Collections.Generic;

namespace Swan.Collections
{
    /// <summary>
    /// Represents a generic collection of key/value pairs that does not store
    /// null values.
    /// </summary>
    /// <typeparam name="TKey">The type of keys in the dictionary. This must be a reference type.</typeparam>
    /// <typeparam name="TValue">The type of values in the dictionary. This must be a reference type.</typeparam>
    public interface IDataDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
        where TKey : class
        where TValue : class
    {
        /// <summary>
        /// Gets a value that indicates whether the <see cref="IDataDictionary{TKey,TValue}"/> is empty.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the <see cref="IDataDictionary{TKey,TValue}"/> is empty; otherwise, <see langword="false"/>.
        /// </value>
        bool IsEmpty { get; }

        /// <summary>
        /// Attempts to remove and return the value that has the specified key from the <see cref="IDataDictionary{TKey,TValue}"/>.
        /// </summary>
        /// <param name="key">The key of the element to remove and return.</param>
        /// <param name="value">When this method returns, the value removed from the <see cref="IDataDictionary{TKey,TValue}"/>,
        /// if the key is found; otherwise, <see langword="null"/>. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the value was removed successfully; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        bool TryRemove(TKey key, out TValue value);
    }
}
