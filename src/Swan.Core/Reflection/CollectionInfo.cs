namespace Swan.Reflection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// Provides type metadata on a collection type
    /// by taking the most capable collection interface available on the <see cref="TypeInfo"/>
    /// </summary>
    internal sealed class CollectionInfo : ICollectionInfo
    {
        private static readonly ITypeInfo DefaultKeysType = typeof(int).TypeInfo();
        private static readonly ITypeInfo ObjectTypeInfo = typeof(object).TypeInfo();

        public CollectionInfo(ITypeInfo typeProxy)
        {
            SourceType = typeProxy;

            if (typeProxy.IsArray)
            {
                CollectionType = typeof(IList).TypeInfo();
                KeysType = DefaultKeysType;
                ValuesType = typeProxy.NativeType.GetElementType()?.TypeInfo() ?? ObjectTypeInfo;
                CollectionKind = CollectionKind.List;
                return;
            }
            else if (TryGetImplementation(typeProxy, typeof(IDictionary<,>), nameof(IDictionary), 2, out var collectionType))
            {
                CollectionType = collectionType;
                KeysType = collectionType.GenericTypeArguments[0];
                ValuesType = collectionType.GenericTypeArguments[1];
                CollectionKind = CollectionKind.GenericDictionary;
                IsDictionary = true;
                return;
            }
            else if (TryGetImplementation(typeProxy, typeof(IDictionary), nameof(IDictionary), 0, out collectionType))
            {
                CollectionType = collectionType;
                KeysType = ObjectTypeInfo;
                ValuesType = ObjectTypeInfo;
                CollectionKind = CollectionKind.Dictionary;
                IsDictionary = true;
                return;
            }
            else if (TryGetImplementation(typeProxy, typeof(IList<>), nameof(IList), 1, out collectionType))
            {
                CollectionType = collectionType;
                KeysType = DefaultKeysType;
                ValuesType = collectionType.GenericTypeArguments[0];
                CollectionKind = CollectionKind.GenericList;
                return;
            }
            else if (TryGetImplementation(typeProxy, typeof(IList), nameof(IList), 0, out collectionType))
            {
                CollectionType = collectionType;
                KeysType = DefaultKeysType;
                ValuesType = ObjectTypeInfo;
                CollectionKind = CollectionKind.List;
                return;
            }
            else if (TryGetImplementation(typeProxy, typeof(ICollection<>), nameof(ICollection), 1, out collectionType))
            {
                CollectionType = collectionType;
                KeysType = DefaultKeysType;
                ValuesType = collectionType.GenericTypeArguments[0];
                CollectionKind = CollectionKind.GenericCollection;
                return;
            }
            else if (TryGetImplementation(typeProxy, typeof(ICollection), nameof(ICollection), 0, out collectionType))
            {
                CollectionType = collectionType;
                KeysType = DefaultKeysType;
                ValuesType = ObjectTypeInfo;
                CollectionKind = CollectionKind.Collection;
                return;
            }
            else if (TryGetImplementation(typeProxy, typeof(IEnumerable<>), nameof(IEnumerable), 1, out collectionType))
            {
                CollectionType = collectionType;
                KeysType = DefaultKeysType;
                ValuesType = collectionType.GenericTypeArguments[0];
                CollectionKind = CollectionKind.GenericEnumerable;
                return;
            }
            else if (TryGetImplementation(typeProxy, typeof(IEnumerable), nameof(IEnumerable), 0, out collectionType))
            {
                CollectionType = collectionType;
                KeysType = DefaultKeysType;
                ValuesType = ObjectTypeInfo;
                CollectionKind = CollectionKind.Enumerable;
                return;
            }

            CollectionType = typeProxy;
            KeysType = DefaultKeysType;
            ValuesType = ObjectTypeInfo;
            CollectionKind = CollectionKind.None;
        }

        /// <inheritdoc />
        public ITypeInfo SourceType { get; }

        /// <inheritdoc />
        public CollectionKind CollectionKind { get; }

        /// <inheritdoc />
        public ITypeInfo CollectionType { get; }

        /// <inheritdoc />
        public ITypeInfo KeysType { get; }

        /// <inheritdoc />
        public ITypeInfo ValuesType { get; }

        /// <inheritdoc />
        public bool IsDictionary { get; }

        /// <inheritdoc />
        public bool IsArray => SourceType.IsArray;

        private static bool TryGetImplementation(ITypeInfo typeProxy, Type interfaceType, string nameMatch, int genericsCount, [MaybeNullWhen(false)] out ITypeInfo implementation)
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
