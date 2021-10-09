namespace Swan.Mapping
{
#pragma warning disable CA1031 // Do not catch general exception types
    using Swan.Collections;
    using Swan.Reflection;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// Represents an AutoMapper-like object to map from one object type
    /// to another using defined properties map or using the default behaviour
    /// to copy same named properties from one object to another.
    /// 
    /// The extension methods like CopyPropertiesTo use the default behaviour.
    /// </summary>
    /// <example>
    /// The following code explains how to map an object's properties into an instance of type T. 
    /// <code>
    /// using Swan.Mappers;
    /// 
    /// class Example
    /// {
    ///     class Person
    ///     {
    ///         public string Name { get; set; }
    ///         public int Age { get; set; }
    ///     }
    ///     
    ///     static void Main()
    ///     {
    ///         var obj = new { Name = "John", Age = 42 };
    ///         
    ///         var person = Runtime.ObjectMapper.Map&lt;Person&gt;(obj);
    ///     }
    /// }
    /// </code>
    /// 
    /// The following code explains how to explicitly map certain properties.
    /// <code>
    /// using Swan.Mappers;
    /// 
    /// class Example
    /// {
    ///     class User
    ///     {
    ///         public string Name { get; set; }
    ///         public Role Role { get; set; }
    ///     }
    ///     
    ///     public class Role
    ///     {
    ///         public string Name { get; set; }
    ///     }
    ///     
    ///     class UserDto
    ///     {
    ///         public string Name { get; set; }
    ///         public string Role { get; set; }
    ///     }
    ///     
    ///     static void Main()
    ///     {
    ///         // create a User object
    ///         var person = 
    ///             new User { Name = "Phillip", Role = new Role { Name = "Admin" } };
    ///         
    ///         // create an Object Mapper
    ///         var mapper = new ObjectMapper();
    ///         
    ///         // map the User's Role.Name to UserDto's Role
    ///         mapper.CreateMap&lt;User, UserDto&gt;()
    ///             .MapProperty(d => d.Role, x => x.Role.Name);
    ///         
    ///         // apply the previous map and retrieve a UserDto object
    ///         var destination = mapper.Map&lt;UserDto&gt;(person);
    ///     }
    /// }
    /// </code>
    /// </example>
    public class ObjectMapper
    {
        private static readonly Lazy<ObjectMapper> LazyInstance = new(() => new ObjectMapper());

        private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Type, IObjectMap>> TargetMaps = new();

        /// <summary>
        /// Gets the default instance of the object mapper.
        /// </summary>
        public static ObjectMapper Default => LazyInstance.Value;

        /// <summary>
        /// Copies the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="propertiesToCopy">The properties to copy.</param>
        /// <param name="ignoreProperties">The ignore properties.</param>
        /// <returns>
        /// Copied properties count.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// source
        /// or
        /// target.
        /// </exception>
        public static int Copy(
            object? source,
            object? target,
            IEnumerable<string>? propertiesToCopy = null,
            params string[]? ignoreProperties)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            return CopyInternal(
                target,
                GetSourceMap(source),
                propertiesToCopy,
                ignoreProperties);
        }

        /// <summary>
        /// Copies the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="target">The target.</param>
        /// <param name="propertiesToCopy">The properties to copy.</param>
        /// <param name="ignoreProperties">The ignore properties.</param>
        /// <returns>
        /// Copied properties count.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// source
        /// or
        /// target.
        /// </exception>
        public static int Copy(
            IDictionary<string, object?> source,
            object? target,
            IEnumerable<string>? propertiesToCopy = null,
            params string[] ignoreProperties)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            return CopyInternal(
                target,
                source.ToDictionary(
                    x => x.Key.ToLowerInvariant(),
                    x => Tuple.Create(typeof(object), x.Value)),
                propertiesToCopy,
                ignoreProperties);
        }


        public bool HasMap(Type sourceType, Type targetType) =>
            !TargetMaps.TryGetValue(targetType, out var sourceMaps)
            ? false
            : sourceMaps.ContainsKey(sourceType);

        public bool TryGetMap(Type sourceType, Type targetType, out IObjectMap map)
        {
            if (!TargetMaps.TryGetValue(targetType, out var sourceMaps))
            {
                sourceMaps = new ConcurrentDictionary<Type, IObjectMap>();
                TargetMaps.TryAdd(targetType, sourceMaps);
            }

            return sourceMaps.TryGetValue(sourceType, out map);
        }

        private bool TryGetMap<TSource, TTarget>([MaybeNullWhen(false)] out IObjectMap<TSource, TTarget> map)
        {
            if (TryGetMap(typeof(TSource), typeof(TTarget), out var existingMap) &&
                existingMap is IObjectMap<TSource, TTarget> typedMap)
            {
                map = typedMap;
                return true;
            }

            map = default;
            return false;
        }

        private bool TrySetMap<TSource, TTarget>(IObjectMap map)
        {
            if (map is null)
                throw new ArgumentNullException(nameof(map));

            if (!TargetMaps.TryGetValue(typeof(TTarget), out var sourceMaps))
            {
                sourceMaps = new ConcurrentDictionary<Type, IObjectMap>();
                TargetMaps.TryAdd(typeof(TTarget), sourceMaps);
            }

            if (!sourceMaps.ContainsKey(typeof(TSource)))
            {
                sourceMaps[typeof(TSource)] = map;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Creates a default map between the source and target types
        /// and adds it to this object mapper.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TTarget">The type of the target</typeparam>
        /// <returns>A newly-created default map between the 2 objects.</returns>
        public IObjectMap<TSource, TTarget> AddMap<TSource, TTarget>()
        {
            if (TryGetMap<TSource, TTarget>(out _))
                throw new InvalidOperationException(
                    $"This mapper already defines a map between target '{typeof(TTarget)}' and source '{typeof(TSource)}'");

            if (!TargetMaps.TryGetValue(typeof(TTarget), out var sourceMaps))
            {
                sourceMaps = new ConcurrentDictionary<Type, IObjectMap>();
                TargetMaps.TryAdd(typeof(TTarget), sourceMaps);
            }

            var map = new ObjectMap<TSource, TTarget>(this);
            if (!map.Any())
                throw new InvalidOperationException("Types don't have any matching properties.");

            TrySetMap<TSource, TTarget>(map);

            return map;
        }

        /// <summary>
        /// Retrieves an existing map between target and source types.
        /// If the map already exists, it returns it from this mapper's internal storage.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TTarget">The type of the target</typeparam>
        /// <returns>A map between the 2 objects.</returns>
        public IObjectMap<TSource, TTarget> GetOrAddMap<TSource, TTarget>()
        {
            if (TryGetMap<TSource, TTarget>(out var map))
                return map;

            map = new ObjectMap<TSource, TTarget>(this);
            TrySetMap<TSource, TTarget>(map);

            return map;
        }

        /// <summary>
        /// Maps the specified source.
        /// </summary>
        /// <typeparam name="TTarget">The type of the destination.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="autoResolve">if set to <c>true</c> [automatic resolve].</param>
        /// <returns>
        /// A new instance of the map.
        /// </returns>
        /// <exception cref="ArgumentNullException">source.</exception>
        /// <exception cref="InvalidOperationException">You can't map from type {source.GetType().Name} to {typeof(TDestination).Name}.</exception>
        public TTarget Apply<TTarget>(object source, bool autoResolve = true)
            where TTarget : class, new()
        {
            if (source is null)
                throw new ArgumentNullException(nameof(source));

            var sourceType = source.GetType();
            var targetType = typeof(TTarget);

            var target = new TTarget();

            if (TryGetMap(sourceType, targetType, out var map))
            {
                map.Apply(source, target);
                return target;
            }
            else
            {
                if (!autoResolve)
                {
                    throw new InvalidOperationException(
                        $"You can't map from type {source.GetType().Name} to {typeof(TTarget).Name}");
                }

                // Missing mapping, try to use default behavior
                Copy(source, target);
            }

            return target;
        }

        private static int CopyInternal(
            object target,
            Dictionary<string, Tuple<Type, object?>> sourceProperties,
            IEnumerable<string>? propertiesToCopy,
            IEnumerable<string>? ignoreProperties)
        {
            // Filter properties
            var requiredProperties = propertiesToCopy?
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.ToLowerInvariant());

            var ignoredProperties = ignoreProperties?
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.ToLowerInvariant());

            var properties = target.GetType().Properties().Where(c => c.CanWrite);

            return properties
                .Select(x => x.PropertyName)
                .Distinct()
                .ToDictionary(x => x.ToLowerInvariant(), x => properties.First(y => y.PropertyName == x))
                .Where(x => sourceProperties.ContainsKey(x.Key))
                .When(() => requiredProperties is not null, q => q.Where(y => requiredProperties!.Contains(y.Key)))
                .When(() => ignoredProperties is not null, q => q.Where(y => !ignoredProperties!.Contains(y.Key)))
                .ToDictionary(x => x.Value, x => sourceProperties[x.Key])
                .Sum(x => TrySetValue(x.Key.PropertyInfo, x.Value, target) ? 1 : 0);
        }

        private static bool TrySetValue(PropertyInfo targetProperty, Tuple<Type, object?> sourceProperty, object targetInstance)
        {
            try
            {
                var (type, value) = sourceProperty;

                if (targetProperty.PropertyType.IsArray)
                {
                    targetProperty.TrySetArray(value as IEnumerable<object>, targetInstance);
                    return true;
                }

                if (targetProperty.ToPropertyProxy().TryWrite(targetInstance, value))
                    return true;

            }

            catch
            {
                // swallow
            }

            return false;
        }

        private static Dictionary<string, Tuple<Type, object?>> GetSourceMap(object source)
        {
            // select distinct properties because they can be duplicated by inheritance
            var sourceProperties = source.GetType().Properties().Where(c => c.CanRead);

            return sourceProperties
                .Select(x => x.PropertyName)
                .Distinct()
                .ToDictionary(
                    x => x.ToLowerInvariant(),
                    x => Tuple.Create(sourceProperties.First(y => y.PropertyName == x).PropertyType.NativeType,
                        sourceProperties.First(y => y.PropertyName == x).Read(source)));
        }
    }
}
#pragma warning restore CA1031 // Do not catch general exception types
