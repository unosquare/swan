namespace Unosquare.Swan
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Functional programming extension methods
    /// </summary>
    public static partial class Extensions
    {
        /// <summary>
        /// Whens the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of IQueryable</typeparam>
        /// <param name="this">The this.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="fn">The function.</param>
        /// <returns>The IQueryable</returns>
        public static IQueryable<T> When<T>(
            this IQueryable<T> @this,
            Func<bool> condition,
            Func<IQueryable<T>, IQueryable<T>> fn) =>
            condition() ? fn(@this) : @this;

        /// <summary>
        /// Whens the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of IEnumerable</typeparam>
        /// <param name="this">The this.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="fn">The function.</param>
        /// <returns>The IEnumerable</returns>
        public static IEnumerable<T> When<T>(
            this IEnumerable<T> @this,
            Func<bool> condition,
            Func<IEnumerable<T>, IEnumerable<T>> fn) =>
            condition() ? fn(@this) : @this;

        /// <summary>
        /// Adds the value when the condition is true.
        /// </summary>
        /// <typeparam name="T">The type of IList element</typeparam>
        /// <param name="this">The this.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="value">The value.</param>
        /// <returns>The IList</returns>
        public static IList<T> AddWhen<T>(
            this IList<T> @this,
            Func<bool> condition,
            Func<T> value)
        {
            if (condition())
                @this.Add(value());

            return @this;
        }

        /// <summary>
        /// Adds the value when the condition is true.
        /// </summary>
        /// <typeparam name="T">The type of IList element</typeparam>
        /// <param name="this">The this.</param>
        /// <param name="condition">if set to <c>true</c> [condition].</param>
        /// <param name="value">The value.</param>
        /// <returns>The IList</returns>
        public static IList<T> AddWhen<T>(
            this IList<T> @this,
            bool condition,
            T value)
        {
            if (condition)
                @this.Add(value);

            return @this;
        }

        /// <summary>
        /// Adds the range when the condition is true.
        /// </summary>
        /// <typeparam name="T">The type of List element</typeparam>
        /// <param name="this">The this.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="value">The value.</param>
        /// <returns>The List</returns>
        public static List<T> AddRangeWhen<T>(
            this List<T> @this,
            Func<bool> condition,
            Func<IEnumerable<T>> value)
        {
            if (condition())
                @this.AddRange(value());

            return @this;
        }
    }
}