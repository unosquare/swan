namespace Swan.Reflection;

/// <summary>
/// Provides type metadata on a collection type
/// by taking the most capable collection interface available on the <see cref="TypeInfo"/>
/// </summary>
public interface ICollectionInfo
{
    /// <summary>
    /// Gets the type proxy that generated and owns this collection type proxy.
    /// </summary>
    ITypeInfo SourceType { get; }

    /// <summary>
    /// Gets the underlying collection kind.
    /// </summary>
    CollectionKind CollectionKind { get; }

    /// <summary>
    /// Gets the underlying constructed interface type
    /// that this collection implements.
    /// </summary>
    ITypeInfo CollectionType { get; }

    /// <summary>
    /// Gets the type proxy for keys in dictionaries.
    /// For non-dictionaries, returns the proxy for <see cref="int"/>.
    /// </summary>
    ITypeInfo KeysType { get; }

    /// <summary>
    /// Gets the type proxy for values in the collection.
    /// For dictionaries it gets the value type in the key-value pairs.
    /// For other collection types, it returns the item type.
    /// </summary>
    ITypeInfo ValuesType { get; }

    /// <summary>
    /// Gets a value indicating that the collection type is a dictionary.
    /// This specifies that the <see cref="KeysType"/> might not be <see cref="int"/>.
    /// </summary>
    bool IsDictionary { get; }

    /// <summary>
    /// Gets a value indicating that the collection type is an array.
    /// </summary>
    bool IsArray { get; }
}
