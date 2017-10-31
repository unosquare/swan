namespace Unosquare.Swan
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Functional programming extension methods
    /// </summary>
    public static class FunctionalExtensions
    {
        /// <summary>
        /// Whens the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of IQueryable</typeparam>
        /// <param name="this">The this.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="fn">The function.</param>
        /// <returns>
        /// The IQueryable
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// this
        /// or
        /// condition
        /// or
        /// fn
        /// </exception>
        public static IQueryable<T> When<T>(
            this IQueryable<T> @this,
            Func<bool> condition,
            Func<IQueryable<T>, IQueryable<T>> fn)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (fn == null)
                throw new ArgumentNullException(nameof(fn));

            return condition() ? fn(@this) : @this;
        }

        /// <summary>
        /// Whens the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of IEnumerable</typeparam>
        /// <param name="this">The this.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="fn">The function.</param>
        /// <returns>
        /// The IEnumerable
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// this
        /// or
        /// condition
        /// or
        /// fn
        /// </exception>
        public static IEnumerable<T> When<T>(
            this IEnumerable<T> @this,
            Func<bool> condition,
            Func<IEnumerable<T>, IEnumerable<T>> fn)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (fn == null)
                throw new ArgumentNullException(nameof(fn));

            return condition() ? fn(@this) : @this;
        }

        /// <summary>
        /// Adds the value when the condition is true.
        /// </summary>
        /// <typeparam name="T">The type of IList element</typeparam>
        /// <param name="this">The this.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The IList
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// this
        /// or
        /// condition
        /// or
        /// value
        /// </exception>
        public static IList<T> AddWhen<T>(
            this IList<T> @this,
            Func<bool> condition,
            Func<T> value)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

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
        /// <returns>
        /// The IList
        /// </returns>
        /// <exception cref="ArgumentNullException">this</exception>
        public static IList<T> AddWhen<T>(
            this IList<T> @this,
            bool condition,
            T value)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

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
        /// <returns>
        /// The List
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// this
        /// or
        /// condition
        /// or
        /// value
        /// </exception>
        public static List<T> AddRangeWhen<T>(
            this List<T> @this,
            Func<bool> condition,
            Func<IEnumerable<T>> value)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (condition())
                @this.AddRange(value());

            return @this;
        }
    }
}