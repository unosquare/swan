using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Swan.Reflection
{
    /// <summary>
    /// Provides access to a cached repository of <see cref="ExtendedTypeInfo"/>.
    /// </summary>
    public static partial class TypeManager
    {
        private static readonly ConcurrentDictionary<Type, ExtendedTypeInfo> TypeCache = new();

        /// <summary>
        /// Provides a callection of primitive, numeric types.
        /// </summary>
        public static IReadOnlyCollection<Type> NumericTypes { get; } = new[]
        {
            typeof(byte),
            typeof(sbyte),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(short),
            typeof(ushort),
        };

        /// <summary>
        /// Provides a collection of basic value types including numeric types,
        /// string, guid, timespan, and datetime.
        /// </summary>
        public static IReadOnlyCollection<Type> BasicValueTypes { get; } = new[]
        {
                typeof(int),
                typeof(bool),
                typeof(string),
                typeof(DateTime),
                typeof(double),
                typeof(decimal),
                typeof(Guid),
                typeof(long),
                typeof(TimeSpan),
                typeof(uint),
                typeof(float),
                typeof(byte),
                typeof(short),
                typeof(sbyte),
                typeof(ushort),
                typeof(ulong),
                typeof(char),
        };

        /// <summary>
        /// Provides cached and extended type information for
        /// easy and efficient access to common reflection scenarios.
        /// </summary>
        /// <param name="t">The type to provide extended info for.</param>
        /// <returns>Returns an <see cref="ExtendedTypeInfo"/> for the given type.</returns>
        public static ExtendedTypeInfo TypeInfo(this Type t)
        {
            if (t is null)
                throw new ArgumentNullException(nameof(t));

            if (TypeCache.TryGetValue(t, out var typeInfo))
                return typeInfo;

            typeInfo = new ExtendedTypeInfo(t);
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
            ? type.TypeInfo().Default
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
