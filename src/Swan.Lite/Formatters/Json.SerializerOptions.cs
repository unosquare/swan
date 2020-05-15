using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Swan.Reflection;

namespace Swan.Formatters
{
    /// <summary>
    /// A very simple, light-weight JSON library written by Mario
    /// to teach Geo how things are done
    /// 
    /// This is an useful helper for small tasks but it doesn't represent a full-featured
    /// serializer such as the beloved Json.NET.
    /// </summary>
    public class SerializerOptions
    {
        private static readonly ConcurrentDictionary<Type, Dictionary<Tuple<string, string>, MemberInfo>>
            TypeCache = new ConcurrentDictionary<Type, Dictionary<Tuple<string, string>, MemberInfo>>();

        private readonly string[]? _includeProperties;
        private readonly Dictionary<int, List<WeakReference>> _parentReferences = new Dictionary<int, List<WeakReference>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializerOptions"/> class.
        /// </summary>
        /// <param name="format">if set to <c>true</c> [format].</param>
        /// <param name="typeSpecifier">The type specifier.</param>
        /// <param name="includeProperties">The include properties.</param>
        /// <param name="excludeProperties">The exclude properties.</param>
        /// <param name="includeNonPublic">if set to <c>true</c> [include non public].</param>
        /// <param name="parentReferences">The parent references.</param>
        /// <param name="jsonSerializerCase">The json serializer case.</param>
        public SerializerOptions(
            bool format,
            string? typeSpecifier,
            string[]? includeProperties,
            string[]? excludeProperties = null,
            bool includeNonPublic = true,
            IReadOnlyCollection<WeakReference>? parentReferences = null,
            JsonSerializerCase jsonSerializerCase = JsonSerializerCase.None)
        {
            _includeProperties = includeProperties;

            ExcludeProperties = excludeProperties;
            IncludeNonPublic = includeNonPublic;
            Format = format;
            TypeSpecifier = typeSpecifier;
            JsonSerializerCase = jsonSerializerCase;

            if (parentReferences == null)
                return;

            foreach (var parentReference in parentReferences.Where(x => x.IsAlive))
            {
                IsObjectPresent(parentReference.Target);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="SerializerOptions"/> is format.
        /// </summary>
        /// <value>
        ///   <c>true</c> if format; otherwise, <c>false</c>.
        /// </value>
        public bool Format { get; }

        /// <summary>
        /// Gets the type specifier.
        /// </summary>
        /// <value>
        /// The type specifier.
        /// </value>
        public string? TypeSpecifier { get; }

        /// <summary>
        /// Gets a value indicating whether [include non public].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [include non public]; otherwise, <c>false</c>.
        /// </value>
        public bool IncludeNonPublic { get; }

        /// <summary>
        /// Gets the json serializer case.
        /// </summary>
        /// <value>
        /// The json serializer case.
        /// </value>
        public JsonSerializerCase JsonSerializerCase { get; }

        /// <summary>
        /// Gets or sets the exclude properties.
        /// </summary>
        /// <value>
        /// The exclude properties.
        /// </value>
        public string[]? ExcludeProperties { get; set; }

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
                .When(() => ExcludeProperties?.Length > 0,
                    query => query.Where(p => !ExcludeProperties.Contains(p.Key.Item1)))
                .ToDictionary(x => x.Key.Item2, x => x.Value);
        
        private Dictionary<Tuple<string, string>, MemberInfo> GetPropertiesCache(Type targetType)
        {
            if (TypeCache.TryGetValue(targetType, out var current))
                return current;

            var fields =
                new List<MemberInfo>(PropertyTypeCache.DefaultCache.Value.RetrieveAllProperties(targetType).Where(p => p.CanRead));

            // If the target is a struct (value type) navigate the fields.
            if (targetType.IsValueType)
            {
                fields.AddRange(FieldTypeCache.DefaultCache.Value.RetrieveAllFields(targetType));
            }

            var value = fields
                .Where(x => x.GetCustomAttribute<JsonPropertyAttribute>()?.Ignored != true)
                .ToDictionary(
                    x => Tuple.Create(x.Name,
                        x.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName ?? x.Name.GetNameWithCase(JsonSerializerCase)),
                    x => x);

            TypeCache.TryAdd(targetType, value);

            return value;
        }
    }
}
