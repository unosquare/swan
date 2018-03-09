namespace Unosquare.Swan.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Unosquare.Swan.Components;

    /// <summary>
    /// A thread-safe cache of Enums belonging to a given type
    /// The Retrieve method is the most useful one in this class as it
    /// calls the retrieval process if the type is not contained
    /// in the cache.
    /// </summary>
    public class EnumCache : CacheRepository<Type, Tuple<string, object>>
    {
        /// <summary>
        /// Gets all the names and enumerators from a specific Enum type
        /// </summary>
        /// <typeparam name="T">The type of the attribute to be retrieved</typeparam>
        /// <returns>A tuple of enumarator names and their value stored for the specified type</returns>
        public Tuple<string, object>[] Retrieve<T>()
            where T : struct, IConvertible
        {
            return Retrieve(typeof(T), () =>
            {
                var list = new List<Tuple<string, object>>();
                var values = Enum.GetValues(typeof(T)).Cast<object>();

                foreach (var item in values)
                {
                    list.Add(new Tuple<string, object>(Enum.GetName(typeof(T), item), item));
                }
                return list;
            });
        }

        /// <summary>
        /// Retrieves all enumerator names from a specified enum type
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <returns>List of strings</returns>
        public List<string> RetrieveNames<T>()
             where T : struct, IConvertible
        {
            return Retrieve<T>().Select(x => x.Item1).ToList();
        }

        /// <summary>
        /// Retrieves all enumerator names from a specified enum type given a value
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <param name="value">The value</param>
        /// <returns>A list of strings</returns>
        public List<string> RetrieveNames<T>(int value)
             where T : struct, IConvertible
        {
           return Retrieve<T>()
                .Where(x => ((int)x.Item2 & value) == (int)x.Item2)
                .Select(x => x.Item1)
                .ToList();
        }

        /// <summary>
        /// Retrieves all enumerator values from a specified enum type
        /// </summary>
        /// <typeparam name="T">The enum type</typeparam>
        /// <returns>List of enum type</returns>
        public List<T> RetrieveValues<T>()
            where T : struct, IConvertible
        {
            return Retrieve<T>().Select(x => (T) x.Item2).ToList();
        }
    }
}
