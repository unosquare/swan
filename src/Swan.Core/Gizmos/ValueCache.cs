namespace Swan.Gizmos;

/// <summary>
/// Provides a thread-safe, lazy provider of cached values.
/// </summary>
/// <typeparam name="TKey">The type of the keys.</typeparam>
/// <typeparam name="TValue">The type of the values.</typeparam>
public class ValueCache<TKey, TValue>
    where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, TValue> _dictionary = new();

    /// <summary>
    /// Creates a new instance of the <see cref="ValueCache{TKey, TValue}"/> class.
    /// </summary>
    public ValueCache()
    {
        // placeholder
    }

    /// <summary>
    /// Gets a value indicating whether the value for the given key exists.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>True if the key exists. False otherwise.</returns>
    public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

    /// <summary>
    /// Gets a value if it exists in the cache or returns the default value.
    /// </summary>
    /// <param name="key">The key to test for.</param>
    /// <returns>The value or its default.</returns>
    public TValue? GetValueOrDefault(TKey key) => _dictionary.TryGetValue(key, out var value)
        ? value
        : default;

    /// <summary>
    /// Gets a cached value. If the key is not found, the factory method is
    /// called, the result cached, and the value is returned.
    /// </summary>
    /// <param name="key">The key store.</param>
    /// <param name="factory">The factory method.</param>
    /// <returns>The cached or newly created value.</returns>
    public TValue GetValue(TKey key, Func<TValue> factory)
    {
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        if (_dictionary.TryGetValue(key, out var value))
            return value;

        _dictionary[key] = value = factory();
        return value;
    }

    /// <summary>
    /// Gets a cached value. If the key is not found, the factory method is
    /// called, the result cached, and the value is returned.
    /// </summary>
    /// <param name="key">The key store.</param>
    /// <param name="factory">The factory method.</param>
    /// <returns>The cached or newly created value.</returns>
    public async Task<TValue> GetValueAsync(TKey key, Func<Task<TValue>> factory)
    {
        if (factory is null)
            throw new ArgumentNullException(nameof(factory));

        if (_dictionary.TryGetValue(key, out var value))
            return value;

        _dictionary[key] = value = await factory().ConfigureAwait(false);
        return value;
    }

    /// <summary>
    /// Clears all the value caches.
    /// </summary>
    public void Clear() => _dictionary.Clear();

    /// <summary>
    /// Clears the value stored in the specified key.
    /// </summary>
    /// <param name="key">The key of the value to clear.</param>
    public void Clear(TKey key) => _dictionary.TryRemove(key, out _);
}
