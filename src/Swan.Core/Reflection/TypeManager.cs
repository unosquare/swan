using System;
using System.Collections.Concurrent;

namespace Swan.Reflection
{
    /// <summary>
    /// Provides efficient access to a cached repository of <see cref="TypeProxy"/>.
    /// </summary>
    public static partial class TypeManager
    {
        private static readonly ConcurrentDictionary<Type, ITypeProxy> TypeCache = new();

        /// <summary>
        /// Provides cached and extended type information for
        /// easy and efficient access to common reflection scenarios.
        /// </summary>
        /// <param name="t">The type to provide extended info for.</param>
        /// <returns>Returns an <see cref="TypeProxy"/> for the given type.</returns>
        public static ITypeProxy TypeInfo(this Type t)
        {
            if (t is null)
                throw new ArgumentNullException(nameof(t));

            if (TypeCache.TryGetValue(t, out var typeInfo))
                return typeInfo;

            typeInfo = new TypeProxy(t);
            TypeCache.TryAdd(t, typeInfo);

            return typeInfo;
        }

        /// <summary>
        /// The closest programmatic equivalent of default(T).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// Default value of this type.
        /// </returns>
        /// <exception cref="ArgumentNullException">type.</exception>
        public static object? GetDefault(this Type type) => type is not null
            ? type.TypeInfo().DefaultValue
            : throw new ArgumentNullException(nameof(type));

        /// <summary>
        /// Calls the parameterless constructor on this type returning an isntance.
        /// For value types it returns the default value.
        /// If no parameterless constructor is available a <see cref="MissingMethodException"/> is thrown.
        /// </summary>
        /// <param name="type">The type to create an instance of.</param>
        /// <returns>A new instance of this type or the default value for value types.</returns>
        public static object CreateInstance(this Type type) => type is not null
            ? type.TypeInfo().CreateInstance()
            : throw new ArgumentNullException(nameof(type));

        /// <summary>
        /// Calls the parameterless constructor on this type returning an isntance.
        /// For value types it returns the default value.
        /// If no parameterless constructor is available a <see cref="MissingMethodException"/> is thrown.
        /// </summary>
        /// <typeparam name="T">The type to create an instance of.</typeparam>
        /// <returns>A new instance of this type or the default value for value types.</returns>
        public static T CreateInstance<T>() => (T)typeof(T).CreateInstance();
    }
}
