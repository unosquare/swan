﻿namespace Swan.Collections
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Extension methods.
    /// </summary>
    public static class DictionaryExtensions
    {
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
            if (dict is null)
                throw new ArgumentNullException(nameof(dict));

            if (itemAction is null)
                throw new ArgumentNullException(nameof(itemAction));

            foreach ((TKey key, TValue value) in dict)
            {
                itemAction?.Invoke(key, value);
            }
        }
    }
}