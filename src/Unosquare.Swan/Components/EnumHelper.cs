namespace Unosquare.Swan.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Provide Enumerations helpers with internal cache
    /// </summary>
    public static class EnumHelper
    {
        private static readonly Dictionary<Type, Tuple<int, string>[]> Cache =
            new Dictionary<Type, Tuple<int, string>[]>();

        private static readonly object LockObject = new object();

        /// <summary>
        /// Gets the items with the enum item value.
        /// </summary>
        /// <typeparam name="T">The type of enumeration</typeparam>
        /// <param name="humanize">if set to <c>true</c> [humanize].</param>
        /// <returns>
        /// A collection of Type/Tuple pairs 
        /// that represents items with the enum item value
        /// </returns>
        public static Tuple<int, string>[] GetItemsWithValue<T>(bool humanize = true)
        {
            lock (LockObject)
            {
                if (Cache.ContainsKey(typeof(T)) == false)
                {
                    Cache.Add(typeof(T), Enum.GetNames(typeof(T))
                        .Select(x => new Tuple<int, string>((int)Enum.Parse(typeof(T), x), humanize ? x.Humanize() : x))
                        .ToArray());
                }

                return Cache[typeof(T)];
            }
        }

        /// <summary>
        /// Gets the items with the enum item index.
        /// </summary>
        /// <typeparam name="T">The type of enumeration</typeparam>
        /// <param name="humanize">if set to <c>true</c> [humanize].</param>
        /// <returns>
        /// A collection of Type/Tuple pairs that represents items with the enum item value
        /// </returns>
        public static Tuple<int, string>[] GetItemsWithIndex<T>(bool humanize = true)
        {
            lock (LockObject)
            {
                if (Cache.ContainsKey(typeof(T)) == false)
                {
                    var i = 0;

                    Cache.Add(typeof(T), Enum.GetNames(typeof(T))
                        .Select(x => new Tuple<int, string>(i++, humanize ? x.Humanize() : x))
                        .ToArray());
                }

                return Cache[typeof(T)];
            }
        }
    }
}
