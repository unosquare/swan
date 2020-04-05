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
        /// <returns> An <see cref="IEnumerable{TSource}"/> that contains the elements from non-null input sequences, excluding duplicates. </returns>
        public static IEnumerable<TSource> UnionExcludingNulls<TSource>(this IEnumerable<TSource> @this, IEnumerable<TSource> second)
        {
            if (@this == null) return second ?? Enumerable.Empty<TSource>();
            return second == null ? @this : @this.Union(second);
        }
    }
}
