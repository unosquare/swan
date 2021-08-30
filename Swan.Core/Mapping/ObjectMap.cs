using Swan.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Swan.Mapping
{
    /// <summary>
    /// Represents an object map.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TTarget">The type of the destination.</typeparam>
    /// <seealso cref="IObjectMap" />
    public sealed class ObjectMap<TSource, TTarget> : IObjectMap
    {
        internal ObjectMap()
        {
            SourceType = typeof(TSource).TypeInfo();
            TargetType = typeof(TTarget).TypeInfo();

            foreach (var targetProperty in TargetType.Properties())
            {
                if (!targetProperty.CanWrite)
                    continue;

                if (!SourceType.Properties.TryGetValue(targetProperty.PropertyName, out var sourceProperty))
                    continue;

                if (!sourceProperty.CanRead)
                    continue;

                Paths[targetProperty] = new[] { sourceProperty };
            }
        }

        /// <inheritdoc/>
        public MapPathSet Paths { get; } = new();

        /// <inheritdoc/>
        public ITypeProxy SourceType { get; }

        /// <inheritdoc/>
        public ITypeProxy TargetType { get; }

        /// <summary>
        /// Adds or replaces a path to this map specifying, first the target and then the source
        /// which can be multi-level.
        /// </summary>
        /// <typeparam name="TTargetMember">The type of the destination property.</typeparam>
        /// <typeparam name="TSourceMember">The type of the source property.</typeparam>
        /// <param name="targetProperty">The destination property.</param>
        /// <param name="sourceProperty">The source property.</param>
        /// <returns>
        /// An object map representation of type of the destination property 
        /// and type of the source property.
        /// </returns>
        public ObjectMap<TSource, TTarget> SetPath
            <TTargetMember, TSourceMember>(
                Expression<Func<TTarget, TTargetMember>> targetProperty,
                Expression<Func<TSource, TSourceMember>> sourceProperty)
        {
            if (targetProperty == null)
                throw new ArgumentNullException(nameof(targetProperty));

            var propertyDestinationInfo = (targetProperty.Body as MemberExpression)?.Member as PropertyInfo;

            if (propertyDestinationInfo == null)
                throw new ArgumentException("Invalid destination expression", nameof(targetProperty));

            var sourceMembers = CreateSourcePath(sourceProperty);

            if (!sourceMembers.Any())
                throw new ArgumentException("Invalid source expression", nameof(sourceProperty));

            // reverse order
            sourceMembers.Reverse();
            Paths[propertyDestinationInfo.ToPropertyProxy()] = sourceMembers;

            return this;
        }

        /// <summary>
        /// Removes the target property from the map.
        /// </summary>
        /// <typeparam name="TDestinationProperty">The type of the destination property.</typeparam>
        /// <param name="destinationProperty">The destination property.</param>
        /// <returns>
        /// An object map representation of type of the destination property 
        /// and type of the source property. 
        /// </returns>
        /// <exception cref="System.Exception">Invalid destination expression.</exception>
        public ObjectMap<TSource, TTarget> RemovePath<TDestinationProperty>(
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

        private static List<IPropertyProxy> CreateSourcePath<TSourceProperty>(Expression<Func<TSource, TSourceProperty>> sourceProperty)
        {
            if (sourceProperty == null)
                throw new ArgumentNullException(nameof(sourceProperty));

            var sourceMembers = new List<IPropertyProxy>(16);
            var initialExpression = sourceProperty.Body as MemberExpression;

            while (true)
            {
                var propertySourceInfo = initialExpression?.Member as PropertyInfo;
                if (propertySourceInfo == null) break;
                sourceMembers.Add(propertySourceInfo.ToPropertyProxy());
                initialExpression = initialExpression?.Expression as MemberExpression;
            }

            return sourceMembers;
        }
    }
}