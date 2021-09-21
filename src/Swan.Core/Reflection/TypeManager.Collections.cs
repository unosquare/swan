using Swan.Collections;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Swan.Reflection
{
    public static partial class TypeManager
    {
        /// <summary>
        /// Wraps any enumerable as a <see cref="CollectionProxy"/>.
        /// </summary>
        /// <param name="target">The collection to wrap.</param>
        /// <returns>A collection proxy that wraps the specified target.</returns>
        public static CollectionProxy AsCollectionProxy(this IEnumerable target)
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            if (!CollectionProxy.TryCreate(target, out var proxy))
                throw new ArgumentException("Unable to create collection proxy.", nameof(target));

            return proxy;
        }

        /// <summary>
        /// Creates an instance of a <see cref="Array"/> from the given
        /// element type and count.
        /// </summary>
        /// <param name="elementType">The type of array elements.</param>
        /// <param name="elementCount">The array length.</param>
        /// <returns>An instance of an array.</returns>
        public static Array CreateArray(Type elementType, int elementCount) =>
            elementType is null
                ? throw new ArgumentNullException(nameof(elementType))
                : elementCount < 0
                    ? throw new ArgumentOutOfRangeException(nameof(elementCount))
                    : Array.CreateInstance(elementType, elementCount);

        /// <summary>
        /// Creates an instance of a <see cref="Array"/> from the given
        /// element type and count.
        /// </summary>
        /// <param name="elementType">The type of array elements.</param>
        /// <param name="elementCount">The array length.</param>
        /// <returns>An instance of an array.</returns>
        public static Array CreateArray(ITypeInfo elementType, int elementCount) =>
            elementType is null
                ? throw new ArgumentNullException(nameof(elementType))
                : elementCount < 0
                    ? throw new ArgumentOutOfRangeException(nameof(elementCount))
                    : Array.CreateInstance(elementType.NativeType, elementCount);

        /// <summary>
        /// Creates a <see cref="List{T}"/> from type arguments.
        /// </summary>
        /// <param name="elementType">The generic type argument.</param>
        /// <returns>An instance of a <see cref="List{T}"/></returns>
        public static IEnumerable CreateGenericList(Type elementType)
        {
            var resultType = typeof(List<>).MakeGenericType(elementType);
            return (resultType.TypeInfo().CreateInstance() as IEnumerable)!;
        }

        /// <summary>
        /// Creates an <see cref="Dictionary{TKey, TValue}"/> from type arguments.
        /// </summary>
        /// <param name="keyType">The type for keys.</param>
        /// <param name="valueType">The type for values.</param>
        /// <returns>An instance of a <see cref="Dictionary{TKey, TValue}"/></returns>
        public static IEnumerable CreateGenericDictionary(Type keyType, Type valueType)
        {
            var resultType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            return (resultType.TypeInfo().CreateInstance() as IEnumerable)!;
        }

    }
}
