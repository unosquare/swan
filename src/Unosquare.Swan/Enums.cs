namespace Unosquare.Swan
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
        /// <typeparam name="T"></typeparam>
        /// <param name="humanize">if set to <c>true</c> [humanize].</param>
        /// <returns></returns>
        public static Tuple<int, string>[] GetItemsWithValue<T>(bool humanize = true)
        {
            lock (LockObject)
            {
                if (Cache.ContainsKey(typeof(T)) == false)
                {
                    Cache.Add(typeof(T), Enum.GetNames(typeof(T))
                        .Select(x => new Tuple<int, string>((int) Enum.Parse(typeof(T), x), humanize ? x.Humanize() : x))
                        .ToArray());
                }

                return Cache[typeof(T)];
            }
        }

        /// <summary>
        /// Gets the items with the enum item index.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
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

    /// <summary>
    /// Enumeration of Operating Systems
    /// </summary>
    public enum OperatingSystem
    {
        /// <summary>
        /// Unknown OS
        /// </summary>
        Unknown,
        /// <summary>
        /// Windows
        /// </summary>
        Windows,
        /// <summary>
        /// UNIX/Linux
        /// </summary>
        Unix,
        /// <summary>
        /// Mac OSX
        /// </summary>
        Osx
    }

    /// <summary>
    /// Enumerates the different Application Worker States
    /// </summary>
    public enum AppWorkerState
    {
        /// <summary>
        /// The stopped
        /// </summary>
        Stopped,
        /// <summary>
        /// The running
        /// </summary>
        Running,
    }

    /// <summary>
    /// Enumerates the possible causes of the DataReceived event occurring.
    /// </summary>
    public enum ConnectionDataReceivedTrigger
    {
        /// <summary>
        /// The trigger was a forceful flush of the buffer
        /// </summary>
        Flush,
        /// <summary>
        /// The new line sequence bytes were received
        /// </summary>
        NewLineSequenceEncountered,
        /// <summary>
        /// The buffer was full
        /// </summary>
        BufferFull,
        /// <summary>
        /// The block size reached
        /// </summary>
        BlockSizeReached
    }

}
