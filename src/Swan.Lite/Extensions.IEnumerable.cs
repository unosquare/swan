using System.Collections.Generic;
using System.Linq;

namespace Swan
{
    /// <summary>
    /// This class contains extensions methods for IEnumerable
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// This method make an union of two IEnumerables
        /// validation when some of them is null.
        /// </summary>
        /// <typeparam name="T">T</typeparam>
        /// <param name="this">The this.</param>
        /// <param name="second">The second.</param>
        /// <returns> The Union </returns>
        public static IEnumerable<T> UnionNull<T>(this IEnumerable<T> @this, IEnumerable<T> second)
        {
            if (@this == null) return second;
            return second == null ? @this : @this.Union(second);
        }
    }
}
