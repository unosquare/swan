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
        private static readonly Dictionary<Type, Tuple<int, string>[]> ValueCache =
            new Dictionary<Type, Tuple<int, string>[]>();

        private static readonly Dictionary<Type, Tuple<int, string>[]> IndexCache =
            new Dictionary<Type, Tuple<int, string>[]>();

        private static readonly Dictionary<Type, Array> ArrayValueCache =
            new Dictionary<Type, Array>();

        private static readonly object LockObject = new object();

        /// <summary>
        /// Gets the cached items with the enum item value.
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

                if (ValueCache.ContainsKey(tupleName) == false)
                {
                    ValueCache.Add(tupleName, Enum.GetNames(tupleName)
                        .Select(x =>
                            new Tuple<int, string>((int) Enum.Parse(tupleName, x), humanize ? x.Humanize() : x))
                        .ToArray());
                }

                return ValueCache[tupleName];
            }
        }

        /// <summary>
        /// Gets the cached items with the enum item index.
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
                var tupleName = typeof(T);

                if (IndexCache.ContainsKey(tupleName) == false)
                {
                    var i = 0;

                    IndexCache.Add(tupleName, Enum.GetNames(tupleName)
                        .Select(x => new Tuple<int, string>(i++, humanize ? x.Humanize() : x))
                        .ToArray());
                }

                return IndexCache[tupleName];
            }
        }

        /// <summary>
        /// Gets the cached values array.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <returns>The array with values from the enumeration</returns>
        public static Array GetValuesArray<TEnum>()
        {
            lock (LockObject)
            {
                var key = typeof(TEnum);

                if (ArrayValueCache.ContainsKey(key) == false)
                {
                    ArrayValueCache.Add(key, Enum.GetValues(typeof(TEnum)));
                }

                return ArrayValueCache[key];
            }
        }

        /// <summary>
        /// Gets the flag values.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="ignoreZero">if set to <c>true</c> [ignore zero].</param>
        /// <returns>
        /// A list of values in the flag
        /// </returns>
        public static List<int> GetFlagValues<TEnum>(int value, bool ignoreZero = false)
            where TEnum : struct, IConvertible
        {
            return GetValuesArray<TEnum>()
                .Cast<int>()
                .When(() => ignoreZero, q => q.Where(f => f != 0))
                .Where(f => (f & value) == f)
                .ToList();
        }

        /// <summary>
        /// Gets the flag values.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>A list of values in the flag</returns>
        public static List<long> GetFlagValues<TEnum>(long value)
            where TEnum : struct, IConvertible
        {
            return GetValuesArray<TEnum>()
                .Cast<long>()
                .Where(f => (f & value) == f)
                .ToList();
        }

        /// <summary>
        /// Gets the flag values.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>A list of values in the flag</returns>
        public static List<byte> GetFlagValues<TEnum>(byte value)
            where TEnum : struct, IConvertible
        {
            return GetValuesArray<TEnum>()
                .Cast<byte>()
                .Where(f => (f & value) == f)
                .ToList();
        }
    }
}