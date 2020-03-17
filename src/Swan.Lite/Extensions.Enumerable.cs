using System.Collections.Generic;
using System.Linq;

namespace Swan
{
    /// <summary>
    /// This class contains extensions methods for types implementing IEnumerable<T>
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// This method returns the <see cref="System.Linq.Enumerable.Union"/>
        /// of all non-null parameters.
        /// </summary>
        /// <returns> An <see cref="IEnumerable{T}"/> that contains the elements from non-null input sequences, excluding duplicates. </returns>
        public static IEnumerable<T> UnionExcludingNulls<T>(this IEnumerable<T> @this, IEnumerable<T> second)
        {
            if (@this == null) return second ?? Enumerable.Empty<T>();
            return second == null ? @this : @this.Union(second);
        }
    }
}
