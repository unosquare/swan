namespace Swan.Data.Extensions;

using Swan.Data.Records;

/// <summary>
/// Provides extensions for <see cref="IEnumerable"/> compatible objects.
/// </summary>
public static class DataReaderExtensions
{
    /// <summary>
    /// Retrieves the first result from a <see cref="IAsyncEnumerable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The enumerable type.</typeparam>
    /// <param name="enumerable">The enumerable object.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The first item or the default value for the type parameter.</returns>
    public static async Task<T?> FirstOrDefaultAsync<T>(this IAsyncEnumerable<T> enumerable, CancellationToken ct = default)
    {
        if (enumerable is null)
            return default;

        await foreach (var item in enumerable.WithCancellation(ct).ConfigureAwait(false))
            return item;

        return default;
    }

    /// <summary>
    /// Asynchronously iterates over each element and produces a list of items.
    /// This materializes the asynchronous enumerable set.
    /// </summary>
    /// <typeparam name="T">The element type of the list.</typeparam>
    /// <param name="enumerable">The enumerable to iterate over.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A list of elements.</returns>
    public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> enumerable, CancellationToken ct = default)
    {
        const int BufferSize = 1024;

        if (enumerable is null)
            return new(0);

        var result = new List<T>(BufferSize);
        enumerable.WithCancellation(ct).ConfigureAwait(false);

        await foreach (var item in enumerable)
        {
            if (item is null)
                continue;

            result.Add(item);
        }

        return result;
    }

    /// <summary>
    /// Wraps the enumerator of the given collection as a <see cref="IDataReader"/>.
    /// </summary>
    /// <param name="collection">The collection to get the <see cref="IEnumerator"/> from.</param>
    /// <param name="schema">The schema used to produce the record values for the data reader.</param>
    /// <returns>A data reader.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IDataReader ToDataReader(this IEnumerable collection, IDbTableSchema schema) => collection is null
        ? throw new ArgumentNullException(nameof(collection))
        : new CollectionDbReader(collection.GetEnumerator(), schema);

    /// <summary>
    /// Wraps the enumerator of the given collection as a <see cref="IDataReader"/>.
    /// </summary>
    /// <param name="collection">The collection to get the <see cref="IEnumerator"/> from.</param>
    /// <param name="itemType">The type of the items the collection holds. This produces a basic table schema.</param>
    /// <returns>A data reader.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IDataReader ToDataReader(this IEnumerable collection, Type itemType) => collection is null
        ? throw new ArgumentNullException(nameof(collection))
        : new CollectionDbReader(collection.GetEnumerator(), itemType);

    /// <summary>
    /// Wraps the enumerator of the given collection as a <see cref="IDataReader"/>.
    /// </summary>
    /// <typeparam name="T">The type of the items the collection holds. This produces a basic table schema.</typeparam>
    /// <param name="collection">The collection to get the <see cref="IEnumerator"/> from.</param>
    /// <returns>A data reader.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IDataReader ToDataReader<T>(this IEnumerable<T> collection) => collection is null
        ? throw new ArgumentNullException(nameof(collection))
        : new CollectionDbReader(collection.GetEnumerator(), typeof(T));
}
