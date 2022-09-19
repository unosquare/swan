namespace Swan.Gizmos;

/// <summary>
/// Represents a dictionary of discrete keys
/// that form a continuum to which keys can be mapped.
/// </summary>
/// <typeparam name="TKey">The key type. Typically dates or integers representing years.</typeparam>
/// <typeparam name="TValue">The type of values stored within the keys.</typeparam>
public class RangeLookup<TKey, TValue> : IDictionary<TKey, TValue>
    where TKey : struct, IComparable, IComparable<TKey>
{
    private readonly SortedList<TKey, TValue> _values = new();

    /// <summary>
    /// Creates a new instance of the <see cref="RangeLookup{TKey, TValue}"/> class.
    /// </summary>
    public RangeLookup()
    {
        // placeholder
    }

    /// <summary>
    /// Creates a new instance of the <see cref="RangeLookup{TKey, TValue}"/> class.
    /// </summary>
    /// <param name="dictionary">The initial dictionary containing the data.</param>
    public RangeLookup(IDictionary<TKey, TValue> dictionary)
    {
        _values = new(dictionary);
    }

    /// <inheritdoc />
    public TValue this[TKey key]
    {
        get => _values.FindStartValue(key)!;
        set => _values[key] = value!;
    }

    /// <inheritdoc />
    public ICollection<TKey> Keys => _values.Keys;

    /// <inheritdoc />
    public ICollection<TValue> Values => _values.Values;

    /// <inheritdoc />
    public int Count => _values.Count;

    /// <inheritdoc />
    public bool IsReadOnly => false;

    /// <inheritdoc />
    public void Add(TKey key, TValue value) => this[key] = value;

    /// <inheritdoc />
    public void Add(KeyValuePair<TKey, TValue> item) => _values[item.Key] = item.Value;

    /// <inheritdoc />
    public void Clear() => _values.Clear();

    /// <inheritdoc />
    public bool Contains(KeyValuePair<TKey, TValue> item) => _values.Contains(item);

    /// <inheritdoc />
    public bool ContainsKey(TKey key) => _values.ContainsKey(key);

    /// <inheritdoc />
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        var index = 0;
        foreach (var kvp in _values)
        {
            if (index >= arrayIndex)
                array[index] = kvp;

            ++index;
        }
    }

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _values.GetEnumerator();

    /// <inheritdoc />
    public bool Remove(TKey key) => _values.Remove(key);

    /// <inheritdoc />
    public bool Remove(KeyValuePair<TKey, TValue> item) => _values.Remove(item.Key);

    /// <inheritdoc />
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        value = this[key];
        return true;
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => (_values as IEnumerable).GetEnumerator();
}
