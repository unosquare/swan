﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Swan.Reflection
{
    public sealed class CollectionProxy : IList, IDictionary
    {
        private CollectionProxy(CollectionTypeProxy info, dynamic target)
        {
            Info = info;
            Target = target;
            Kind = Info.CollectionKind;
        }

        /// <summary>
        /// Gets the collection kind for this collection proxy.
        /// </summary>
        public CollectionKind Kind { get; }

        /// <summary>
        /// Gets the collection metadata supporting
        /// the operations of this wrapper.
        /// </summary>
        public CollectionTypeProxy Info { get; }

        /// <summary>
        /// Gets the underlying collection object this wrapper operates on.
        /// </summary>
        public dynamic Target { get; }

        /// <inheritdoc />
        public bool IsFixedSize
        {
            get
            {
                if (Info.OwnerProxy.ProxiedType.IsArray)
                    return true;

                if (Kind == CollectionKind.Dictionary || Kind == CollectionKind.List)
                    return Target.IsFixedSize;

                return Kind switch
                {
                    CollectionKind.Collection => true,
                    CollectionKind.Enumerable => true,
                    CollectionKind.GenericEnumerable => true,
                    CollectionKind.ReadOnlyCollection => true,
                    CollectionKind.ReadOnlyDictionary => true,
                    CollectionKind.ReadOnlyList => true,
                    _ => false
                };
            }
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get
            {
                return Kind switch
                {
                    CollectionKind.ReadOnlyCollection => true,
                    CollectionKind.ReadOnlyDictionary => true,
                    CollectionKind.ReadOnlyList => true,
                    _ => false
                };
            }
        }

        /// <inheritdoc />
        public int Count
        {
            get
            {
                if (Kind == CollectionKind.Enumerable ||
                    Kind == CollectionKind.GenericEnumerable)
                {
                    var enumerator = GetEnumerator();
                    var result = 0;
                    while (enumerator.MoveNext())
                        result++;

                    return result;
                }

                return Target.Count;
            }
        }

        /// <inheritdoc />
        public bool IsSynchronized => Target is ICollection collection ? collection.IsSynchronized : false;

        /// <inheritdoc />
        public object SyncRoot => Target is ICollection collection ? collection.SyncRoot : Target;

        /// <inheritdoc />
        public ICollection Keys
        {
            get
            {
                if (Target is IDictionary dictionary)
                    return dictionary.Keys;


                if (Info.IsDictionary)
                {
                    var result = new List<dynamic>(256);
                    foreach (var key in Target.Keys)
                        result.Add(key);

                    return result;
                }

                return Enumerable.Range(0, Count).ToArray();
            }
        }

        /// <inheritdoc />
        public ICollection Values
        {
            get
            {
                if (Target is IDictionary dictionary)
                    return dictionary.Values;

                var result = new List<dynamic>(256);
                if (Info.IsDictionary)
                {
                    foreach (var value in Target.Values)
                        result.Add(value);
                }
                else
                {
                    var enumerator = GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        result.Add(enumerator.Current);
                    }
                }

                return result;
            }
        }

        /// <inheritdoc />
        public object? this[object key]
        {
            get
            {
                if (!TypeManager.TryChangeType(key, Info.KeysType, out var keyItem))
                    throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(key));

                if (Kind == CollectionKind.Dictionary)
                    return Target[keyItem];

                return this[keyItem];

            }
            set
            {
                if (IsReadOnly)
                    throw new InvalidOperationException("Unable to write to read-only collection.");

                if (Kind == CollectionKind.Dictionary)
                {
                    if (!TypeManager.TryChangeType(key, Info.KeysType, out var keyItem))
                        throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(key));

                    if (!TypeManager.TryChangeType(value, Info.ValuesType, out var valueItem))
                        throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(value));

                    Target[keyItem] = valueItem;
                    return;
                }
                else if (key is int index)
                {
                    this[index] = value;
                    return;
                }

                throw new ArgumentException($"Key is of an invalid type for this collection kind '{Kind}'.", nameof(key));
            }
        }

        /// <inheritdoc />
        public object? this[int index]
        {
            get
            {
                if (Kind == CollectionKind.List || Kind == CollectionKind.GenericList || Kind == CollectionKind.ReadOnlyList)
                    return Target[index];

                if (Info.IsDictionary)
                {
                    var currentIndex = 0;
                    foreach (var value in Target.Values)
                    {
                        if (currentIndex == index)
                            return value;

                        currentIndex++;
                    }
                }
                else
                {
                    var currentIndex = 0;
                    var enumerator = GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        currentIndex++;
                        if (currentIndex == index)
                            return enumerator.Current;
                    }
                }

                throw new IndexOutOfRangeException($"Collection does not contain index {index}.");
            }
            set
            {
                if (IsReadOnly)
                    throw new InvalidOperationException("Collection is read-only.");

                if (Kind == CollectionKind.List || Kind == CollectionKind.GenericList)
                {
                    if (!TypeManager.TryChangeType(value, Info.ValuesType, out var item))
                        throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(value));

                    Target[index] = item;
                }

                throw new NotSupportedException("Collection does not support setting a value via indexer.");
            }
        }

        /// <summary>
        /// Tries to create a collection proxy for the given target object.
        /// </summary>
        /// <param name="target">The target to create the collection proxy for.</param>
        /// <param name="proxy">The resulting proxy.</param>
        /// <returns>True of the operation succeeds. False otherwise.</returns>
        public static bool TryCreate(object target, out CollectionProxy? proxy)
        {
            proxy = default;
            if (target is null || target is not IEnumerable)
                return false;

            var typeProxy = target.GetType().TypeInfo().Collection;
            if (typeProxy is not null)
            {
                proxy = new(typeProxy, target);
                return true;
            }

            return false;
        }

        /// <inheritdoc />
        public IEnumerator GetEnumerator() => (Target as IEnumerable)!.GetEnumerator();

        /// <inheritdoc />
        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            if (Target is IDictionary dictionary)
                return dictionary.GetEnumerator();

            throw new NotSupportedException($"Callection of kind {Kind} does not support dictionary enumerators.");
        }

        /// <inheritdoc />
        public int Add(object? value)
        {
            if (Info.IsDictionary || IsFixedSize || IsReadOnly)
                throw new InvalidOperationException($"Collection of kind {Kind} does not support the {nameof(Add)} operation.");

            if (Kind == CollectionKind.GenericCollection || Kind == CollectionKind.List || Kind == CollectionKind.GenericList)
            {
                if (TypeManager.TryChangeType(value, Info.ValuesType, out var item))
                {
                    Target.Add(item);
                    return Count - 1;
                }
                else
                {
                    throw new ArgumentException($"Unable to convert value into type {Info.ValuesType.ProxiedType}", nameof(value));
                }
            }

            throw new NotSupportedException($"Collection of kind {Kind} does not support the {nameof(Add)} operation.");
        }

        /// <inheritdoc />
        public void Add(object key, object? value)
        {
            if (!Info.IsDictionary || IsFixedSize || IsReadOnly)
                throw new NotSupportedException($"Collection of kind {Kind} does not support the {nameof(Add)} operation.");

            if (TypeManager.TryChangeType(value, Info.ValuesType, out var itemValue) &&
                TypeManager.TryChangeType(key, Info.KeysType, out var itemKey))
                Target.Add(itemKey, itemValue);
            else
                throw new ArgumentException($"Unable to convert key and/or value to a suitable type.", nameof(value));
        }

        /// <inheritdoc />
        public void Clear()
        {
            if (IsFixedSize || IsReadOnly)
                throw new InvalidOperationException($"Unable to clear collection of kind {Kind} because it is read-only or fixed size.");

            if (Kind == CollectionKind.GenericCollection ||
                Kind == CollectionKind.List ||
                Kind == CollectionKind.GenericList ||
                Kind == CollectionKind.Dictionary ||
                Kind == CollectionKind.GenericDictionary)
            {
                Target.Clear();
                return;
            }

            throw new NotSupportedException($"Collection of kind {Kind} does not support the {nameof(Clear)} operation.");
        }

        /// <inheritdoc />
        public bool Contains(object? value)
        {
            if (Kind == CollectionKind.Dictionary)
            {
                return Target.Contains(value as dynamic);
            }
            else if (Kind == CollectionKind.GenericDictionary || Kind == CollectionKind.ReadOnlyDictionary)
            {
                if (!TypeManager.TryChangeType(value, Info.KeysType, out var item))
                    throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(value));

                return Target.ContainsKey(item);
            }
            else
            {
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
            if (Kind == CollectionKind.Dictionary)
                return Contains(value);

            if (!TypeManager.TryChangeType(value, Info.KeysType, out var item))
                throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(value));

            return item >= 0 && item < Count;
        }

        /// <inheritdoc />
        public int IndexOf(object? value)
        {
            var index = -1;

            if (Info.IsDictionary)
            {
                if (!TypeManager.TryChangeType(value, Info.KeysType, out var item))
                    throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(value));

                foreach (var key in Target.Keys)
                {
                    index++;
                    if (key == item)
                        return index;
                }
            }
            else
            {
                if (!TypeManager.TryChangeType(value, Info.ValuesType, out var item))
                    throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(value));

                var enumerator = GetEnumerator();
                while (enumerator.MoveNext())
                {
                    index++;
                    if (enumerator.Current == item)
                        return index;
                }
            }

            return -1;
        }

        /// <inheritdoc />
        public void Insert(int index, object? value)
        {
            if (Info.IsDictionary || IsFixedSize || IsReadOnly)
                throw new InvalidOperationException($"Collection of kind {Kind} does not support the {nameof(Insert)} operation.");

            if (Kind == CollectionKind.List || Kind == CollectionKind.GenericList)
            {
                if (TypeManager.TryChangeType(value, Info.ValuesType, out var item))
                    Target.Insert(index, item);
                else
                    throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(value));
                return;
            }

            throw new NotSupportedException($"Collection of kind {Kind} does not support the {nameof(Insert)} operation.");
        }

        /// <inheritdoc />
        public void Remove(object? value)
        {
            if (IsFixedSize || IsReadOnly)
                throw new InvalidOperationException($"Collection of kind {Kind} does not support the {nameof(Remove)} operation.");

            if (Info.IsDictionary)
            {
                if (TypeManager.TryChangeType(value, Info.KeysType, out var item))
                    Target.Remove(item);
                else
                    throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(value));
                return;
            }
            else if (Kind == CollectionKind.List || Kind == CollectionKind.GenericList)
            {
                if (TypeManager.TryChangeType(value, Info.ValuesType, out var item))
                    Target.Remove(item);
                else
                    throw new ArgumentException($"Unable to cast value to a suitable type.", nameof(value));
                return;
            }

            throw new NotSupportedException($"Collection of kind {Kind} does not support the {nameof(Remove)} operation.");
        }

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            if (IsFixedSize || IsReadOnly)
                throw new InvalidOperationException($"Collection of kind {Kind} does not support the {nameof(RemoveAt)} operation.");

            if (Info.IsDictionary)
            {
                var keyIndex = -1;
                foreach (var key in Keys)
                {
                    keyIndex++;
                    if (keyIndex == index)
                    {
                        Target.Remove(key as dynamic);
                        return;
                    }
                }

                throw new ArgumentOutOfRangeException(nameof(index), "Index must be greater than 0 and less than the count.");
            }

            if (Kind == CollectionKind.List || Kind == CollectionKind.GenericList)
            {
                Target.RemoveAt(index);
                return;
            }

            throw new NotSupportedException($"Collection of kind {Kind} does not support the {nameof(RemoveAt)} operation.");
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

            if (Info.IsDictionary)
            {
                foreach (var value in Target.Values)
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
    }
}
