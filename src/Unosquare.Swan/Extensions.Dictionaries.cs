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
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>A dictionary of generic types</returns>
        public static T GetValueOrDefault<T>(this Dictionary<T, T> dict, T key, T defaultValue = default(T))
        {
            return dict.ContainsKey(key) ? dict[key] : defaultValue;
        }

        /// <summary>
        /// Executes the item action for each element in the Dictionary
        /// </summary>
        /// <typeparam name="T">Generic type</typeparam>
        /// <param name="dict">The dictionary.</param>
        /// <param name="itemAction">The item action.</param>
        public static void ForEach<T>(this Dictionary<T, T> dict, Action<T, T> itemAction)
        {
            foreach (var kvp in dict)
            {
                itemAction(kvp.Key, kvp.Value);
            }
        }
    }
}
