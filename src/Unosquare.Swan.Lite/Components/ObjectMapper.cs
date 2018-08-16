namespace Unosquare.Swan.Components
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
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
    /// using Unosquare.Swan
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
    ///         var obj = new { Name = "Søren", Age = 42 };
    ///         
    ///         var person = Runtime.ObjectMapper.Map&lt;Person&gt;(obj);
    ///     }
    /// }
    /// </code>
    /// The following code explains how to explicitly map certain properties.
    /// <code>
    /// using Unosquare.Swan
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
        private readonly List<IObjectMap> _maps = new List<IObjectMap>();

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
            object source,
            object target,
            string[] propertiesToCopy = null,
            string[] ignoreProperties = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            return Copy(
                target,
                propertiesToCopy,
                ignoreProperties,
                GetSourceMap(source));
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
            IDictionary<string, object> source,
            object target,
            string[] propertiesToCopy = null,
            string[] ignoreProperties = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (target == null)
                throw new ArgumentNullException(nameof(target));

            return Copy(
                target,
                propertiesToCopy,
                ignoreProperties,
                source.ToDictionary(
                    x => x.Key.ToLowerInvariant(),
                    x => new TypeValuePair(typeof(object), x.Value)));
        }

        /// <summary>
        /// Creates the map.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TDestination">The type of the destination.</typeparam>
        /// <returns>
        /// An object map representation of type of the destination property 
        /// and type of the source property.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// You can't create an existing map
        /// or
        /// Types doesn't match.
        /// </exception>
        public ObjectMap<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            if (_maps.Any(x => x.SourceType == typeof(TSource) && x.DestinationType == typeof(TDestination)))
            {
                throw new InvalidOperationException("You can't create an existing map");
            }

            var sourceType = Runtime.PropertyTypeCache.RetrieveAllProperties<TSource>(true);
            var destinationType = Runtime.PropertyTypeCache.RetrieveAllProperties<TDestination>(true);

            var intersect = sourceType.Intersect(destinationType, new PropertyInfoComparer()).ToArray();

            if (intersect.Any() == false)
            {
                throw new InvalidOperationException("Types doesn't match");
            }

            var map = new ObjectMap<TSource, TDestination>(intersect);

            _maps.Add(map);

            return map;
        }

        /// <summary>
        /// Maps the specified source.
        /// </summary>
        /// <typeparam name="TDestination">The type of the destination.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="autoResolve">if set to <c>true</c> [automatic resolve].</param>
        /// <returns>
        /// A new instance of the map.
        /// </returns>
        /// <exception cref="ArgumentNullException">source.</exception>
        /// <exception cref="InvalidOperationException">You can't map from type {source.GetType().Name} to {typeof(TDestination).Name}.</exception>
        public TDestination Map<TDestination>(object source, bool autoResolve = true)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var destination = Activator.CreateInstance<TDestination>();
            var map = _maps
                .FirstOrDefault(x => x.SourceType == source.GetType() && x.DestinationType == typeof(TDestination));

            if (map != null)
            {
                foreach (var property in map.Map)
                {
                    var finalSource = property.Value.Aggregate(source,
                        (current, sourceProperty) => sourceProperty.GetValue(current));

                    property.Key.SetValue(destination, finalSource);
                }
            }
            else
            {
                if (autoResolve == false)
                {
                    throw new InvalidOperationException(
                        $"You can't map from type {source.GetType().Name} to {typeof(TDestination).Name}");
                }

                // Missing mapping, try to use default behavior
                Copy(source, destination);
            }

            return destination;
        }

        private static int Copy(
            object target,
            IEnumerable<string> propertiesToCopy,
            IEnumerable<string> ignoreProperties,
            Dictionary<string, TypeValuePair> sourceProperties)
        {
            // Filter properties
            var requiredProperties = propertiesToCopy?
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.ToLowerInvariant());

            var ignoredProperties = ignoreProperties?
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.ToLowerInvariant());

            var properties = Runtime.PropertyTypeCache
                .RetrieveFilteredProperties(target.GetType(), true, x => x.CanWrite)
                .ToArray();

            return properties
                .Select(x => x.Name)
                .Distinct()
                .ToDictionary(x => x.ToLowerInvariant(), x => properties.First(y => y.Name == x))
                .Where(x => sourceProperties.Keys.Contains(x.Key))
                .When(() => requiredProperties != null, q => q.Where(y => requiredProperties.Contains(y.Key)))
                .When(() => ignoredProperties != null, q => q.Where(y => !ignoredProperties.Contains(y.Key)))
                .ToDictionary(x => x.Value, x => sourceProperties[x.Key])
                .Sum(x => TrySetValue(x, target) ? 1 : 0);
        }

        private static bool TrySetValue(KeyValuePair<PropertyInfo, TypeValuePair> property, object target)
        {
            try
            {
                SetValue(property, target);
                return true;
            }
            catch
            {
                // swallow
            }

            return false;
        }

        private static void SetValue(KeyValuePair<PropertyInfo, TypeValuePair> property, object target)
        {
            if (property.Value.Type.GetTypeInfo().IsEnum)
            {
                property.Key.SetValue(target,
                    Enum.ToObject(property.Key.PropertyType, property.Value.Value));

                return;
            }

            if (!property.Value.Type.IsValueType() && property.Key.PropertyType == property.Value.Type)
            {
                property.Key.SetValue(target, GetValue(property.Value.Value, property.Key.PropertyType));

                return;
            }

            if (property.Key.PropertyType == typeof(bool))
            {
                property.Key.SetValue(target,
                    Convert.ToBoolean(property.Value.Value));

                return;
            }

            property.Key.TrySetBasicType(property.Value.Value, target);
        }

        private static object GetValue(object source, Type targetType)
        {
            if (source == null)
                return null;

            object target = null;

            source.CreateTarget(targetType, false, ref target);

            switch (source)
            {
                case string _:
                    target = source;
                    break;
                case IList sourceList when target is Array targetArray:
                    for (var i = 0; i < sourceList.Count; i++)
                    {
                        try
                        {
                            targetArray.SetValue(
                                sourceList[i].GetType().IsValueType()
                                    ? sourceList[i]
                                    : sourceList[i].CopyPropertiesToNew<object>(), i);
                        }
                        catch
                        {
                            // ignored
                        }
                    }

                    break;
                case IList sourceList when target is IList targetList:
                    var addMethod = targetType.GetMethods()
                        .FirstOrDefault(
                            m => m.Name.Equals(Formatters.Json.AddMethodName) && m.IsPublic &&
                                 m.GetParameters().Length == 1);

                    if (addMethod == null) return target;

                    foreach (var item in sourceList)
                    {
                        try
                        {
                            targetList.Add(item.GetType().IsValueType()
                                ? item
                                : item.CopyPropertiesToNew<object>());
                        }
                        catch
                        {
                            // ignored
                        }
                    }

                    break;
                default:
                    source.CopyPropertiesTo(target);
                    break;
            }

            return target;
        }

        private static Dictionary<string, TypeValuePair> GetSourceMap(object source)
        {
            // select distinct properties because they can be duplicated by inheritance
            var sourceProperties = Runtime.PropertyTypeCache
                .RetrieveFilteredProperties(source.GetType(), true, x => x.CanRead)
                .ToArray();

            return sourceProperties
                .Select(x => x.Name)
                .Distinct()
                .ToDictionary(
                    x => x.ToLowerInvariant(),
                    x => new TypeValuePair(sourceProperties.First(y => y.Name == x).PropertyType,
                        sourceProperties.First(y => y.Name == x).GetValue(source)));
        }

        internal class TypeValuePair
        {
            public TypeValuePair(Type type, object value)
            {
                Type = type;
                Value = value;
            }

            public Type Type { get; }

            public object Value { get; }
        }

        internal class PropertyInfoComparer : IEqualityComparer<PropertyInfo>
        {
            public bool Equals(PropertyInfo x, PropertyInfo y)
                => x != null && y != null && x.Name == y.Name && x.PropertyType == y.PropertyType;

            public int GetHashCode(PropertyInfo obj)
                => obj.Name.GetHashCode() + obj.PropertyType.Name.GetHashCode();
        }
    }
}