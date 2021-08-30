using Swan.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Swan.Mappers
{
    /// <summary>
    /// Represents an object map.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TDestination">The type of the destination.</typeparam>
    /// <seealso cref="IObjectMap" />
    public class ObjectMap<TSource, TDestination> : IObjectMap
    {
        internal ObjectMap(IEnumerable<IPropertyProxy> intersect)
        {
            SourceInfo = typeof(TSource).TypeInfo();
            TargetInfo = typeof(TDestination).TypeInfo();
            Map = intersect.ToDictionary(
                property => TargetInfo.Properties[property.PropertyName],
                property => new List<IPropertyProxy> { SourceInfo.Properties[property.PropertyName] });
        }

        /// <inheritdoc/>
        public Dictionary<IPropertyProxy, List<IPropertyProxy>> Map { get; }

        /// <inheritdoc/>
        public ITypeProxy SourceInfo { get; }

        /// <inheritdoc/>
        public ITypeProxy TargetInfo { get; }

        /// <summary>
        /// Maps the property.
        /// </summary>
        /// <typeparam name="TDestinationProperty">The type of the destination property.</typeparam>
        /// <typeparam name="TSourceProperty">The type of the source property.</typeparam>
        /// <param name="destinationProperty">The destination property.</param>
        /// <param name="sourceProperty">The source property.</param>
        /// <returns>
        /// An object map representation of type of the destination property 
        /// and type of the source property.
        /// </returns>
        public ObjectMap<TSource, TDestination> MapProperty
            <TDestinationProperty, TSourceProperty>(
                Expression<Func<TDestination, TDestinationProperty>> destinationProperty,
                Expression<Func<TSource, TSourceProperty>> sourceProperty)
        {
            if (destinationProperty == null)
                throw new ArgumentNullException(nameof(destinationProperty));

            var propertyDestinationInfo = (destinationProperty.Body as MemberExpression)?.Member as PropertyInfo;

            if (propertyDestinationInfo == null)
                throw new ArgumentException("Invalid destination expression", nameof(destinationProperty));

            var sourceMembers = GetSourceMembers(sourceProperty);

            if (!sourceMembers.Any())
                throw new ArgumentException("Invalid source expression", nameof(sourceProperty));

            // reverse order
            sourceMembers.Reverse();
            Map[propertyDestinationInfo.ToPropertyProxy()] = sourceMembers;

            return this;
        }

        /// <summary>
        /// Removes the map property.
        /// </summary>
        /// <typeparam name="TDestinationProperty">The type of the destination property.</typeparam>
        /// <param name="destinationProperty">The destination property.</param>
        /// <returns>
        /// An object map representation of type of the destination property 
        /// and type of the source property. 
        /// </returns>
        /// <exception cref="System.Exception">Invalid destination expression.</exception>
        public ObjectMap<TSource, TDestination> RemoveMapProperty<TDestinationProperty>(
            Expression<Func<TDestination, TDestinationProperty>> destinationProperty)
        {
            if (destinationProperty == null)
                throw new ArgumentNullException(nameof(destinationProperty));

            var propertyDestinationInfo = (destinationProperty.Body as MemberExpression)?.Member as PropertyInfo;

            if (propertyDestinationInfo == null)
                throw new ArgumentException("Invalid destination expression", nameof(destinationProperty));

            var property = propertyDestinationInfo.ToPropertyProxy();
            if (Map.ContainsKey(property))
                Map.Remove(property);

            return this;
        }

        private static List<IPropertyProxy> GetSourceMembers<TSourceProperty>(Expression<Func<TSource, TSourceProperty>> sourceProperty)
        {
            if (sourceProperty == null)
                throw new ArgumentNullException(nameof(sourceProperty));

            var sourceMembers = new List<IPropertyProxy>();
            var initialExpression = sourceProperty.Body as MemberExpression;

            while (true)
            {
                var propertySourceInfo = initialExpression?.Member as PropertyInfo;

                if (propertySourceInfo == null) break;
                sourceMembers.Add(propertySourceInfo.ToPropertyProxy());
                initialExpression = initialExpression.Expression as MemberExpression;
            }

            return sourceMembers;
        }
    }
}