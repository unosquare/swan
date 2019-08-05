using System;
using System.Collections.Generic;
using System.Linq;
using Swan.Collections;

namespace Swan
{
    /// <summary>
    /// Provide Enumerations helpers with internal cache.
    /// </summary>
    public class EnumHelper
        : SingletonBase<CollectionCacheRepository<Tuple<string, object>>>
    {
        /// <summary>
        /// Gets all the names and enumerators from a specific Enum type.
        /// </summary>
        /// <typeparam name="T">The type of the attribute to be retrieved.</typeparam>
        /// <returns>A tuple of enumerator names and their value stored for the specified type.</returns>
        public static IEnumerable<Tuple<string, object>> Retrieve<T>()
            where T : struct, IConvertible
        {
            return Instance.Retrieve(typeof(T), t => Enum.GetValues(t)
                .Cast<object>()
                .Select(item => Tuple.Create(Enum.GetName(t, item), item)));
        }

        /// <summary>
        /// Gets the cached items with the enum item value.
        /// </summary>
        /// <typeparam name="T">The type of enumeration.</typeparam>
        /// <param name="humanize">if set to <c>true</c> [humanize].</param>
        /// <returns>
        /// A collection of Type/Tuple pairs 
        /// that represents items with the enum item value.
        /// </returns>
        public static IEnumerable<Tuple<int, string>> GetItemsWithValue<T>(bool humanize = true)
            where T : struct, IConvertible
        {
            return Retrieve<T>()
                .Select(x => Tuple.Create((int) x.Item2, humanize ? x.Item1.Humanize() : x.Item1));
        }

        /// <summary>
        /// Gets the flag values.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="ignoreZero">if set to <c>true</c> [ignore zero].</param>
        /// <returns>
        /// A list of values in the flag.
        /// </returns>
        public static IEnumerable<int> GetFlagValues<TEnum>(int value, bool ignoreZero = false)
            where TEnum : struct, IConvertible
        {
            return Retrieve<TEnum>()
                .Select(x => (int) x.Item2)
                .When(() => ignoreZero, q => q.Where(f => f != 0))
                .Where(x => (x & value) == x);
        }

        /// <summary>
        /// Gets the flag values.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="ignoreZero">if set to <c>true</c> [ignore zero].</param>
        /// <returns>
        /// A list of values in the flag.
        /// </returns>
        public static IEnumerable<long> GetFlagValues<TEnum>(long value, bool ignoreZero = false)
            where TEnum : struct, IConvertible
        {
            return Retrieve<TEnum>()
                .Select(x => (long) x.Item2)
                .When(() => ignoreZero, q => q.Where(f => f != 0))
                .Where(x => (x & value) == x);
        }

        /// <summary>
        /// Gets the flag values.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="ignoreZero">if set to <c>true</c> [ignore zero].</param>
        /// <returns>
        /// A list of values in the flag.
        /// </returns>
        public static IEnumerable<byte> GetFlagValues<TEnum>(byte value, bool ignoreZero = false)
            where TEnum : struct, IConvertible
        {
            return Retrieve<TEnum>()
                .Select(x => (byte) x.Item2)
                .When(() => ignoreZero, q => q.Where(f => f != 0))
                .Where(x => (x & value) == x);
        }

        /// <summary>
        /// Gets the flag names.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="value">the value.</param>
        /// <param name="ignoreZero">if set to <c>true</c> [ignore zero].</param>
        /// <param name="humanize">if set to <c>true</c> [humanize].</param>
        /// <returns>
        /// A list of flag names.
        /// </returns>
        public static IEnumerable<string> GetFlagNames<TEnum>(int value, bool ignoreZero = false, bool humanize = true)
            where TEnum : struct, IConvertible
        {
            return Retrieve<TEnum>()
                .When(() => ignoreZero, q => q.Where(f => (int) f.Item2 != 0))
                .Where(x => ((int) x.Item2 & value) == (int) x.Item2)
                .Select(x => humanize ? x.Item1.Humanize() : x.Item1);
        }

        /// <summary>
        /// Gets the flag names.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="ignoreZero">if set to <c>true</c> [ignore zero].</param>
        /// <param name="humanize">if set to <c>true</c> [humanize].</param>
        /// <returns>
        /// A list of flag names.
        /// </returns>
        public static IEnumerable<string> GetFlagNames<TEnum>(long value, bool ignoreZero = false, bool humanize = true)
            where TEnum : struct, IConvertible
        {
            return Retrieve<TEnum>()
                .When(() => ignoreZero, q => q.Where(f => (long) f.Item2 != 0))
                .Where(x => ((long) x.Item2 & value) == (long) x.Item2)
                .Select(x => humanize ? x.Item1.Humanize() : x.Item1);
        }

        /// <summary>
        /// Gets the flag names.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="ignoreZero">if set to <c>true</c> [ignore zero].</param>
        /// <param name="humanize">if set to <c>true</c> [humanize].</param>
        /// <returns>
        /// A list of flag names.
        /// </returns>
        public static IEnumerable<string> GetFlagNames<TEnum>(byte value, bool ignoreZero = false, bool humanize = true)
            where TEnum : struct, IConvertible
        {
            return Retrieve<TEnum>()
                .When(() => ignoreZero, q => q.Where(f => (byte) f.Item2 != 0))
                .Where(x => ((byte) x.Item2 & value) == (byte) x.Item2)
                .Select(x => humanize ? x.Item1.Humanize() : x.Item1);
        }

        /// <summary>
        /// Gets the cached items with the enum item index.
        /// </summary>
        /// <typeparam name="T">The type of enumeration.</typeparam>
        /// <param name="humanize">if set to <c>true</c> [humanize].</param>
        /// <returns>
        /// A collection of Type/Tuple pairs that represents items with the enum item value.
        /// </returns>
        public static IEnumerable<Tuple<int, string>> GetItemsWithIndex<T>(bool humanize = true)
            where T : struct, IConvertible
        {
            var i = 0;

            return Retrieve<T>()
                .Select(x => Tuple.Create(i++, humanize ? x.Item1.Humanize() : x.Item1));
        }
    }
}