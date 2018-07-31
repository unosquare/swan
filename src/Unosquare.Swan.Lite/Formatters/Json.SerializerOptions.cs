namespace Unosquare.Swan.Formatters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Attributes;

    /// <summary>
    /// A very simple, light-weight JSON library written by Mario
    /// to teach Geo how things are done
    /// 
    /// This is an useful helper for small tasks but it doesn't represent a full-featured
    /// serializer such as the beloved Json.NET
    /// </summary>
    public partial class Json
    {
        private class SerializerOptions
        {
            private static readonly Dictionary<Type, Dictionary<Tuple<string, string>, Func<object, object>>>
                TypeCache = new Dictionary<Type, Dictionary<Tuple<string, string>, Func<object, object>>>();

            private readonly string[] _includeProperties;
            private readonly string[] _excludeProperties;
            private readonly bool _includeNonPublic;
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
                _includeNonPublic = includeNonPublic;

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

            internal Dictionary<string, Func<object, object>> GetProperties(Type targetType)
                => GetPropertiesCache(targetType)
                    .When(() => _includeProperties?.Length > 0,
                        query => query.Where(p => _includeProperties.Contains(p.Key.Item1)))
                    .When(() => _excludeProperties?.Length > 0,
                        query => query.Where(p => !_excludeProperties.Contains(p.Key.Item1)))
                    .ToDictionary(x => x.Key.Item2, x => x.Value);

            private Dictionary<Tuple<string, string>, Func<object, object>> GetPropertiesCache(Type targetType)
            {
                if (TypeCache.ContainsKey(targetType))
                    return TypeCache[targetType];

                var fields = new List<MemberInfo>();

                // If the target is a struct (value type) navigate the fields.
                if (targetType.IsValueType())
                {
                    fields.AddRange(FieldTypeCache.RetrieveAllFields(targetType));
                }

                // then incorporate the properties
                fields.AddRange(PropertyTypeCache.RetrieveAllProperties(targetType).Where(p => p.CanRead).ToArray());

                TypeCache[targetType] = fields
                    .ToDictionary(
                        x => new Tuple<string, string>(x.Name,
                            x.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? x.Name),
                        x => new Func<object, object>(target =>
                            x is PropertyInfo info
                                ? info.GetGetMethod(_includeNonPublic)?.Invoke(target, null)
                                : (x as FieldInfo).GetValue(target)));

                return TypeCache[targetType];
            }
        }
    }
}