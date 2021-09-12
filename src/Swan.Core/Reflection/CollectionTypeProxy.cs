using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Swan.Reflection
{
    /// <summary>
    /// Provides metho
    /// </summary>
    public sealed class CollectionTypeProxy
    {
        private static readonly ITypeProxy DefaultKeysType = typeof(int).TypeInfo();
        private static readonly ITypeProxy ObjectTypeInfo = typeof(object).TypeInfo();

        private CollectionTypeProxy(ITypeProxy typeProxy)
        {
            OwnerProxy = typeProxy;
        }

        /// <summary>
        /// Gets the type proxy that generated and owns this collection type proxy.
        /// </summary>
        public ITypeProxy OwnerProxy { get; }

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

        /// <summary>
        /// Gets a value indicating that the collection type is a dictionary.
        /// This specifies that the <see cref="KeysType"/> might not be <see cref="int"/>.
        /// </summary>
        public bool IsDictionary { get; private set; }

        internal static CollectionTypeProxy? Create(ITypeProxy typeProxy)
        {
            var result = new CollectionTypeProxy(typeProxy);

            if (typeProxy.ProxiedType.IsArray)
            {
                result.CollectionType = typeProxy;
                result.KeysType = DefaultKeysType;
                result.ValuesType = typeProxy.ProxiedType.GetElementType()?.TypeInfo() ?? ObjectTypeInfo;
                result.CollectionKind = CollectionKind.List;
            }
            else if (TryGetImplementation(typeProxy, typeof(IReadOnlyDictionary<,>), "IReadOnlyDictionary", 2, out var collectionType))
            {
                result.CollectionType = collectionType!;
                result.KeysType = collectionType!.GenericTypeArguments[0];
                result.ValuesType = collectionType.GenericTypeArguments[1];
                result.CollectionKind = CollectionKind.ReadOnlyDictionary;
                result.IsDictionary = true;
            }
            else if (TryGetImplementation(typeProxy, typeof(IDictionary<,>), nameof(IDictionary), 2, out collectionType))
            {
                result.CollectionType = collectionType!;
                result.KeysType = collectionType!.GenericTypeArguments[0];
                result.ValuesType = collectionType.GenericTypeArguments[1];
                result.CollectionKind = CollectionKind.GenericDictionary;
                result.IsDictionary = true;
            }
            else if (TryGetImplementation(typeProxy, typeof(IDictionary), nameof(IDictionary), 0, out collectionType))
            {
                result.CollectionType = collectionType!;
                result.KeysType = ObjectTypeInfo;
                result.ValuesType = ObjectTypeInfo;
                result.CollectionKind = CollectionKind.Dictionary;
                result.IsDictionary = true;
            }
            else if (TryGetImplementation(typeProxy, typeof(IReadOnlyList<>), "IReadOnlyList", 1, out collectionType))
            {
                result.CollectionType = collectionType!;
                result.KeysType = DefaultKeysType;
                result.ValuesType = collectionType!.GenericTypeArguments[0];
                result.CollectionKind = CollectionKind.GenericList;
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
                result.ValuesType = ObjectTypeInfo;
                result.CollectionKind = CollectionKind.List;
            }
            else if (TryGetImplementation(typeProxy, typeof(IReadOnlyCollection<>), "IReadOnlyCollection", 1, out collectionType))
            {
                result.CollectionType = collectionType!;
                result.KeysType = DefaultKeysType;
                result.ValuesType = collectionType!.GenericTypeArguments[0];
                result.CollectionKind = CollectionKind.GenericCollection;
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
                result.ValuesType = ObjectTypeInfo;
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
                result.ValuesType = ObjectTypeInfo;
                result.CollectionKind = CollectionKind.Enumerable;
            }

            if (result.CollectionKind == CollectionKind.None)
                return null;

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
