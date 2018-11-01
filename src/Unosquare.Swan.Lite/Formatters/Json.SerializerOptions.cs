namespace Unosquare.Swan.Formatters
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;
    using Attributes;

    /// <summary>
    /// A very simple, light-weight JSON library written by Mario
    /// to teach Geo how things are done
    /// 
    /// This is an useful helper for small tasks but it doesn't represent a full-featured
    /// serializer such as the beloved Json.NET.
    /// </summary>
    public partial class Json
    {
        private class SerializerOptions
        {
            private static readonly ConcurrentDictionary<Type, Dictionary<Tuple<string, string>, MemberInfo>>
                TypeCache = new ConcurrentDictionary<Type, Dictionary<Tuple<string, string>, MemberInfo>>();

            private readonly string[] _includeProperties;
            private readonly string[] _excludeProperties;
            private readonly Dictionary<int, List<WeakReference>> _parentReferences = new Dictionary<int, List<WeakReference>>();

            public SerializerOptions(
                bool format,
                string typeSpecifier,
                string[] includeProperties,
                string[] excludeProperties = null,
                bool includeNonPublic = true,
                IReadOnlyCollection<WeakReference> parentReferences = null)
            {
                _includeProperties = includeProperties;
                _excludeProperties = excludeProperties;

                IncludeNonPublic = includeNonPublic;
                Format = format;
                TypeSpecifier = typeSpecifier;

                if (parentReferences == null)
                    return;

                foreach (var parentReference in parentReferences.Where(x => x.IsAlive))
                {
                    IsObjectPresent(parentReference.Target);
                }
            }

            public bool Format { get; }
            public string TypeSpecifier { get; }
            public bool IncludeNonPublic { get; }

            internal bool IsObjectPresent(object target)
            {
                var hashCode = target.GetHashCode();

                if (_parentReferences.ContainsKey(hashCode))
                {
                    if (_parentReferences[hashCode].Any(p => ReferenceEquals(p.Target, target)))
                        return true;

                    _parentReferences[hashCode].Add(new WeakReference(target));
                    return false;
                }

                _parentReferences.Add(hashCode, new List<WeakReference> { new WeakReference(target) });
                return false;
            }

            internal Dictionary<string, MemberInfo> GetProperties(Type targetType)
                => GetPropertiesCache(targetType)
                    .When(() => _includeProperties?.Length > 0,
                        query => query.Where(p => _includeProperties.Contains(p.Key.Item1)))
                    .When(() => _excludeProperties?.Length > 0,
                        query => query.Where(p => !_excludeProperties.Contains(p.Key.Item1)))
                    .ToDictionary(x => x.Key.Item2, x => x.Value);

            private static Dictionary<Tuple<string, string>, MemberInfo> GetPropertiesCache(Type targetType)
            {
                if (TypeCache.TryGetValue(targetType, out var current))
                    return current;

                var fields =
                    new List<MemberInfo>(PropertyTypeCache.RetrieveAllProperties(targetType).Where(p => p.CanRead));

                // If the target is a struct (value type) navigate the fields.
                if (targetType.IsValueType())
                {
                    fields.AddRange(FieldTypeCache.RetrieveAllFields(targetType));
                }

                var value = fields
                    .ToDictionary(
                        x => new Tuple<string, string>(x.Name,
                            x.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? x.Name),
                        x => x);

                TypeCache.TryAdd(targetType, value);

                return value;
            }
        }
    }
}