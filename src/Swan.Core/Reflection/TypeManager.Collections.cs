namespace Swan.Reflection;

using Swan.Collections;

public static partial class TypeManager
{
    /// <summary>
    /// Wraps any enumerable as a <see cref="CollectionProxy"/>.
    /// </summary>
    /// <param name="target">The collection to wrap.</param>
    /// <returns>A collection proxy that wraps the specified target.</returns>
    public static CollectionProxy AsProxy(this IEnumerable target)
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
    public static CollectionProxy CreateArray(Type elementType, int elementCount) =>
        elementType is null
            ? throw new ArgumentNullException(nameof(elementType))
            : elementCount < 0
                ? throw new ArgumentOutOfRangeException(nameof(elementCount))
                : Array.CreateInstance(elementType, elementCount).AsProxy();

    /// <summary>
    /// Creates an instance of a <see cref="Array"/> from the given
    /// element type and count.
    /// </summary>
    /// <param name="elementType">The type of array elements.</param>
    /// <param name="elementCount">The array length.</param>
    /// <returns>An instance of an array.</returns>
    public static CollectionProxy CreateArray(ITypeInfo elementType, int elementCount) =>
        elementType is null
            ? throw new ArgumentNullException(nameof(elementType))
            : elementCount < 0
                ? throw new ArgumentOutOfRangeException(nameof(elementCount))
                : Array.CreateInstance(elementType.NativeType, elementCount).AsProxy();

    /// <summary>
    /// Creates a <see cref="List{T}"/> from type arguments.
    /// </summary>
    /// <param name="elementType">The generic type argument.</param>
    /// <returns>An instance of a <see cref="List{T}"/></returns>
    public static CollectionProxy CreateGenericList(Type elementType)
    {
        var resultType = typeof(List<>).MakeGenericType(elementType);
        return (resultType.TypeInfo().CreateInstance() as IEnumerable)!.AsProxy();
    }

    /// <summary>
    /// Creates an <see cref="Dictionary{TKey, TValue}"/> from type arguments.
    /// </summary>
    /// <param name="keyType">The type for keys.</param>
    /// <param name="valueType">The type for values.</param>
    /// <returns>An instance of a <see cref="Dictionary{TKey, TValue}"/></returns>
    public static CollectionProxy CreateGenericDictionary(Type keyType, Type valueType)
    {
        var resultType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
        return (resultType.TypeInfo().CreateInstance() as IEnumerable)!.AsProxy();
    }

}
