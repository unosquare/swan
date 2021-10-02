namespace Swan.Collections
{
    using Swan.Reflection;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Provides a unified API for most commonly available collection types.
    /// Please note that the implementation of these methods is not as fast
    /// as the collection's native implementation due to some processing and
    /// lambda binding that this proxy requires o function properly.
    /// </summary>
    public sealed class CollectionProxy : IList, IDictionary, ICollectionInfo
    {
        private const string InvalidCastMessage = "Unable to cast value to a suitable type.";
        private readonly object _syncRoot = new();
        private readonly DelegateFactory Delegates;

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
                    : KeysType.NativeType == typeof(int)
                        ? this[(int)keyItem]
                        : Delegates.ObjectGetter is not null
                            ? Delegates.ObjectGetter(keyItem)
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
                    this[(int)keyItem] = valueItem;
                    return;
                }

                if (Delegates.ObjectSetter is null)
                    throw new NotSupportedException(
                        $"Collection ({SourceType.ShortName}) does not support setting a value via key object.");

                Delegates.ObjectSetter.Invoke(keyItem, valueItem);
            }
        }

        /// <inheritdoc />
        public object? this[int index]
        {
            get
            {
                if (Delegates.IndexGetter is not null)
                    return Delegates.IndexGetter(index);

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

                if (Delegates.IndexSetter is null)
                    throw new NotSupportedException(
                        $"Collection ({SourceType.ShortName})  does not support setting a value via indexer.");

                Delegates.IndexSetter(index, item);
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
        public IEnumerator GetEnumerator() => Delegates.GetEnumerator.Invoke();

        /// <inheritdoc />
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            if (Collection is IDictionary dictionary)
                return dictionary.GetEnumerator();

            return new ListDictionaryEnumerator(this);
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
            if (IsDictionary)
                return Contains(value);

            if (!TypeManager.TryChangeType(value, KeysType, out var indexKey) ||
                indexKey is not int index)
                return false;

            return index >= 0 && index < Count;
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
            if (count <= 0) return ValuesType.DefaultValue;

            return this[count - 1] ?? ValuesType.DefaultValue;
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
            if (count <= 0) return ValuesType.DefaultValue;

            return this[0] ?? ValuesType.DefaultValue;
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
                        !TypeManager.TryChangeType(kvp.Value, target.ValuesType, out var value))
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

        private class ListDictionaryEnumerator : IDictionaryEnumerator
        {
            private int _currentIndex = -1;

            public ListDictionaryEnumerator(CollectionProxy proxy)
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

        private class DelegateFactory
        {
            private readonly Lazy<Func<IEnumerator>> GetEnumeratorLazy;
            private readonly Lazy<Action?> ClearLazy;
            private readonly Lazy<Action<object?>?> RemoveLazy;
            private readonly Lazy<Action<object?>?> AddValueLazy;
            private readonly Lazy<Action<object, object?>?> AddKeyValueLazy;
            private readonly Lazy<Func<object?, bool>?> ContainsLazy;
            private readonly Lazy<Func<object?, bool>?> ContainsKeyLazy;
            private readonly Lazy<Action<int, object?>?> InsertLazy;
            private readonly Lazy<Action<int>?> RemoveAtLazy;
            private readonly Lazy<Func<int, object?>?> IndexGetterLazy;
            private readonly Lazy<Action<int, object?>?> IndexSetterLazy;
            private readonly Lazy<Func<object, object?>?> ObjectGetterLazy;
            private readonly Lazy<Action<object, object?>?> ObjectSetterLazy;

            public DelegateFactory(IEnumerable target, ICollectionInfo info)
            {
                GetEnumeratorLazy = new(() =>
                {
                    info.SourceType.TryFindPublicMethod(nameof(IEnumerable.GetEnumerator), null, out var method);
                    return Expression.Lambda<Func<IEnumerator>>(Expression.Convert(
                        Expression.Call(Expression.Constant(target), method!),
                        typeof(IEnumerator))).Compile();
                }, true);

                ClearLazy = new(() => info.SourceType.TryFindPublicMethod(nameof(IList.Clear), null, out var method)
                    ? method.CreateDelegate<Action>(target)
                    : default, true);

                RemoveLazy = new(() =>
                {
                    var elementType = info.IsDictionary ? info.KeysType.NativeType : info.ValuesType.NativeType;
                    var parameterTypes = new[] { elementType };

                    if (!info.SourceType.TryFindPublicMethod(nameof(IList.Remove), parameterTypes, out var method))
                        return default;

                    elementType = method.GetParameters().First().ParameterType;
                    var valueParameter = Expression.Parameter(typeof(object), "value");
                    var body = Expression.Call(
                        Expression.Constant(target), method, Expression.Convert(valueParameter, elementType));
                    return Expression.Lambda<Action<object?>>(body, valueParameter).Compile();
                }, true);

                RemoveAtLazy = new(() =>
                {
                    var parameterTypes = new[] { typeof(int) };
                    if (!info.SourceType.TryFindPublicMethod(nameof(IList.RemoveAt), parameterTypes, out var method))
                        return default;

                    var valueParameter = Expression.Parameter(typeof(int), "value");
                    var body = Expression.Call(
                        Expression.Constant(target), method, valueParameter);
                    return Expression.Lambda<Action<int>>(body, valueParameter).Compile();
                }, true);

                AddValueLazy = new(() =>
                {
                    if (info.IsDictionary)
                        return default;

                    var elementType = info.ValuesType.NativeType;
                    var parameterTypes = new[] { elementType };

                    if (!info.SourceType.TryFindPublicMethod(nameof(IList.Add), parameterTypes, out var method))
                        return default;

                    elementType = method.GetParameters().First().ParameterType;
                    var valueParameter = Expression.Parameter(typeof(object), "value");
                    var body = Expression.Call(
                        Expression.Constant(target), method, Expression.Convert(valueParameter, elementType));
                    return Expression.Lambda<Action<object?>>(body, valueParameter).Compile();
                }, true);

                AddKeyValueLazy = new(() =>
                {
                    if (!info.IsDictionary)
                        return default;

                    var keysType = info.KeysType.NativeType;
                    var valuesType = info.ValuesType.NativeType;
                    var parameterTypes = new[] { keysType, valuesType };

                    if (!info.SourceType.TryFindPublicMethod(nameof(IDictionary.Add), parameterTypes, out var method))
                        return default;

                    var keyParameter = Expression.Parameter(typeof(object), "key");
                    var valueParameter = Expression.Parameter(typeof(object), "value");

                    keysType = method.GetParameters().First().ParameterType;
                    valuesType = method.GetParameters().Last().ParameterType;

                    var body = Expression.Call(
                        Expression.Constant(target), method,
                        Expression.Convert(keyParameter, keysType),
                        Expression.Convert(valueParameter, valuesType));
                    return Expression.Lambda<Action<object, object?>>(body, keyParameter, valueParameter).Compile();
                }, true);

                ContainsLazy = new(() =>
                {
                    var elementType = info.IsDictionary ? info.KeysType.NativeType : info.ValuesType.NativeType;
                    var parameterTypes = new[] { elementType };

                    if (!info.SourceType.TryFindPublicMethod(nameof(IDictionary.Contains), parameterTypes, out var method))
                        return default;

                    elementType = method.GetParameters().First().ParameterType;
                    var valueParameter = Expression.Parameter(typeof(object), "value");

                    var body = Expression.Call(
                        Expression.Constant(target), method,
                        Expression.Convert(valueParameter, elementType));
                    return Expression.Lambda<Func<object?, bool>>(body, valueParameter).Compile();
                }, true);

                ContainsKeyLazy = new(() =>
                {
                    if (!info.IsDictionary)
                        return default;

                    var elementType = info.KeysType.NativeType;
                    var parameterTypes = new[] { elementType };

                    if (!info.SourceType.TryFindPublicMethod(nameof(IDictionary<int, int>.ContainsKey), parameterTypes, out var method))
                        return default;

                    elementType = method.GetParameters().First().ParameterType;
                    var valueParameter = Expression.Parameter(typeof(object), "value");

                    var body = Expression.Call(
                        Expression.Constant(target), method,
                        Expression.Convert(valueParameter, elementType));
                    return Expression.Lambda<Func<object?, bool>>(body, valueParameter).Compile();
                }, true);

                InsertLazy = new(() =>
                {
                    if (info.IsDictionary)
                        return default;

                    var elementType = info.KeysType.NativeType;
                    var parameterTypes = new[] { typeof(int), elementType };

                    if (!info.SourceType.TryFindPublicMethod(nameof(IList<int>.Insert), parameterTypes, out var method))
                        return default;

                    elementType = method.GetParameters().Last().ParameterType;
                    var indexParameter = Expression.Parameter(typeof(int), "index");
                    var valueParameter = Expression.Parameter(typeof(object), "value");

                    var body = Expression.Call(
                        Expression.Constant(target), method,
                        indexParameter, Expression.Convert(valueParameter, elementType));
                    return Expression.Lambda<Action<int, object?>>(body, indexParameter, valueParameter).Compile();

                }, true);

                IndexGetterLazy = new(() =>
                {
                    var allProperties = info.SourceType.NativeType
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .ToArray();

                    var indexer = allProperties
                        .Select(c => new { Property = c, IndexParameters = c.GetIndexParameters() })
                        .FirstOrDefault(c =>
                            c.IndexParameters.Length == 1 && c.IndexParameters[0].ParameterType == typeof(int));

                    if (indexer is null)
                        return default;

                    var argument = Expression.Parameter(typeof(int), "index");
                    var property = Expression.Convert(
                        Expression.Property(Expression.Constant(target), indexer.Property, argument),
                        typeof(object));

                    var getter = Expression
                        .Lambda<Func<int, object?>>(property, argument)
                        .Compile();

                    return getter;

                }, true);

                ObjectGetterLazy = new(() =>
                {
                    var allProperties = info.SourceType.NativeType
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .ToArray();

                    var indexer = allProperties
                        .Select(c => new { Property = c, IndexParameters = c.GetIndexParameters() })
                        .FirstOrDefault(c =>  c.IndexParameters.Length == 1 && c.IndexParameters[0].ParameterType != typeof(int));

                    if (indexer is null)
                        return default;

                    var argument = Expression.Parameter(typeof(object), "index");
                    var conversionType = indexer.IndexParameters[0].ParameterType;

                    var property = Expression.Convert(
                        Expression.Property(Expression.Constant(target),
                            indexer.Property,
                            Expression.Convert(argument, conversionType)),
                        typeof(object));

                    var getter = Expression
                        .Lambda<Func<object, object?>>(property, argument)
                        .Compile();

                    return getter;
                }, true);

                IndexSetterLazy = new(() =>
                {
                    var allProperties = info.SourceType.NativeType
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .ToArray();

                    var indexer = allProperties
                        .Select(c => new { Property = c, IndexParameters = c.GetIndexParameters() })
                        .FirstOrDefault(c =>
                            c.IndexParameters.Length == 1 && c.IndexParameters[0].ParameterType == typeof(int));

                    if (indexer is null)
                        return default;

                    var valueArgument = Expression.Parameter(typeof(object), "value");
                    var indexArgument = Expression.Parameter(typeof(int), "index");

                    var body = Expression.Assign(
                        Expression.Property(Expression.Constant(target), indexer.Property, indexArgument),
                        Expression.Convert(valueArgument, indexer.Property.PropertyType));

                    var setter = Expression
                        .Lambda<Action<int, object?>>(body, indexArgument, valueArgument)
                        .Compile();

                    return setter;

                }, true);

                ObjectSetterLazy = new(() =>
                {
                    var allProperties = info.SourceType.NativeType
                        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .ToArray();

                    var indexer = allProperties
                        .Select(c => new { Property = c, IndexParameters = c.GetIndexParameters() })
                        .FirstOrDefault(c => c.IndexParameters.Length == 1 && c.IndexParameters[0].ParameterType != typeof(int));

                    if (indexer is null)
                        return default;

                    var valueArgument = Expression.Parameter(typeof(object), "value");
                    var keyArgument = Expression.Parameter(typeof(object), "key");
                    var keyType = indexer.IndexParameters[0].ParameterType;

                    var body = Expression.Assign(
                        Expression.Property(
                            Expression.Constant(target),
                            indexer.Property,
                            Expression.Convert(keyArgument, keyType)),
                        Expression.Convert(valueArgument, indexer.Property.PropertyType));

                    var setter = Expression
                        .Lambda<Action<object, object?>>(body, keyArgument, valueArgument)
                        .Compile();

                    return setter;

                }, true);
            }

            public Func<IEnumerator> GetEnumerator => GetEnumeratorLazy.Value;

            public Action? Clear => ClearLazy.Value;

            public Action<object?>? Remove => RemoveLazy.Value;

            public Action<object?>? AddValue => AddValueLazy.Value;

            public Action<object, object?>? AddKeyValue => AddKeyValueLazy.Value;

            public Func<object?, bool>? Contains => ContainsLazy.Value;

            public Func<object?, bool>? ContainsKey => ContainsKeyLazy.Value;

            public Action<int, object?>? Insert => InsertLazy.Value;

            public Action<int>? RemoveAt => RemoveAtLazy.Value;

            public Func<int, object?>? IndexGetter => IndexGetterLazy.Value;

            public Func<object, object?>? ObjectGetter => ObjectGetterLazy.Value;

            public Action<int, object?>? IndexSetter => IndexSetterLazy.Value;

            public Action<object, object?>? ObjectSetter => ObjectSetterLazy.Value;
        }
    }
}
