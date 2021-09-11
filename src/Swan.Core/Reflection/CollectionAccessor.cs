using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Swan.Reflection
{
    /// <summary>
    /// Provides metho
    /// </summary>
    public sealed class CollectionAccessor
    {
        private readonly ITypeProxy Proxy;
        private readonly CollectionCapabilities Capability;

        internal CollectionAccessor(ITypeProxy typeProxy)
        {
            Proxy = typeProxy;
            
            if (Proxy.ProxiedType.IsArray)
            {
                CollectionType = Proxy;
                ItemType = Proxy.ProxiedType.GetElementType()?.TypeInfo() ?? typeof(object).TypeInfo();
                Capability = CollectionCapabilities.Array;
            }

            if (TryGetImplementation(typeof(IDictionary<,>), nameof(IDictionary), 2, out var genericDictionary))
            {
                CollectionType = genericDictionary!;
                KeyType = genericDictionary.GenericTypeArguments[0];
                ItemType = genericDictionary.GenericTypeArguments[1];
                Capability = CollectionCapabilities.GenericDictionary;
            }

            if (TryGetImplementation(typeof(IDictionary<,>), nameof(IDictionary), 2, out var dictionary))
            {
                CollectionType = genericDictionary!;
                KeyType = genericDictionary.GenericTypeArguments[0];
                ItemType = genericDictionary.GenericTypeArguments[1];
                Capability = CollectionCapabilities.GenericDictionary;
            }

            if (genericDictionary is not null)
            {
                CollectionType = genericDictionary.TypeInfo();
                ItemType
            }

            var genericInterface = Proxy.Interfaces.FirstOrDefault(
                c => c.IsGenericType && c.GetGenericTypeDefinition() == typeof(IDictionary<,>));

            if (genericInterface is null || genericInterface.GenericTypeArguments.Length < 2)
                return null;

            var keyType = genericInterface.GenericTypeArguments[0];
            var valueType = genericInterface.GenericTypeArguments[1];

            return typeof(IDictionary<,>)
                .MakeGenericType(keyType, valueType)
                .TypeInfo();

        }

        public ITypeProxy CollectionType { get; }

        public ITypeProxy KeyType { get; }

        public ITypeProxy ValueType => ItemType;

        public ITypeProxy ItemType { get; }

        public bool IsDictionary { get; }

        /// <summary>
        /// Gets a value indicating whether this type is an array.
        /// </summary>
        public bool IsArray { get; }

        /// <summary>
        /// Returns true for types that implement <see cref="IEnumerable"/>
        /// </summary>
        public bool IsEnumerable { get; }

        /// <summary>
        /// Returns true for types that implement <see cref="IList"/>
        /// </summary>
        public bool IsList { get; }

        public bool IsCollection { get; }

        internal CollectionAccessor? Create(ITypeProxy typeProxy)
        {
            if (!typeProxy.IsEnumerable || typeProxy.GenericDictionaryType is not null)
                return null;
        }

        private bool TryGetImplementation(Type genericType, string match, int genericsCount, out ITypeProxy? implementation)
        {
            var iterfaces = Proxy.Interfaces.FirstOrDefault(c =>
                     c.IsGenericType &&
                     c.GenericTypeArguments.Length == genericsCount &&
                     c.Name.StartsWith(match, StringComparison.Ordinal) &&
                     c.GetGenericTypeDefinition() == genericType)

            implementation = genericType.IsGenericTypeDefinition
                ? Proxy.Interfaces.FirstOrDefault(c =>
                     c.IsGenericType &&
                     c.GenericTypeArguments.Length == genericsCount &&
                     c.Name.StartsWith(match, StringComparison.Ordinal) &&
                     c.GetGenericTypeDefinition() == genericType)?.TypeInfo();

            return implementation is not null;
        }

        private static ITypeProxy? GetItemType(ITypeProxy enumerableType)
        {
            if (!enumerableType.IsEnumerable || enumerableType.GenericDictionaryType is not null)
                return null;

            return enumerableType.IsArray
                ? enumerableType.ElementType
                : enumerableType.GenericCollectionType is not null
                ? enumerableType.GenericTypeArguments[0]
                : typeof(object).TypeInfo();
        }

        private enum CollectionCapabilities
        {
            Array,
            Enumerable,
            GenericEnumerable,
            Collection,
            GenericCollection,
            List,
            GenericList,
            Dictionary,
            GenericDictionary
        }
    }
}
