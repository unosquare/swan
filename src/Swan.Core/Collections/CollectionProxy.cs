namespace Swan.Collections;

using Swan.Reflection;

/// <summary>
/// Provides a unified API for most commonly available collection types.
/// Please note that the implementation of these methods is not as fast
/// as the collection's native implementation due to some processing and
/// lambda binding that this proxy requires to function properly. Use the actual
/// collection object whenever possible.
/// </summary>
public sealed partial class CollectionProxy : IList, IDictionary, ICollectionInfo
{
    private const string InvalidCastMessage = "Unable to cast value to a suitable type.";
    private readonly object _syncRoot = new();
    private readonly CollectionDelegates Delegates;

    /// <summary>
    /// Creates a new instance of the <see cref="CollectionProxy"/> class.
    /// </summary>
    /// <param name="info">The backing collection info.</param>
    /// <param name="target">The object that this collection proxy wraps.</param>
    private CollectionProxy(ICollectionInfo info, IEnumerable target)
    {
        Info = info;
        Collection = target;
        Delegates = new(target, info);
    }

    /// <summary>
    /// Gets the underlying collection object this proxy operates on.
    /// </summary>
    public object Collection { get; }

    /// <inheritdoc cref="IList" />
    public bool IsFixedSize =>
        SourceType.IsArray ||
        (CollectionKind is CollectionKind.Enumerable or CollectionKind.GenericEnumerable) ||
        (SourceType.TryReadProperty(Collection, nameof(IsFixedSize), out bool value)
            ? value
            : IsReadOnly);

    /// <inheritdoc cref="IList" />
    public bool IsReadOnly =>
        SourceType.TryReadProperty(Collection, nameof(IsReadOnly), out bool value) && value;

    /// <inheritdoc />
    public int Count =>
        SourceType.TryReadProperty(Collection, nameof(Count), out int value)
            ? value
            : Values.Count;

    /// <inheritdoc />
    public bool IsSynchronized =>
        SourceType.TryReadProperty(Collection, nameof(IsSynchronized), out bool value) &&
        value;

    /// <inheritdoc />
    public object SyncRoot =>
        SourceType.TryReadProperty(Collection, nameof(SyncRoot), out object? value)
            ? value ?? _syncRoot
            : _syncRoot;

    /// <inheritdoc />
    public ICollection Keys => Collection is IDictionary dictionary
        ? dictionary.Keys
        : Enumerable.Range(0, Count).ToArray();

    /// <inheritdoc />
    public ICollection Values => Collection is IDictionary dictionary
        ? dictionary.Values
        : this.Cast<object?>().ToArray();

    /// <inheritdoc />
    public ITypeInfo SourceType => Info.SourceType;

    /// <inheritdoc />
    public CollectionKind CollectionKind => Info.CollectionKind;

    /// <inheritdoc />
    public ITypeInfo CollectionType => Info.CollectionType;

    /// <inheritdoc />
    public ITypeInfo KeysType => Info.KeysType;

    /// <inheritdoc />
    public ITypeInfo ValuesType => Info.ValuesType;

    /// <inheritdoc />
    public bool IsDictionary => Info.IsDictionary;

    /// <inheritdoc />
    public bool IsArray => Info.IsArray;

    /// <inheritdoc />
    public object? this[object key]
    {
        get =>
            !TypeManager.TryChangeType(key, KeysType, out var keyItem)
                ? throw new ArgumentException(InvalidCastMessage, nameof(key))
                : keyItem is null
                    ? throw new ArgumentNullException(nameof(key))
                    : KeysType.NativeType == typeof(int)
                        ? this[(int)keyItem]
                        : Delegates.IndexerObjGetter is not null
                            ? Delegates.IndexerObjGetter(keyItem)
                            : throw new NotSupportedException(
                                $"Collection ({SourceType.ShortName}) does not support getting a value via key object.");
        set
        {
            if (!TypeManager.TryChangeType(key, KeysType, out var keyItem))
                throw new ArgumentException(InvalidCastMessage, nameof(key));

            if (!TypeManager.TryChangeType(value, ValuesType, out var valueItem))
                throw new ArgumentException(InvalidCastMessage, nameof(value));

            if (KeysType.NativeType == typeof(int))
            {
                this[(int)keyItem!] = valueItem;
                return;
            }

            if (Delegates.IndexerObjSetter is null)
                throw new NotSupportedException(
                    $"Collection ({SourceType.ShortName}) does not support setting a value via key object.");

            Delegates.IndexerObjSetter.Invoke(keyItem!, valueItem);
        }
    }

