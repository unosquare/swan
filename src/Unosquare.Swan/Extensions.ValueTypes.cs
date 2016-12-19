namespace Unosquare.Swan
{
    using System;
    using System.Collections;
    using System.Reflection;

    /// <summary>
    /// Provides various extension methods
    /// </summary>
    partial class Extensions
    {
        /// <summary>
        /// Clamps the specified value between the minimum and the maximum
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns></returns>
        public static T Clamp<T>(this T value, T min, T max)
            where T : struct, IComparable
        {
            if (value.CompareTo(min) < 0) return min;

            return value.CompareTo(max) > 0 ? max : value;
        }

        /// <summary>
        /// Determines whether the specified value is between a minimum and a maximum value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <param name="min">The minimum.</param>
        /// <param name="max">The maximum.</param>
        /// <returns>
        ///   <c>true</c> if the specified minimum is between; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsBetween<T>(this T value, T min, T max)
            where T : struct, IComparable
        {
            if (value.CompareTo(min) < 0 || value.CompareTo(max) > 0)
                return false;

            return true;
        }

        /// <summary>
        /// Determines whether this instance is collection.
        /// </summary>
        /// <param name="prop">The property.</param>
        /// <returns>
        ///   <c>true</c> if the specified property is collection; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsCollection(this PropertyInfo prop)
        {
            return prop.PropertyType != typeof(string) &&
                             typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(prop.PropertyType);
        }
    }
}
