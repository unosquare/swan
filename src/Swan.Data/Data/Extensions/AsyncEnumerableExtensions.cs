namespace Swan.Data.Extensions;

/// <summary>
/// Provides extensions for asynchronous enumerables
/// </summary>
public static class AsyncEnumerableExtensions
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
        await foreach (var item in enumerable.WithCancellation(ct).ConfigureAwait(false))
        {
            if (item is null)
                continue;

            result.Add(item);
        }

        return result;
    }
}

