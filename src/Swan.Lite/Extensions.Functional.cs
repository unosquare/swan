using System;
using System.Collections.Generic;
using System.Linq;

namespace Swan
{
    /// <summary>
    /// Functional programming extension methods.
    /// </summary>
    public static class FunctionalExtensions
    {
        /// <summary>
        /// Whens the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of IQueryable.</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="fn">The function.</param>
        /// <returns>
        /// The IQueryable.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// this
        /// or
        /// condition
        /// or
        /// fn.
        /// </exception>
        public static IQueryable<T> When<T>(
            this IQueryable<T> list,
            Func<bool> condition,
            Func<IQueryable<T>, IQueryable<T>> fn)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (fn == null)
                throw new ArgumentNullException(nameof(fn));

            return condition() ? fn(list) : list;
        }

        /// <summary>
        /// Whens the specified condition.
        /// </summary>
        /// <typeparam name="T">The type of IEnumerable.</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="fn">The function.</param>
        /// <returns>
        /// The IEnumerable.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// this
        /// or
        /// condition
        /// or
        /// fn.
        /// </exception>
        public static IEnumerable<T> When<T>(
            this IEnumerable<T> list,
            Func<bool> condition,
            Func<IEnumerable<T>, IEnumerable<T>> fn)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (fn == null)
                throw new ArgumentNullException(nameof(fn));

            return condition() ? fn(list) : list;
        }

        /// <summary>
        /// Adds the value when the condition is true.
        /// </summary>
        /// <typeparam name="T">The type of IList element.</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The IList.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// this
        /// or
        /// condition
        /// or
        /// value.
        /// </exception>
        public static IList<T> AddWhen<T>(
            this IList<T> list,
            Func<bool> condition,
            Func<T> value)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (condition())
                list.Add(value());

            return list;
        }

        /// <summary>
        /// Adds the value when the condition is true.
        /// </summary>
        /// <typeparam name="T">The type of IList element.</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="condition">if set to <c>true</c> [condition].</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The IList.
        /// </returns>
        /// <exception cref="ArgumentNullException">list.</exception>
        public static IList<T> AddWhen<T>(
            this IList<T> list,
            bool condition,
            T value)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (condition)
                list.Add(value);

            return list;
        }

        /// <summary>
        /// Adds the range when the condition is true.
        /// </summary>
        /// <typeparam name="T">The type of List element.</typeparam>
        /// <param name="list">The list.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The List.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// this
        /// or
        /// condition
        /// or
        /// value.
        /// </exception>
        public static List<T> AddRangeWhen<T>(
            this List<T> list,
            Func<bool> condition,
            Func<IEnumerable<T>> value)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (condition == null)
                throw new ArgumentNullException(nameof(condition));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (condition())
                list.AddRange(value());

            return list;
        }
    }
}