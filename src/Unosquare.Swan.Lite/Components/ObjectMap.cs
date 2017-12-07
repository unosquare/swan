namespace Unosquare.Swan.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Represents an object map
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TDestination">The type of the destination.</typeparam>
    /// <seealso cref="Unosquare.Swan.Components.IObjectMap" />
    public class ObjectMap<TSource, TDestination> : IObjectMap
    {
        internal ObjectMap(IEnumerable<PropertyInfo> intersect)
        {
            SourceType = typeof(TSource);
            DestinationType = typeof(TDestination);
            Map = intersect.ToDictionary(
                property => DestinationType.GetProperty(property.Name),
                property => new List<PropertyInfo> {SourceType.GetProperty(property.Name)});
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
            var propertyDestinationInfo = (destinationProperty.Body as MemberExpression)?.Member as PropertyInfo;

            if (propertyDestinationInfo == null)
            {
                throw new Exception("Invalid destination expression");
            }

            var sourceMembers = new List<PropertyInfo>();
            var initialExpression = sourceProperty.Body as MemberExpression;

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
        /// Removes the map property.
        /// </summary>
        /// <typeparam name="TDestinationProperty">The type of the destination property.</typeparam>
        /// <param name="destinationProperty">The destination property.</param>
        /// <returns>
        /// An object map representation of type of the destination property 
        /// and type of the source property 
        /// </returns>
        /// <exception cref="System.Exception">Invalid destination expression</exception>
        public ObjectMap<TSource, TDestination> RemoveMapProperty<TDestinationProperty>(
            Expression<Func<TDestination, TDestinationProperty>> destinationProperty)
        {
            var propertyDestinationInfo = (destinationProperty.Body as MemberExpression)?.Member as PropertyInfo;

            if (propertyDestinationInfo == null)
                throw new Exception("Invalid destination expression");

            if (Map.ContainsKey(propertyDestinationInfo))
            {
                Map.Remove(propertyDestinationInfo);
            }

            return this;
        }
    }
}
