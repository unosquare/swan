using Swan.Extensions;
using Swan.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Swan.Mappers
{
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
    public partial class ObjectMapper
    {
        private static readonly Lazy<ObjectMapper> LazyInstance = new(() => new ObjectMapper());

        private readonly List<IObjectMap> _maps = new();

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
            IDictionary<string, object>? source,
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

        /// <summary>
        /// Creates the map.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TTarget">The type of the destination.</typeparam>
        /// <returns>
        /// An object map representation of type of the destination property 
        /// and type of the source property.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// You can't create an existing map
        /// or
        /// Types doesn't match.
        /// </exception>
        public ObjectMap<TSource, TTarget> CreateMap<TSource, TTarget>()
        {
            if (_maps.Any(x => x.SourceType.ProxiedType == typeof(TSource) && x.TargetType.ProxiedType == typeof(TTarget)))
                throw new InvalidOperationException("You can't create an existing map");

            var sourceType = typeof(TSource).Properties();
            var targetType = typeof(TTarget).Properties();

            var intersect = sourceType.Intersect(targetType, new PropertyProxyComparer()).ToArray();

            if (!intersect.Any())
                throw new InvalidOperationException("Types don't have any mathing properties.");
            
            var map = new ObjectMap<TSource, TTarget>(intersect);

            _maps.Add(map);

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
        public TTarget Map<TTarget>(object source, bool autoResolve = true)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var destination = TypeManager.CreateInstance<TTarget>();
            var map = _maps
                .FirstOrDefault(x => x.SourceType.ProxiedType == source.GetType() && x.TargetType.ProxiedType == typeof(TTarget));

            if (map != null)
            {
                foreach (var property in map.Paths)
                {
                    var finalSource = property.Value.SourcePath.Aggregate(source,
                        (current, sourceProperty) => sourceProperty.GetValue(current));

                    property.Value.TargetMember.SetValue(destination, finalSource);
                }
            }
            else
            {
                if (!autoResolve)
                {
                    throw new InvalidOperationException(
                        $"You can't map from type {source.GetType().Name} to {typeof(TTarget).Name}");
                }

                // Missing mapping, try to use default behavior
                Copy(source, destination!);
            }

            return destination;
        }

        private static int CopyInternal(
            object target,
            Dictionary<string, Tuple<Type, object>> sourceProperties,
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
                .Where(x => sourceProperties.Keys.Contains(x.Key))
                .When(() => requiredProperties != null, q => q.Where(y => requiredProperties!.Contains(y.Key)))
                .When(() => ignoredProperties != null, q => q.Where(y => !ignoredProperties!.Contains(y.Key)))
                .ToDictionary(x => x.Value, x => sourceProperties[x.Key])
                .Sum(x => TrySetValue(x.Key.PropertyInfo, x.Value, target) ? 1 : 0);
        }

        private static bool TrySetValue(PropertyInfo propertyInfo, Tuple<Type, object> property, object target)
        {
            try
            {
                var (type, value) = property;

                if (type.IsEnum)
                {
                    propertyInfo.SetValue(target,
                        Enum.ToObject(propertyInfo.PropertyType, value));

                    return true;
                }

                if (type.IsValueType || propertyInfo.PropertyType != type)
                    return propertyInfo.TrySetBasicType(value, target);

                if (propertyInfo.PropertyType.IsArray)
                {
                    propertyInfo.TrySetArray(value as IEnumerable<object>, target);
                    return true;
                }

                propertyInfo.SetValue(target, GetValue(value, propertyInfo.PropertyType));

                return true;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                // swallow
            }

            return false;
        }

        private static object? GetValue(object? source, Type targetType)
        {
            if (source == null)
                return null;

            object? target = null;

            source.CreateTarget(targetType, false, ref target);

            switch (source)
            {
                case string:
                    target = source;
                    break;
                case IList sourceList when target is IList targetList:
                    var addMethod = targetType.GetMethods()
                        .FirstOrDefault(
                            m => m.Name == nameof(sourceList.Add) && m.IsPublic && m.GetParameters().Length == 1);

                    if (addMethod == null) return target;

                    var isItemValueType = targetList.GetType().GetElementType().IsValueType;

                    foreach (var item in sourceList)
                    {
                        try
                        {
                            targetList.Add(isItemValueType
                                ? item
                                : item.CopyPropertiesToNew<object>());
                        }
#pragma warning disable CA1031 // Do not catch general exception types
                        catch
#pragma warning restore CA1031 // Do not catch general exception types
                        {
                            // ignored
                        }
                    }

                    break;
                default:
                    source.CopyPropertiesTo(target!);
                    break;
            }

            return target;
        }

        private static Dictionary<string, Tuple<Type, object>> GetSourceMap(object source)
        {
            // select distinct properties because they can be duplicated by inheritance
            var sourceProperties = source.GetType().Properties().Where(c => c.CanRead);

            return sourceProperties
                .Select(x => x.PropertyName)
                .Distinct()
                .ToDictionary(
                    x => x.ToLowerInvariant(),
                    x => Tuple.Create(sourceProperties.First(y => y.PropertyName == x).PropertyType,
                        sourceProperties.First(y => y.PropertyName == x).GetValue(source)));
        }
    }
}