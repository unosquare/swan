﻿namespace Unosquare.Swan.Components
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
                var tupleName = typeof(T);
                var tuple = Enum.GetNames(tupleName)
                       .Select(x => new Tuple<int, string>((int)Enum.Parse(tupleName, x), humanize ? x.Humanize() : x))
                       .ToArray();

                if (Cache.ContainsKey(tupleName) == false)
                {
                    Cache.Add(tupleName, tuple);
                }

                if (Cache.GetValueOrDefault(tupleName) != tuple)
                {
                    Cache[tupleName] = tuple;
                }

                return Cache[tupleName];
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
                var i = 0;
                var tupleName = typeof(T);
                var tuple = Enum.GetNames(tupleName)
                        .Select(x => new Tuple<int, string>(i++, humanize ? x.Humanize() : x))
                        .ToArray();

                if (Cache.ContainsKey(tupleName) == false)
                {
                    Cache.Add(tupleName, tuple);
                }

                if (Cache.GetValueOrDefault(tupleName) != tuple)
                {
                    Cache[tupleName] = tuple;
                }

                return Cache[tupleName];
            }
        }
    }
}
