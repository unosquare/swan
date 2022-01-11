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
    public static async Task<T?> FirstOrDefault<T>(this IAsyncEnumerable<T> enumerable, CancellationToken ct = default)
    {
        if (enumerable is null)
            return default;

        await foreach (var item in enumerable.WithCancellation(ct).ConfigureAwait(false))
            return item;

        return default;
    }
}

