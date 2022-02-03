namespace Swan.Gizmos;

/// <summary>
/// Provides a thread-safe, lazy provider of cached values.
/// </summary>
/// <typeparam name="TKey">The type of the keys.</typeparam>
/// <typeparam name="TValue">The type of the values.</typeparam>
public class ValueCache<TKey, TValue>
    where TKey : notnull
{
    private readonly object SyncLock = new();
    private readonly Dictionary<TKey, TValue> _dictionary = new(64);

    /// <summary>
    /// Creates a new instance of the <see cref="ValueCache{TKey, TValue}"/> class.
    /// </summary>
    public ValueCache()
    {
        // placeholder
    }

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

        lock (SyncLock)
        {
            if (_dictionary.TryGetValue(key, out var value))
                return value;

            value = factory();
            _dictionary[key] = value;
            return value;
        }
    }

    /// <summary>
    /// Clears all the value caches.
    /// </summary>
    public void Clear()
    {
        lock (SyncLock)
        {
            _dictionary.Clear();
        }
    }

    /// <summary>
    /// Clears the value stored in the specified key.
    /// </summary>
    /// <param name="key">The key of the value to clear.</param>
    public void Clear(TKey key)
    {
        lock (SyncLock)
        {
            _dictionary.Remove(key);
        }
    }
}