    /// <inheritdoc />
    public object? this[int index]
    {
        get
        {
            if (Delegates.IndexerIntGetter is not null)
                return Delegates.IndexerIntGetter(index);

            if (IsArray)
                return (Collection as Array)!.GetValue(index);

            if (IsDictionary)
            {
                var currentIndex = -1;
                foreach (var value in Values)
                {
                    currentIndex++;

                    if (currentIndex == index)
                        return value;
                }
            }
            else
            {
                var currentIndex = -1;
                var enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    currentIndex++;
                    if (currentIndex == index)
                        return enumerator.Current;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(index), $"Collection does not contain index {index}.");
        }
        set
        {
            if (!TypeManager.TryChangeType(value, ValuesType, out var item))
                throw new ArgumentException(InvalidCastMessage, nameof(value));

            if (IsArray)
            {
                (Collection as Array)!.SetValue(item, index);
                return;
            }

            if (Delegates.IndexerIntSetter is null)
                throw new NotSupportedException(
                    $"Collection ({SourceType.ShortName})  does not support setting a value via indexer.");

            Delegates.IndexerIntSetter(index, item);
        }
    }

    /// <summary>
    /// Gets the collection metadata supporting
    /// the operations of this wrapper.
    /// </summary>
    private ICollectionInfo Info { get; }

    /// <summary>
    /// Tries to create a collection proxy for the given target object.
    /// </summary>
    /// <param name="target">The target to create the collection proxy for.</param>
    /// <param name="proxy">The resulting proxy.</param>
    /// <returns>True of the operation succeeds. False otherwise.</returns>
    public static bool TryCreate(object? target, [MaybeNullWhen(false)] out CollectionProxy proxy)
    {
        proxy = default;

        if (target is not IEnumerable enumerableTarget)
            return false;

        if (target is CollectionProxy outputProxy)
        {
            proxy = outputProxy;
            return true;
        }

        var typeProxy = target.GetType().TypeInfo().Collection;
        if (typeProxy is null)
            return false;

        proxy = new(typeProxy, enumerableTarget);
        return true;

    }

    /// <inheritdoc />
    public IEnumerator GetEnumerator() =>
        (Collection as IEnumerable)?.GetEnumerator() ?? throw new InvalidCastException();

