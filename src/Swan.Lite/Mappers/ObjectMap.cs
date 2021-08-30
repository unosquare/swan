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
    /// <typeparam name="TTarget">The type of the destination.</typeparam>
    /// <seealso cref="IObjectMap" />
    public class ObjectMap<TSource, TTarget> : IObjectMap
    {
        internal ObjectMap(IEnumerable<IPropertyProxy> intersect)
        {
            SourceType = typeof(TSource).TypeInfo();
            TargetType = typeof(TTarget).TypeInfo();

            foreach (var property in intersect)
            {
                Paths[TargetType.Properties[property.PropertyName]] =
                    new[] { SourceType.Properties[property.PropertyName] }; 
            }
        }

        /// <summary>
        /// Maps the property.
        /// </summary>
        /// <typeparam name="TTargetMember">The type of the destination property.</typeparam>
        /// <typeparam name="TSourceMember">The type of the source property.</typeparam>
        /// <param name="targetProperty">The destination property.</param>
        /// <param name="sourceProperty">The source property.</param>
        /// <returns>
        /// An object map representation of type of the destination property 
        /// and type of the source property.
        /// </returns>
        public ObjectMap<TSource, TTarget> MapProperty
            <TTargetMember, TSourceMember>(
                Expression<Func<TTarget, TTargetMember>> targetProperty,
                Expression<Func<TSource, TSourceMember>> sourceProperty)
        {
            if (targetProperty == null)
                throw new ArgumentNullException(nameof(targetProperty));

            var propertyDestinationInfo = (targetProperty.Body as MemberExpression)?.Member as PropertyInfo;

            if (propertyDestinationInfo == null)
                throw new ArgumentException("Invalid destination expression", nameof(targetProperty));

            var sourceMembers = GetSourceMembers(sourceProperty);

            if (!sourceMembers.Any())
                throw new ArgumentException("Invalid source expression", nameof(sourceProperty));

            // reverse order
            sourceMembers.Reverse();
            Paths[propertyDestinationInfo.ToPropertyProxy()] = sourceMembers;

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
        public ObjectMap<TSource, TTarget> RemoveMapProperty<TDestinationProperty>(
            Expression<Func<TTarget, TDestinationProperty>> destinationProperty)
        {
            if (destinationProperty == null)
                throw new ArgumentNullException(nameof(destinationProperty));

            var propertyDestinationInfo = (destinationProperty.Body as MemberExpression)?.Member as PropertyInfo;

            if (propertyDestinationInfo == null)
                throw new ArgumentException("Invalid destination expression", nameof(destinationProperty));

            var property = propertyDestinationInfo.ToPropertyProxy();
            Paths.Remove(property);

            return this;
        }

        /// <inheritdoc/>
        public MapPathLookup Paths { get; } = new();

        /// <inheritdoc/>
        public ITypeProxy SourceType { get; }

        /// <inheritdoc/>
        public ITypeProxy TargetType { get; }

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