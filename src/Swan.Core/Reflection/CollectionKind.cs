namespace Swan.Reflection;

/// <summary>
/// Enumerates general collection kinds.
/// </summary>
public enum CollectionKind
{
    /// <summary>
    /// Invalid collection kind.
    /// </summary>
    None,

    /// <summary>
    /// Collection implements <see cref="IDictionary{TKey, TValue}"/>.
    /// </summary>
    GenericDictionary,

    /// <summary>
    /// Collection implements <see cref="IDictionary"/>.
    /// </summary>
    Dictionary,

    /// <summary>
    /// Collection implements <see cref="IList{T}"/>.
    /// </summary>
    GenericList,

    /// <summary>
    /// Collection implements <see cref="IList"/>.
    /// </summary>
    List,

    /// <summary>
    /// Collection implements <see cref="ICollection{T}"/>.
    /// </summary>
    GenericCollection,

    /// <summary>
    /// Collection implements <see cref="ICollection"/>.
    /// </summary>
    Collection,

    /// <summary>
    /// Collection implements <see cref="IEnumerable{T}"/>.
    /// </summary>
    GenericEnumerable,

    /// <summary>
    /// Collection implements <see cref="IEnumerable"/>.
    /// </summary>
    Enumerable,
}
