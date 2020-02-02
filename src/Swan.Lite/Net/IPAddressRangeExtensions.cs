using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;

namespace Swan.Net
{
    /// <summary>
    /// Provides extension methods for instances and collections of <see cref="IPAddressRange"/>.
    /// </summary>
    public static class IPAddressRangeExtensions
    {
        /// <summary>
        /// Determines whether any element of a sequence of <see cref="IPAddressRange"/> instances
        /// contains the given <paramref name="address"/>.
        /// </summary>
        /// <param name="this">The <see cref="IEnumerable{T}">IEnumerable&lt;IPAddressRange&gt;</see> interface
        /// on which this method is called.</param>
        /// <param name="address">The <see cref="IPAddress"/> to look for.</param>
        /// <returns><see langword="true"/> if any of the ranges in <paramref name="this"/>
        /// contains <paramref name="address"/>; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="this"/> is <see langword="null"/>.</exception>
        public static bool AnyContains(this IEnumerable<IPAddressRange> @this, IPAddress address)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            foreach (var range in @this)
            {
                if (range.Contains(address))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether no element of a sequence of <see cref="IPAddressRange"/> instances
        /// contains the given <paramref name="address"/>.
        /// </summary>
        /// <param name="this">The <see cref="IEnumerable{T}">IEnumerable&lt;IPAddressRange&gt;</see> interface
        /// on which this method is called.</param>
        /// <param name="address">The <see cref="IPAddress"/> to look for.</param>
        /// <returns><see langword="true"/> if none of the ranges in <paramref name="this"/>
        /// contains <paramref name="address"/>; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="this"/> is <see langword="null"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NoneContains(this IEnumerable<IPAddressRange> @this, IPAddress address)
            => !AnyContains(@this, address);
    }
}