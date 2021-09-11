using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Swan.Reflection
{
    /// <summary>
    /// Provides metho
    /// </summary>
    public sealed class CollectionTypeProxy
    {
        private static ITypeProxy DefaultKeysType = typeof(int).TypeInfo();

        private static CollectionKind[] FixedSizeKinds = new[] {
            CollectionKind.Array,
            CollectionKind.Enumerable,
            CollectionKind.GenericEnumerable,
            CollectionKind.Collection,
            CollectionKind.ReadOnlyDictionary
        };

        private static CollectionKind[] DictionaryKinds = new[] {
            CollectionKind.GenericDictionary,
            CollectionKind.Dictionary,
            CollectionKind.ReadOnlyDictionary
        };

        private readonly ITypeProxy Proxy;

        private IList EmptyValues;
        private IList EmptyKeys;

        private CollectionTypeProxy(ITypeProxy typeProxy)
        {
            Proxy = typeProxy;
        }

        /// <summary>
        /// Gets the underlying collection kind.
        /// </summary>
        public CollectionKind CollectionKind { get; private set; }

        /// <summary>
        /// Gets the underlying constructed interface tpye
        /// that this collection omplements.
        /// </summary>
        public ITypeProxy CollectionType { get; private set; }

        /// <summary>
        /// Gets the type proxy for keys in dictionaries.
        /// For non-dictionaries, returns the proxy for <see cref="int"/>.
        /// </summary>
        public ITypeProxy KeysType { get; private set; }

        /// <summary>
        /// Gets the type proxy for values in the collection.
        /// For dictionaries it gets the value type in the key-value pairs.
        /// For other collection types, it returns the item type.
        /// </summary>
        public ITypeProxy ValuesType { get; private set; }

        public bool IsFixedSize { get; private set; }

        public bool IsDictionary { get; private set; }

        public bool HasReadIndexer { get; private set; }

        public bool HasCount { get; private set; }

        public bool CanAdd { get; private set; }

        /// <summary>
        /// Gets the enumerator for the provided intance.
        /// </summary>
        /// <param name="instance">The collection instance.</param>
        /// <returns>The enumerator for the collection.</returns>
        public IEnumerator GetEnumerator(object instance)
        {
            if (instance is not IEnumerable enumerable || CollectionKind == CollectionKind.None)
                throw new InvalidCastException($"Parameter '{nameof(instance)}' does not implement {nameof(IEnumerable)}");

            return enumerable.GetEnumerator();
        }

        /// <summary>
        /// Gets the number of elements contained in the the collection.
        /// </summary>
        /// <param name="instance">The collection instance.</param>
        /// <returns>The number of elements in the collection.</returns>
        public int Count(object? instance)
        {
            if (instance is null)
                return default;

            if (instance is Array array)
                return array.Length;

            if (HasCount)
            {
                dynamic collection = instance;
                return collection.Count;
            }

            var enumerator = GetEnumerator(instance);
            var result = 0;
            while (enumerator.MoveNext())
                result++;

            return result;
        }

        /// <summary>
        /// Gets all the keys in the collection. For dictionaries, the list of keys.
        /// For other collection types, the indices.
        /// </summary>
        /// <param name="instance">The collection instance.</param>
        /// <returns>The enumerable keys.</returns>
        public IList GetKeys(object? instance)
        {
            if (instance is null)
                return EmptyKeys;

            if (!IsDictionary)
                return Enumerable.Range(0, Count(instance)).ToArray();

            var result = new List<object?>(256);
            if (((dynamic)instance).Keys is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                    result.Add(item);
            }

            return result;
        }

        /// <summary>
        /// Gets all the values in the collection.
        /// </summary>
        /// <param name="instance">The collection instance.</param>
        /// <returns>The enumerable values.</returns>
        public IList GetValues(object? instance)
        {
            if (instance is null)
                return EmptyValues;

            var result = new List<object?>(256);

            if (IsDictionary)
            {
                if (((dynamic)instance).Values is IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                        result.Add(item);
                }
            }
            else
            {
                var enumerator = GetEnumerator(instance);
                while (enumerator.MoveNext())
                    result.Add(enumerator.Current);
            }

            return result;
        }

        public bool TryAdd(object? instance, object? value)
        {
            if (instance is null || !CanAdd || IsDictionary)
                return false;

            dynamic collection = instance;
            if (!TypeManager.TryChangeType(value, ValuesType, out var item))
                return false;

            collection.Add(item);
            return true;
        }

        public bool TryAdd(object? instance, object key, object? value)
        {
            if (instance is null || !IsDictionary || !CanAdd)
                return false;

            dynamic collection = instance;

            if (!TypeManager.TryChangeType(value, KeysType, out var itemKey))
                return false;

            if (!TypeManager.TryChangeType(value, ValuesType, out var itemValue))
                return false;

            collection.Add(itemKey, itemValue);
            return true;
        }

        public void Clear(object? instance)
        {

        }

        public void Remove(object? instance)
        {

        }

        public bool TryGetItem(object? instance, object key, out object? value)
        {
            value = default;
            if (instance is null)
                return false;

            if (IsDictionary)
            {
                if (!TypeManager.TryChangeType(key, KeysType.ProxiedType, out var collectionKey))
                    return false;

                var collection = Cast(instance);
                if (collection is null) return false;
                var collectionValue = collection[collectionKey];
                return TypeManager.TryChangeType(collectionValue, ValuesType.ProxiedType, out value);
            }

            if (!TypeManager.TryChangeType(key, DefaultKeysType, out var index))
                return false;

            return TryGetItem(instance, (int)index!, out value);
        }

        public bool TryGetItem(object? instance, int index, out object? value)
        {
            value = default;
            if (instance is null)
                return false;

            var enumerator = IsDictionary ? GetValues(instance).GetEnumerator() : GetEnumerator(instance);
            var currentIndex = -1;
            while (enumerator.MoveNext())
            {
                currentIndex++;
                if (index == currentIndex)
                    return TypeManager.TryChangeType(enumerator.Current, ValuesType, out value);
            }

            return false;
        }

        public bool TrySetItem(object? instance, object key, object? value)
        {

        }

        public bool TrySetItem(object? instance, int index, object? value)
        {

        }

        internal static CollectionTypeProxy? Create(ITypeProxy typeProxy)
        {
            var result = new CollectionTypeProxy(typeProxy);

            if (typeProxy.ProxiedType.IsArray)
            {
                result.CollectionType = typeProxy;
                result.KeysType = DefaultKeysType;
                result.ValuesType = typeProxy.ProxiedType.GetElementType()?.TypeInfo() ?? TypeManager.ObjectTypeInfo;
                result.CollectionKind = CollectionKind.Array;
            }
            else if (TryGetImplementation(typeProxy, typeof(IReadOnlyDictionary<,>), "IReadOnlyDictionary", 2, out var collectionType))
            {
                result.CollectionType = collectionType!;
                result.KeysType = collectionType!.GenericTypeArguments[0];
                result.ValuesType = collectionType.GenericTypeArguments[1];
                result.CollectionKind = CollectionKind.ReadOnlyDictionary;
            }
            else if (TryGetImplementation(typeProxy, typeof(IDictionary<,>), nameof(IDictionary), 2, out collectionType))
            {
                result.CollectionType = collectionType!;
                result.KeysType = collectionType!.GenericTypeArguments[0];
                result.ValuesType = collectionType.GenericTypeArguments[1];
                result.CollectionKind = CollectionKind.GenericDictionary;
            }
            else if (TryGetImplementation(typeProxy, typeof(IDictionary), nameof(IDictionary), 0, out collectionType))
            {
                result.CollectionType = collectionType!;
                result.KeysType = TypeManager.ObjectTypeInfo;
                result.ValuesType = TypeManager.ObjectTypeInfo;
                result.CollectionKind = CollectionKind.Dictionary;
            }
            else if (TryGetImplementation(typeProxy, typeof(IList<>), nameof(IList), 1, out collectionType))
            {
                result.CollectionType = collectionType!;
                result.KeysType = DefaultKeysType;
                result.ValuesType = collectionType!.GenericTypeArguments[0];
                result.CollectionKind = CollectionKind.GenericList;
            }
            else if (TryGetImplementation(typeProxy, typeof(IList), nameof(IList), 0, out collectionType))
            {
                result.CollectionType = collectionType!;
                result.KeysType = DefaultKeysType;
                result.ValuesType = TypeManager.ObjectTypeInfo;
                result.CollectionKind = CollectionKind.List;
            }
            else if (TryGetImplementation(typeProxy, typeof(ICollection<>), nameof(ICollection), 1, out collectionType))
            {
                result.CollectionType = collectionType!;
                result.KeysType = DefaultKeysType;
                result.ValuesType = collectionType!.GenericTypeArguments[0];
                result.CollectionKind = CollectionKind.GenericCollection;
            }
            else if (TryGetImplementation(typeProxy, typeof(ICollection), nameof(ICollection), 0, out collectionType))
            {
                result.CollectionType = collectionType!;
                result.KeysType = DefaultKeysType;
                result.ValuesType = TypeManager.ObjectTypeInfo;
                result.CollectionKind = CollectionKind.Collection;
            }
            else if (TryGetImplementation(typeProxy, typeof(IEnumerable<>), nameof(IEnumerable), 1, out collectionType))
            {
                result.CollectionType = collectionType!;
                result.KeysType = DefaultKeysType;
                result.ValuesType = collectionType!.GenericTypeArguments[0];
                result.CollectionKind = CollectionKind.GenericEnumerable;
            }
            else if (TryGetImplementation(typeProxy, typeof(IEnumerable), nameof(IEnumerable), 0, out collectionType))
            {
                result.CollectionType = collectionType!;
                result.KeysType = DefaultKeysType;
                result.ValuesType = TypeManager.ObjectTypeInfo;
                result.CollectionKind = CollectionKind.Enumerable;
            }

            if (result.CollectionKind == CollectionKind.None)
                return null;

            result.IsFixedSize = FixedSizeKinds.Contains(result.CollectionKind);
            result.IsDictionary = DictionaryKinds.Contains(result.CollectionKind);
            result.EmptyKeys = Array.CreateInstance(result.KeysType?.ProxiedType ?? DefaultKeysType.ProxiedType, 0);
            result.EmptyValues = Array.CreateInstance(result.ValuesType.ProxiedType, 0);
            result.HasCount = result.CollectionKind != CollectionKind.Enumerable && result.CollectionKind != CollectionKind.GenericEnumerable;
            result.HasReadIndexer = result.HasCount && result.CollectionKind != CollectionKind.Collection && result.CollectionKind != CollectionKind.GenericCollection;
            result.CanAdd =
                result.CollectionKind == CollectionKind.List ||
                result.CollectionKind == CollectionKind.GenericList ||
                result.CollectionKind == CollectionKind.Dictionary ||
                result.CollectionKind == CollectionKind.GenericDictionary;

            return result;
        }

        private static bool TryGetImplementation(ITypeProxy typeProxy, Type interfaceType, string nameMatch, int genericsCount, out ITypeProxy? implementation)
        {
            implementation = genericsCount <= 0
                ? typeProxy.Interfaces.FirstOrDefault(c => c == interfaceType)?.TypeInfo()
                : typeProxy.Interfaces.FirstOrDefault(c =>
                     c.IsGenericType &&
                     c.GenericTypeArguments.Length == genericsCount &&
                     c.Name.StartsWith(nameMatch, StringComparison.Ordinal) &&
                     c.GetGenericTypeDefinition() == interfaceType)?.TypeInfo();

            return implementation is not null;
        }
    }

}
