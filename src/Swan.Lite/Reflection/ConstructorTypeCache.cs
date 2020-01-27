using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swan.Reflection
{
    /// <summary>
    /// A thread-safe cache of constructors belonging to a given type.
    /// </summary>
    public class ConstructorTypeCache : TypeCache<Tuple<ConstructorInfo, ParameterInfo[]>>
    {
        /// <summary>
        /// Gets the default cache.
        /// </summary>
        /// <value>
        /// The default cache.
        /// </value>
        public static Lazy<ConstructorTypeCache> DefaultCache { get; } =
            new Lazy<ConstructorTypeCache>(() => new ConstructorTypeCache());

        /// <summary>
        /// Retrieves all constructors order by the number of parameters ascending.
        /// </summary>
        /// <typeparam name="T">The type to inspect.</typeparam>
        /// <param name="includeNonPublic">if set to <c>true</c> [include non public].</param>
        /// <returns>
        /// A collection with all the constructors in the given type.
        /// </returns>
        public IEnumerable<Tuple<ConstructorInfo, ParameterInfo[]>> RetrieveAllConstructors<T>(bool includeNonPublic = false)
            => Retrieve<T>(GetConstructors(includeNonPublic));

        /// <summary>
        /// Retrieves all constructors order by the number of parameters ascending.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="includeNonPublic">if set to <c>true</c> [include non public].</param>
        /// <returns>
        /// A collection with all the constructors in the given type.
        /// </returns>
        public IEnumerable<Tuple<ConstructorInfo, ParameterInfo[]>> RetrieveAllConstructors(Type type, bool includeNonPublic = false)
            => Retrieve(type, GetConstructors(includeNonPublic));

        private static Func<Type, IEnumerable<Tuple<ConstructorInfo, ParameterInfo[]>>> GetConstructors(bool includeNonPublic)
            => t => t.GetConstructors(includeNonPublic ? BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance : BindingFlags.Public | BindingFlags.Instance)
                .Select(x => Tuple.Create(x, x.GetParameters()))
                .OrderBy(x => x.Item2.Length)
                .ToList();
    }
}
