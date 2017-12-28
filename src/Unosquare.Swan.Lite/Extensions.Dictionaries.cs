namespace Unosquare.Swan
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extension methods
    /// </summary>
    public partial class Extensions
    {
        /// <summary>
        /// Gets the value or default.
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>
        /// A dictionary of generic types
        /// </returns>
        /// <exception cref="ArgumentNullException">dict</exception>
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default(TValue))
        {
            if (dict == null)
                throw new ArgumentNullException(nameof(dict));

            return dict.ContainsKey(key) ? dict[key] : defaultValue;
        }

        /// <summary>
        /// Executes the item action for each element in the Dictionary
        /// </summary>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="itemAction">The item action.</param>
        /// <exception cref="ArgumentNullException">dict</exception>
        public static void ForEach<TKey, TValue>(this Dictionary<TKey, TValue> dict, Action<TKey, TValue> itemAction)
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