    /// <inheritdoc />
    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
        return Collection is IDictionary dictionary
            ? dictionary.GetEnumerator()
            : new CollectionEnumerator(this);
    }

    /// <inheritdoc />
    public int Add(object? value)
    {
        if (Delegates.AddValue is null)
            throw new InvalidOperationException($"Collection of kind {CollectionKind} does not support the {nameof(Add)} operation.");

        if (!TypeManager.TryChangeType(value, ValuesType, out var item))
            throw new ArgumentException(InvalidCastMessage, nameof(value));

        Delegates.AddValue(item);
        return Count - 1;
    }

    /// <inheritdoc />
    public void Add(object key, object? value)
    {
        if (Delegates.AddKeyValue is null)
            throw new InvalidOperationException($"Collection of kind {CollectionKind} does not support the {nameof(Add)} operation.");

        if (!TypeManager.TryChangeType(key, KeysType, out var itemKey) ||
            !TypeManager.TryChangeType(value, ValuesType, out var itemValue))
            throw new ArgumentException(InvalidCastMessage, nameof(value));

        if (itemKey is null)
            throw new ArgumentNullException(nameof(key));

        Delegates.AddKeyValue(itemKey, itemValue);
    }

    /// <summary>
    /// Repeatedly calls the <see cref="Add(object?)"/> method, adding values to the
    /// underlying collection.
    /// </summary>
    /// <param name="values">The values to add to the collection.</param>
    public void AddRange(IEnumerable values)
    {
        if (values is null)
            throw new ArgumentNullException(nameof(values));

        foreach (var value in values)
            Add(value);
    }

    /// <inheritdoc cref="IList" />
    public void Clear()
    {
        if (Delegates.Clear is null)
            throw new NotSupportedException(
                $"Collection ({SourceType.ShortName}) of kind {CollectionKind} does not support the {nameof(Clear)} operation.");

        Delegates.Clear.Invoke();
    }

    /// <summary>
    /// Determines whether an element is in the collection.
    /// For dictionaries, the search occurs in the keys.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>True if the value is found. False otherwise.</returns>
    public bool Contains(object? value) =>
        !TypeManager.TryChangeType(value, IsDictionary ? KeysType : ValuesType, out var searchValue)
            ? throw new ArgumentException(InvalidCastMessage, nameof(value))
            : !IsDictionary
                ? Delegates.Contains?.Invoke(searchValue) ??
                  Values.Cast<object?>().Contains(searchValue)
                : Delegates.ContainsKey?.Invoke(searchValue) ?? (
                  Delegates.Contains?.Invoke(searchValue) ??
                  Keys.Cast<object?>().Contains(searchValue));

    /// <summary>
    /// For dictionaries, it returns the same as <see cref="Contains(object?)"/>.
    /// For other collections, an integer is expected and determines if such value is
    /// within a valid range.
    /// </summary>
    /// <param name="value"></param>
    /// <returns>True if the key is found. False otherwise.</returns>
    public bool ContainsKey(object? value)
    {
        return IsDictionary
            ? Contains(value)
            : TypeManager.TryChangeType(value, KeysType, out var indexKey) &&
                indexKey is int index &&
                index >= 0 &&
                index < Count;
    }

    /// <summary>
    /// Determines whether the collection contains a specific value.
    /// </summary>
    /// <param name="value">The value to search for.</param>
    /// <returns>True if the value is found. False otherwise.</returns>
    public bool ContainsValue(object? value) => IndexOf(value) >= 0;

    /// <summary>
    /// The zero-based index of the first occurrence of value in the entire collection, if found; otherwise, -1.
    /// For dictionaries, the lookup occurs in the values.
    /// </summary>
    /// <param name="value">The value to look for.</param>
    /// <returns>the zero-based index if found, otherwise false.</returns>
    public int IndexOf(object? value)
    {
        var index = -1;
        if (!TypeManager.TryChangeType(value, ValuesType, out var item))
            item = value;

        foreach (var currentItem in Values)
        {
            index++;
            if (Equals(currentItem, item))
                return index;
        }

        return -1;
    }

    /// <inheritdoc />
    public void Insert(int index, object? value)
    {
        if (Delegates.Insert is null)
            throw new NotSupportedException(
                $"Collection ({SourceType.ShortName}) of kind {CollectionKind} does not support the {nameof(Insert)} operation.");

        if (!TypeManager.TryChangeType(value, ValuesType, out var item))
            throw new ArgumentException(InvalidCastMessage, nameof(value));

        Delegates.Insert(index, item);
    }

    /// <summary>
    /// Removes the element with the specified value from the collection.
    /// For dictionaries, removes the element with the specified key.
    /// </summary>
    /// <param name="value">The value or key to look for.</param>
    public void Remove(object? value)
    {
        if (Delegates.Remove is null)
            throw new NotSupportedException(
                $"Collection ({SourceType.ShortName}) of kind {CollectionKind} does not support the {nameof(Remove)} operation.");

        if (!TypeManager.TryChangeType(value, IsDictionary ? KeysType : ValuesType, out var item))
            throw new ArgumentException(InvalidCastMessage, nameof(value));

        Delegates.Remove(item);
    }

    /// <inheritdoc />
    public void RemoveAt(int index)
    {
        if (IsFixedSize || IsReadOnly)
            throw new NotSupportedException(
                $"Collection ({SourceType.ShortName}) of kind {CollectionKind} does not support the {nameof(RemoveAt)} operation.");

        if (IsDictionary)
        {
            if (index >= Keys.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "Index must be greater than 0 and less than the count.");

            var keyIndex = -1;
            foreach (var key in Keys)
            {
                keyIndex++;
                if (keyIndex != index)
                    continue;

                Remove(key);
                break;
            }

            return;
        }

        if (Delegates.RemoveAt is not null)
        {
            Delegates.RemoveAt(index);
            return;
        }

        Remove(this[index]);
    }

    /// <inheritdoc />
    public void CopyTo(Array array, int index)
    {
        if (array is null)
            throw new ArgumentNullException(nameof(array));

        if (index < 0 || index >= array.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        var elementType = array.GetType().GetElementType()?.TypeInfo();

        if (elementType is null)
            throw new ArgumentException($"Unable to obtain array element type.", nameof(array));

        var arrayIndex = index;

        if (IsDictionary)
        {
            foreach (var value in Values)
            {
                if (!TypeManager.TryChangeType(value, elementType, out var item))
                    throw new ArgumentException(InvalidCastMessage, nameof(array));

                array.SetValue(item, arrayIndex);
                arrayIndex++;
                if (arrayIndex >= array.Length)
                    break;
            }
        }
        else
        {
            var enumerator = GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (!TypeManager.TryChangeType(enumerator.Current, elementType, out var item))
                    throw new ArgumentException(InvalidCastMessage, nameof(array));

                array.SetValue(item, arrayIndex);
                arrayIndex++;
                if (arrayIndex >= array.Length)
                    break;
            }
        }
    }

    /// <summary>
    /// Gets the last item in the <see cref="Values"/> collection.
    /// </summary>
    /// <returns>The value in the last position within the collection.</returns>
    public object? Last() => this[Count - 1];

    /// <summary>
    /// Gets the last item in the <see cref="Values"/> collection.
    /// If the last item does not exist, it returns the default value for <see cref="ValuesType"/>.
    /// </summary>
    /// <returns>The value in the last position within the collection.</returns>
    public object? LastOrDefault()
    {
        var count = Count;
        return count <= 0
            ? ValuesType.DefaultValue
            : this[count - 1] ?? ValuesType.DefaultValue;
    }

    /// <summary>
    /// Gets the first item in the <see cref="Values"/> collection.
    /// </summary>
    /// <returns>The value in the first position within the collection.</returns>
    public object? First() => this[0];

    /// <summary>
    /// Gets the first item in the <see cref="Values"/> collection.
    /// If the first item does not exist, it returns the default value for <see cref="ValuesType"/>.
    /// </summary>
    /// <returns>The value in the first position within the collection.</returns>
    public object? FirstOrDefault()
    {
        var count = Count;
        return count <= 0
            ? ValuesType.DefaultValue
            : this[0] ?? ValuesType.DefaultValue;
    }

    /// <summary>
    /// Converts the items in the <see cref="Values"/> to an array.
    /// </summary>
    /// <returns>An array of values.</returns>
    public Array ToArray()
    {
        var result = Array.CreateInstance(ValuesType.NativeType, Count);

        var index = -1;
        foreach (var value in Values)
        {
            index++;
            result.SetValue(value, index);
        }

        return result;
    }

    /// <summary>
    /// Converts the items in the <see cref="Values"/> to a typed array.
    /// </summary>
    /// <typeparam name="T">The target element type.</typeparam>
    /// <returns>An array of values.</returns>
    public T[] ToArray<T>()
    {
        var result = new T[Count];
        var index = -1;
        var targetType = typeof(T).TypeInfo();

        foreach (var value in Values)
        {
            index++;

            if (value is T itemValue)
            {
                result[index] = itemValue;
                continue;
            }

            if (!TypeManager.TryChangeType(value, targetType, out object? changedValue))
                throw new InvalidCastException(InvalidCastMessage);

            if (changedValue is not T typedValue)
                throw new InvalidCastException(InvalidCastMessage);

            result[index] = typedValue;
        }

        return result;
    }

    /// <summary>
    /// Determines if all the values (or their converted values) contained in this collection
    /// are also contained in the other collection and that both collections match their element count.
    /// </summary>
    /// <param name="other">The other collection containing sequences.</param>
    /// <returns>True if the current and other collection contain same or equivalent values.</returns>
    public bool SequenceEquals(CollectionProxy? other)
    {
        if (other is null)
            return false;

        var otherValues = other.Values.AsProxy();
        var sourceCount = 0;
        foreach (var sourceItem in Values)
        {
            sourceCount++;
            if (!other.ContainsValue(sourceItem))
                return false;
        }

        return otherValues.Count == sourceCount;
    }

    /// <summary>
    /// Converts the items in the <see cref="Values"/> to a typed list.
    /// </summary>
    /// <typeparam name="T">The target element type.</typeparam>
    /// <returns>A list of values.</returns>
    public List<T> ToList<T>() => new(ToArray<T>());

    /// <summary>
    /// Iterates through the collection as a set of <see cref="DictionaryEntry"/> items.
    /// </summary>
    /// <param name="entryAction">The action to execute on each key-value pair.</param>
    public void ForEach(Action<DictionaryEntry>? entryAction)
    {
        if (entryAction is null)
            return;

        var dictionary = this as IDictionary;
        foreach (DictionaryEntry kvp in dictionary)
            entryAction?.Invoke(kvp);
    }

    /// <summary>
    /// Tries to copy keys and values from one collection type to another.
    /// </summary>
    /// <param name="target">The target collection proxy.</param>
    /// <returns>True if the operation succeeds. False otherwise.</returns>
    public bool TryCopyTo(CollectionProxy? target)
    {
        if (target is null)
            return false;

        if (target.IsReadOnly)
            return false;

        if (target.IsDictionary)
        {
            var success = true;
            ForEach(kvp =>
            {
                if (!TypeManager.TryChangeType(kvp.Key, target.KeysType, out var key) ||
                    !TypeManager.TryChangeType(kvp.Value, target.ValuesType, out var value) ||
                    key is null)
                {
                    success = false;
                    return;
                }

                target.Add(key, value);
            });

            return success;
        }

        if (target.IsFixedSize)
        {
            for (var i = 0; i < target.Count; i++)
            {
                if (!TypeManager.TryChangeType(this[i], target.ValuesType, out var item))
                    return false;

                target[i] = item;
            }

            return true;
        }

        for (var i = 0; i < Count; i++)
        {
            if (!TypeManager.TryChangeType(this[i], target.ValuesType, out var item))
                return false;

            target.Add(item);
        }

        return true;
    }
}
