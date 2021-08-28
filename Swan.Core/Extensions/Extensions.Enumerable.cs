using System.Collections.Generic;
using System.Linq;

namespace Swan
{
    /// <summary>
    /// This class contains extension methods for types implementing IEnumerable&lt;TSource&gt;
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// This method produces a union of two IEnumerables
        /// validation when some of them is null.
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="this">The this.</param>
        /// <param name="second">The second.</param>
        /// <returns> The Union </returns>
        public static IEnumerable<T>? UnionNull<T>(this IEnumerable<T>? @this, IEnumerable<T>? second)
        {
            if (@this == null) return second;
            return second == null ? @this : @this.Union(second);
        }

        /// <summary>
        /// This method returns the <see cref="Enumerable.Union{TSource}">Union</see>
        /// of all non-null parameters.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <param name="this">An IEnumerable&lt;TSource&gt; whose distinct elements forms the first set of the union.</param>
        /// <param name="second">An IEnumerable&lt;TSource&gt; whose distinct elements forms the second set of the union.</param>
        /// <returns>
        /// An <see cref="IEnumerable{TSource}" /> that contains the elements from non-null input sequences, excluding duplicates.
        /// </returns>
        public static IEnumerable<TSource> UnionExcludingNulls<TSource>(this IEnumerable<TSource>? @this, IEnumerable<TSource>? second)
            => (@this ?? Enumerable.Empty<TSource>()).Union(second ?? Enumerable.Empty<TSource>());

        /// <summary>
        /// This method returns the <see cref="Enumerable.Union{TSource}">Union</see>
        /// of all non-null parameters.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <param name="this">An IEnumerable&lt;TSource&gt; whose distinct elements forms the first set of the union.</param>
        /// <param name="second">An IEnumerable&lt;TSource&gt; whose distinct elements forms the second set of the union.</param>
        /// <param name="comparer">The IEqualityComparer&lt;TSource&gt; to compare values.</param>
        /// <returns>
        /// An <see cref="IEnumerable{TSource}" /> that contains the elements from non-null input sequences, excluding duplicates.
        /// </returns>
        public static IEnumerable<TSource> UnionExcludingNulls<TSource>(this IEnumerable<TSource>? @this, IEnumerable<TSource>? second, IEqualityComparer<TSource> comparer)
            => (@this ?? Enumerable.Empty<TSource>()).Union(second ?? Enumerable.Empty<TSource>(),
                comparer);
    }
}