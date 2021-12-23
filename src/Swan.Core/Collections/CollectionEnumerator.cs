namespace Swan.Collections;

internal struct CollectionEnumerator : IDictionaryEnumerator
{
    private int _currentIndex = -1;

    public CollectionEnumerator(CollectionProxy proxy)
    {
        Proxy = proxy;
    }

    public DictionaryEntry Entry => _currentIndex >= 0 ? new(Key, Value) : default;

    public object Key => _currentIndex;

    public object? Value => _currentIndex >= 0 ? Proxy[_currentIndex] : default;

    public object Current => _currentIndex >= 0 ? Entry : default;

    private CollectionProxy Proxy { get; }

    public bool MoveNext()
    {
        var elementCount = Proxy.Count;
        _currentIndex++;
        if (_currentIndex < elementCount)
            return true;

        _currentIndex = elementCount - 1;
        return false;
    }

    public void Reset()
    {
        _currentIndex = -1;
    }
}
