namespace Unosquare.Swan.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Reflection;
    
    /// <summary>
    /// Represents an AutoMapper-like object to map from one object type
    /// to another
    /// </summary>
    public class ObjectMapper
    {
        internal class PropertyInfoComparer : IEqualityComparer<PropertyInfo>
        {
            public bool Equals(PropertyInfo x, PropertyInfo y)
            {
                // TODO: Include mapping matcher and types proximity
                return x.Name == y.Name && x.PropertyType == y.PropertyType;
            }

            public int GetHashCode(PropertyInfo obj)
            {
                return obj.Name.GetHashCode() + obj.PropertyType.Name.GetHashCode();
            }
        }

        private static readonly PropertyTypeCache TypeCache = new PropertyTypeCache();
        private readonly List<IObjectMap> _maps = new List<IObjectMap>();

        /// <summary>
        /// Creates the map.
        /// </summary>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TDestination">The type of the destination.</typeparam>
        /// <returns>
        /// An object map representation of type of the destination property 
        /// and type of the source property
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// You can't create an existing map
        /// or
        /// Types doesn't match
        /// </exception>
        public ObjectMap<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            if (_maps.Any(x => x.SourceType == typeof(TSource) && x.DestinationType == typeof(TDestination)))
            {
                throw new InvalidOperationException("You can't create an existing map");
            }

            var sourceType = GetTypeProperties(typeof(TSource));
            var destinationType = GetTypeProperties(typeof(TDestination));

            var intersect = sourceType.Intersect(destinationType, new PropertyInfoComparer()).ToArray();

            if (intersect.Any() == false)
            {
                throw new InvalidOperationException("Types doesn't match");
            }

            var objMap = new ObjectMap<TSource, TDestination>(intersect);

            _maps.Add(objMap);

            return objMap;
        }

        /// <summary>
        /// Maps the specified source.
        /// </summary>
        /// <typeparam name="TDestination">The type of the destination.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="autoResolve">if set to <c>true</c> [automatic resolve].</param>
        /// <returns>A new instance of the map</returns>
        public TDestination Map<TDestination>(object source, bool autoResolve = true)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var destination = Activator.CreateInstance<TDestination>();
            var map =
                _maps.FirstOrDefault(x => x.SourceType == source.GetType() && x.DestinationType == typeof(TDestination));

            if (map != null)
            {
                foreach (var property in map.Map)
                {
                    var finalSource = source;

                    foreach (var sourceProperty in property.Value)
                    {
                        finalSource = sourceProperty.GetValue(finalSource);
                    }

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
                source.CopyPropertiesTo(destination);
            }

            return destination;
        }

        private static IEnumerable<PropertyInfo> GetTypeProperties(Type type)
        {
            return TypeCache.Retrieve(type, PropertyTypeCache.GetAllPublicPropertiesFunc(type));
        }
    }

    /// <summary>
    /// Represents an object map
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TDestination">The type of the destination.</typeparam>
    /// <seealso cref="Unosquare.Swan.Components.IObjectMap" />
    public class ObjectMap<TSource, TDestination> : IObjectMap
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectMap{TSource, TDestination}" /> class.
        /// </summary>
        /// <param name="intersect">The intersect.</param>
        public ObjectMap(IEnumerable<PropertyInfo> intersect)
        {
            SourceType = typeof(TSource);
            DestinationType = typeof(TDestination);

            Map = new Dictionary<PropertyInfo, List<PropertyInfo>>();

            foreach (var property in intersect)
            {
                Map.Add(DestinationType.GetProperty(property.Name), new List<PropertyInfo> { SourceType.GetProperty(property.Name) });
            }
        }

        /// <summary>
        /// Gets or sets the map.
        /// </summary>
        public Dictionary<PropertyInfo, List<PropertyInfo>> Map { get; }

        /// <summary>
        /// Gets or sets the type of the source.
        /// </summary>
        public Type SourceType { get; }

        /// <summary>
        /// Gets or sets the type of the destination.
        /// </summary>
        public Type DestinationType { get; }

        /// <summary>
        /// Maps the property.
        /// </summary>
        /// <typeparam name="TDestinationProperty">The type of the destination property.</typeparam>
        /// <typeparam name="TSourceProperty">The type of the source property.</typeparam>
        /// <param name="destinationProperty">The destination property.</param>
        /// <param name="sourceProperty">The source property.</param>
        /// <returns>
        /// An object map representation of type of the destination property 
        /// and type of the source property
        /// </returns>
        public ObjectMap<TSource, TDestination> MapProperty
            <TDestinationProperty, TSourceProperty>(
                Expression<Func<TDestination, TDestinationProperty>> destinationProperty,
                Expression<Func<TSource, TSourceProperty>> sourceProperty)
        {
            var memberDestinationExpression = destinationProperty?.Body as MemberExpression;
            var propertyDestinationInfo = memberDestinationExpression?.Member as PropertyInfo;

            if (propertyDestinationInfo == null)
            {
                throw new Exception("Invalid destination expression");
            }

            var sourceMembers = new List<PropertyInfo>();
            var initialExpression = sourceProperty?.Body as MemberExpression;

            while (true)
            {
                var propertySourceInfo = initialExpression?.Member as PropertyInfo;

                if (propertySourceInfo == null) break;
                sourceMembers.Add(propertySourceInfo);
                initialExpression = initialExpression.Expression as MemberExpression;
            }

            if (sourceMembers.Any() == false)
            {
                throw new Exception("Invalid source expression");
            }

            // reverse order
            sourceMembers.Reverse();
            Map[propertyDestinationInfo] = sourceMembers;

            return this;
        }

        /// <summary>
        /// Removes the map.
        /// </summary>
        /// <typeparam name="TDestinationProperty">The type of the destination property.</typeparam>
        /// <param name="destinationProperty">The destination property.</param>
        /// <returns>
        /// An object map representation of type of the destination property 
        /// and type of the source property 
        /// </returns>
        /// <exception cref="System.Exception">Invalid destination expression</exception>
        public ObjectMap<TSource, TDestination> RemoveMap<TDestinationProperty>(
            Expression<Func<TDestination, TDestinationProperty>> destinationProperty)
        {
            var memberDestinationExpression = destinationProperty?.Body as MemberExpression;
            var propertyDestinationInfo = memberDestinationExpression?.Member as PropertyInfo;

            if (propertyDestinationInfo == null)
                throw new Exception("Invalid destination expression");

            if (Map.ContainsKey(propertyDestinationInfo))
            {
                Map.Remove(propertyDestinationInfo);
            }

            return this;
        }
    }

    /// <summary>
    /// Interface object map
    /// </summary>
    public interface IObjectMap
    {
        /// <summary>
        /// Gets or sets the map.
        /// </summary>
        Dictionary<PropertyInfo, List<PropertyInfo>> Map { get; }

        /// <summary>
        /// Gets or sets the type of the source.
        /// </summary>
        Type SourceType { get; }

        /// <summary>
        /// Gets or sets the type of the destination.
        /// </summary>
        Type DestinationType { get; }
    }
}