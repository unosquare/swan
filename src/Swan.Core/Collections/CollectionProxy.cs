using Swan.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Swan.Collections
{
    /// <summary>
    /// Provides a unified API for most commonly available collection types.
    /// Please not that the implementation of these methods is not as performant
    /// as the collection's native implementation due to some prcessing and
    /// dynamic binding that this proxy requires o function properly.
    /// </summary>
    public sealed class CollectionProxy : IList, IDictionary, ICollectionInfo
    {
        private readonly object _syncRoot = new();

        /// <summary>
        /// Creates a new instance of the <see cref="CollectionProxy"/> class.
        /// </summary>
        /// <param name="info">The backing collection info.</param>
        /// <param name="target">The object that this collection proxy wraps.</param>
        private CollectionProxy(ICollectionInfo info, dynamic target)
        {
            Info = info;
            Collection = target;
        }

        /// <summary>
        /// Gets the underlying collection object this wrapper operates on.
        /// </summary>
        public dynamic Collection { get; }

        /// <inheritdoc cref="IList" />
        public bool IsFixedSize
        {
            get
            {
                if (SourceType.IsArray)
                    return true;

                if (CollectionKind is CollectionKind.Collection or CollectionKind.Enumerable or CollectionKind.GenericEnumerable)
                    return true;

                return SourceType.TryReadProperty(Collection, nameof(IsFixedSize), out bool value)
                    ? value
                    : IsReadOnly;
            }
        }

        /// <inheritdoc cref="IList" />
        public bool IsReadOnly
        {
            get
            {
                if (SourceType.TryReadProperty(Collection, nameof(IsReadOnly), out bool value))
                    return value;

                return false;
            }
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                if (SourceType.TryReadProperty(Collection, nameof(Count), out int value))
                    return value;

                var enumerator = GetEnumerator();
                var result = 0;
                while (enumerator.MoveNext())
                    result++;

                return result;
            }
        }

        /// <inheritdoc />
        public bool IsSynchronized
        {
            get
            {
                if (SourceType.TryReadProperty(Collection, nameof(IsSynchronized), out bool value))
                    return value;

                return false;
            }
        }

        /// <inheritdoc />
        public object SyncRoot
        {
            get
            {
                return SourceType.TryReadProperty(Collection, nameof(SyncRoot), out object value)
                    ? value
                    : _syncRoot;
            }
        }

        /// <inheritdoc />
        public ICollection Keys
        {
            get
            {
                if (Collection is IDictionary dictionary)
                    return dictionary.Keys;


                if (!IsDictionary)
                    return Enumerable.Range(0, Count).ToArray();

                var result = new List<dynamic>(256);
                foreach (var key in Collection.Keys)
                    result.Add(key);

                return result;
            }
        }

        /// <inheritdoc />
        public ICollection Values
        {
            get
            {
                if (Collection is IDictionary dictionary)
                    return dictionary.Values;

                var result = new List<dynamic?>(256);
                if (IsDictionary)
                {
                    foreach (var value in Collection.Values)
                        result.Add(value);
                }
                else
                {
                    var enumerator = GetEnumerator();
                    while (enumerator.MoveNext())
                        result.Add(enumerator.Current);
                }

                return result;
            }
        }

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
            get
            {
                if (!TypeManager.TryChangeType(key, KeysType, out dynamic? keyItem))
                    throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(key));

                return CollectionKind is CollectionKind.Dictionary
                    ? Collection[keyItem]
                    : this[(int)keyItem!];
            }
            set
            {
                if (IsReadOnly)
                    throw new InvalidOperationException("Unable to write to read-only collection.");

                if (CollectionKind is CollectionKind.Dictionary)
                {
                    if (!TypeManager.TryChangeType(key, KeysType, out var keyItem))
                        throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(key));

                    if (!TypeManager.TryChangeType(value, ValuesType, out var valueItem))
                        throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(value));

                    Collection[keyItem] = valueItem;
                    return;
                }
                else if (key is int index)
                {
                    this[index] = value;
                    return;
                }

                throw new ArgumentException($"Key is of an invalid type for this collection kind '{CollectionKind}'.", nameof(key));
            }
        }

        /// <inheritdoc />
        public object? this[int index]
        {
            get
            {
                if (CollectionKind is CollectionKind.List or CollectionKind.GenericList)
                    return Collection[index];

                if (IsDictionary)
                {
                    var currentIndex = -1;
                    foreach (var value in Collection.Values)
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
                if (IsReadOnly)
                    throw new InvalidOperationException("Collection is read-only.");

                if (CollectionKind is CollectionKind.List or CollectionKind.GenericList)
                {
                    if (!TypeManager.TryChangeType(value, ValuesType, out var item))
                        throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(value));

                    Collection[index] = item;
                }

                throw new NotSupportedException("Collection does not support setting a value via indexer.");
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

            if (target is null or not IEnumerable)
                return false;

            var typeProxy = target.GetType().TypeInfo().Collection;
            if (typeProxy is null)
                return false;

            proxy = new(typeProxy, target);
            return true;

        }

        /// <inheritdoc />
        public IEnumerator GetEnumerator() => (Collection as IEnumerable)!.GetEnumerator();

        /// <inheritdoc />
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            if (Collection is IDictionary dictionary)
                return dictionary.GetEnumerator();

            throw new NotSupportedException($"Collection of kind {CollectionKind} does not support dictionary enumerators.");
        }

        /// <inheritdoc />
        public int Add(object? value)
        {
            if (IsDictionary || IsFixedSize || IsReadOnly)
                throw new InvalidOperationException($"Collection of kind {CollectionKind} does not support the {nameof(Add)} operation.");

            if (CollectionKind is CollectionKind.GenericCollection or CollectionKind.List or CollectionKind.GenericList)
            {
                if (TypeManager.TryChangeType(value, ValuesType, out var item))
                {
                    Collection.Add(item);
                    return Count - 1;
                }
                else
                {
                    throw new ArgumentException($"Unable to convert value into type {ValuesType.ShortName}", nameof(value));
                }
            }

            throw new NotSupportedException($"Collection of kind {CollectionKind} does not support the {nameof(Add)} operation.");
        }

        /// <inheritdoc />
        public void Add(object key, object? value)
        {
            if (!IsDictionary || IsFixedSize || IsReadOnly)
                throw new NotSupportedException($"Collection of kind {CollectionKind} does not support the {nameof(Add)} operation.");

            if (TypeManager.TryChangeType(value, ValuesType, out var itemValue) &&
                TypeManager.TryChangeType(key, KeysType, out var itemKey))
                Collection.Add(itemKey, itemValue);
            else
                throw new ArgumentException($"Unable to convert key and/or value to a suitable type.", nameof(value));
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
            if (IsFixedSize || IsReadOnly)
                throw new InvalidOperationException($"Unable to clear collection of kind {CollectionKind} because it is read-only or fixed size.");

            if (CollectionKind is not (CollectionKind.GenericCollection or CollectionKind.List or
                CollectionKind.GenericList or CollectionKind.Dictionary or CollectionKind.GenericDictionary))
            {
                throw new NotSupportedException(
                    $"Collection of kind {CollectionKind} does not support the {nameof(Clear)} operation.");
            }

            Collection.Clear();
        }

        /// <inheritdoc cref="IList" />
        public bool Contains(object? value)
        {
            switch (CollectionKind)
            {
                case CollectionKind.Dictionary:
                    return Collection.Contains(value as dynamic);
                case CollectionKind.GenericDictionary:
                    {
                        if (!TypeManager.TryChangeType(value, KeysType, out var item))
                            throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(value));

                        return Collection.ContainsKey(item);
                    }
                default:
                    return IndexOf(value) >= 0;
            }
        }

        /// <summary>
        /// For dictionaries, it returns the same as <see cref="Contains(object?)"/>.
        /// For other collections, an integer is expected and determines if such value is
        /// within a valid range.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool ContainsKey(object? value)
        {
            if (CollectionKind is CollectionKind.Dictionary)
                return Contains(value);

            if (!TypeManager.TryChangeType(value, KeysType, out var item))
                throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(value));

            return item >= 0 && item < Count;
        }

        /// <inheritdoc />
        public int IndexOf(object? value)
        {
            var index = -1;

            if (IsDictionary)
            {
                if (!TypeManager.TryChangeType(value, KeysType, out var item))
                    throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(value));

                foreach (dynamic? key in Collection.Keys)
                {
                    index++;
                    if (object.Equals(key, item))
                        return index;
                }
            }
            else
            {
                if (!TypeManager.TryChangeType(value, ValuesType, out var item))
                    throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(value));

                var enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {

                    index++;
                    if (object.Equals(enumerator.Current as dynamic, item))
                        return index;
                }
            }

            return -1;
        }

        /// <inheritdoc />
        public void Insert(int index, object? value)
        {
            if (IsDictionary || IsFixedSize || IsReadOnly)
                throw new InvalidOperationException($"Collection of kind {CollectionKind} does not support the {nameof(Insert)} operation.");

            if (CollectionKind is not (CollectionKind.List or CollectionKind.GenericList))
            {
                throw new NotSupportedException(
                    $"Collection of kind {CollectionKind} does not support the {nameof(Insert)} operation.");
            }

            if (!TypeManager.TryChangeType(value, ValuesType, out var item))
                throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(value));

            Collection.Insert(index, item);
        }

        /// <inheritdoc cref="IList" />
        public void Remove(object? value)
        {
            if (IsFixedSize || IsReadOnly)
                throw new InvalidOperationException($"Collection of kind {CollectionKind} does not support the {nameof(Remove)} operation.");

            if (IsDictionary)
            {
                if (TypeManager.TryChangeType(value, KeysType, out var keyItem))
                    Collection.Remove(keyItem);
                else
                    throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(value));
                return;
            }

            if (CollectionKind is not (CollectionKind.List or CollectionKind.GenericList or CollectionKind.GenericCollection))
            {
                throw new NotSupportedException(
                    $"Collection of kind {CollectionKind} does not support the {nameof(Remove)} operation.");
            }

            if (!TypeManager.TryChangeType(value, ValuesType, out var item))
                throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(value));

            Collection.Remove(item);
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            if (IsFixedSize || IsReadOnly)
                throw new InvalidOperationException($"Collection of kind {CollectionKind} does not support the {nameof(RemoveAt)} operation.");

            if (IsDictionary)
            {
                var keyIndex = -1;
                foreach (var key in Keys)
                {
                    keyIndex++;
                    if (keyIndex != index)
                        continue;

                    Collection.Remove(key as dynamic);
                    return;
                }

                throw new ArgumentOutOfRangeException(nameof(index), "Index must be greater than 0 and less than the count.");
            }

            if (CollectionKind is CollectionKind.GenericCollection)
            {
                Remove(this[index]);
                return;
            }

            if (CollectionKind is not (CollectionKind.List or CollectionKind.GenericList))
                throw new NotSupportedException(
                    $"Collection of kind {CollectionKind} does not support the {nameof(RemoveAt)} operation.");

            Collection.RemoveAt(index);
        }

        /// <inheritdoc />
        public void CopyTo(Array array, int index)
        {
            if (array is null)
                throw new ArgumentNullException(nameof(array));

            if (index < 0 || index >= array.Length)
                throw new ArgumentOutOfRangeException(nameof(index));

            var elementType = array.GetType().GetElementType();

            if (elementType is null)
                throw new ArgumentException($"Unable to obtain array element type.", nameof(array));

            var arrayIndex = index;

            if (IsDictionary)
            {
                foreach (var value in Collection.Values)
                {
                    if (!TypeManager.TryChangeType(value, elementType, out dynamic? item))
                        throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(array));

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
                    if (!TypeManager.TryChangeType(enumerator.Current, elementType, out dynamic? item))
                        throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(array));

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
        public dynamic? Last() => this[Count - 1];

        /// <summary>
        /// Gets the last item in the <see cref="Values"/> collection.
        /// If the last item does not exist, it returns the default value for <see cref="ValuesType"/>.
        /// </summary>
        /// <returns>The value in the last position within the collection.</returns>
        public dynamic? LastOrDefault()
        {
            var count = Count;
            if (count <= 0) return ValuesType.DefaultValue;

            return this[count - 1] ?? ValuesType.DefaultValue;
        }

        /// <summary>
        /// Gets the first item in the <see cref="Values"/> collection.
        /// </summary>
        /// <returns>The value in the first position within the collection.</returns>
        public dynamic? First() => this[0];

        /// <summary>
        /// Gets the first item in the <see cref="Values"/> collection.
        /// If the first item does not exist, it returns the default value for <see cref="ValuesType"/>.
        /// </summary>
        /// <returns>The value in the first position within the collection.</returns>
        public dynamic? FirstOrDefault()
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
            var result = TypeManager.CreateArray(ValuesType, Count);

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
                    throw new InvalidCastException("Unable to cast value to a suitable type.");

                if (changedValue is not T typedValue)
                    throw new InvalidCastException("Unable to cast value to a suitable type.");

                result[index] = typedValue;
            }

            return result;
        }

        /// <summary>
        /// Converts the items in the <see cref="Values"/> to a typed list.
        /// </summary>
        /// <typeparam name="T">The target element type.</typeparam>
        /// <returns>A list of values.</returns>
        public List<T> ToList<T>() => new(ToArray<T>());
    }
}
