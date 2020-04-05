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
        /// This method returns the <see cref="Enumerable.Union{TSource}">Union</see>
        /// of all non-null parameters.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <param name="this">An IEnumerable&lt;TSource&gt; whose distinct elements forms the first set of the union.</param>
        /// <param name="second">An IEnumerable&lt;TSource&gt; whose distinct elements forms the second set of the union.</param>
        /// <returns>
        /// An <see cref="IEnumerable{TSource}" /> that contains the elements from non-null input sequences, excluding duplicates.
        /// </returns>
        public static IEnumerable<TSource> UnionExcludingNulls<TSource>(this IEnumerable<TSource> @this, IEnumerable<TSource> second)
            => Enumerable.Union(
                @this ?? Enumerable.Empty<TSource>(),
                second ?? Enumerable.Empty<TSource>());

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
        public static IEnumerable<TSource> UnionExcludingNulls<TSource>(this IEnumerable<TSource> @this, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
            => Enumerable.Union(
                @this ?? Enumerable.Empty<TSource>(),
                second ?? Enumerable.Empty<TSource>(),
                comparer);
    }
}
