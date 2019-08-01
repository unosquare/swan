using System;
using System.Collections.Generic;

namespace Swan
{
    /// <summary>
    /// Extension methods.
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Gets the value if exists or default.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// The value of the provided key or default.
        /// </returns>
        /// <exception cref="ArgumentNullException">dict.</exception>
        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
        {
            if (dict == null)
                throw new ArgumentNullException(nameof(dict));

            return dict.ContainsKey(key) ? dict[key] : defaultValue;
        }

        /// <summary>
        /// Adds a key/value pair to the Dictionary if the key does not already exist.
        /// If the value is null, the key will not be updated.
        /// Based on <c>ConcurrentDictionary.GetOrAdd</c> method.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns>
        /// The value for the key.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// dict
        /// or
        /// valueFactory.
        /// </exception>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TKey, TValue> valueFactory)
        {
            if (dict == null)
                throw new ArgumentNullException(nameof(dict));

            if (valueFactory == null)
                throw new ArgumentNullException(nameof(valueFactory));

            if (!dict.ContainsKey(key))
            {
                var value = valueFactory(key);
                if (Equals(value, default)) return default;
                dict[key] = value;
            }

            return dict[key];
        }

        /// <summary>
        /// Executes the item action for each element in the Dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="itemAction">The item action.</param>
        /// <exception cref="ArgumentNullException">dict.</exception>
        public static void ForEach<TKey, TValue>(this IDictionary<TKey, TValue> dict, Action<TKey, TValue> itemAction)
        {
            if (dict == null)
                throw new ArgumentNullException(nameof(dict));

            foreach (var kvp in dict)
            {
                itemAction(kvp.Key, kvp.Value);
            }
        }
    }
}