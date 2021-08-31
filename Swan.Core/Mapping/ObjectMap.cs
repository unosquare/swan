using Swan.Reflection;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Swan.Mapping
{
    /// <summary>
    /// Represents an object map.
    /// </summary>
    /// <typeparam name="TTarget">The type of the destination.</typeparam>
    /// <typeparam name="TSource">The type of the destination.</typeparam>
    /// <seealso cref="IObjectMap" />
    public sealed class ObjectMap<TSource, TTarget> : ObjectMapBase
    {
        /// <summary>
        /// Creates a default mapping between the <see cref="TTarget"/> and the provided source type.
        /// </summary>
        /// <param name="sourceType">The source type.</param>
        public ObjectMap(ITypeProxy sourceType)
            : base(typeof(TTarget).TypeInfo())
        {
            if (sourceType is null)
                throw new ArgumentNullException(nameof(sourceType));

            foreach (var targetProperty in TargetType.Properties())
            {
                if (!targetProperty.CanWrite)
                    continue;

                if (!sourceType.Properties.TryGetValue(targetProperty.PropertyName, out var sourceProperty))
                    continue;

                if (!sourceProperty.CanRead)
                    continue;

                this[targetProperty] = (source) => sourceProperty.TryGetValue(source, out var value)
                    ? value
                    : targetProperty.DefaultValue;
            }
        }

        /// <summary>
        /// Adds or replaces a path to this map specifying, first the target and then the source
        /// which can be multi-level.
        /// </summary>
        /// <typeparam name="TTargetMember">The type of the destination property.</typeparam>
        /// <typeparam name="TSourceMember">The type of the source property.</typeparam>
        /// <param name="targetPropertyExpression">The destination property.</param>
        /// <param name="sourceProperty">The source property.</param>
        /// <returns>
        /// An object map representation of type of the destination property 
        /// and type of the source property.
        /// </returns>
        public ObjectMap<TSource, TTarget> SetPath<TTargetMember>(
                Expression<Func<TTarget, TTargetMember>> targetPropertyExpression,
                Func<TSource, object?> valueProvider)
        {
            if (targetPropertyExpression is null)
                throw new ArgumentNullException(nameof(targetPropertyExpression));

            if (valueProvider is null)
                throw new ArgumentNullException(nameof(valueProvider));

            var targetProperty = (targetPropertyExpression.Body as MemberExpression)?.Member as PropertyInfo;

            if (targetProperty is null)
                throw new ArgumentException("Invalid destination expression", nameof(targetPropertyExpression));

            this[targetProperty.ToPropertyProxy()] = (s) => valueProvider.Invoke((TSource)s);

            return this;
        }

        /// <summary>
        /// Removes the target property from the map.
        /// </summary>
        /// <typeparam name="TTargetProperty">The type of the destination property.</typeparam>
        /// <param name="targetPropertyExpression">The destination property.</param>
        /// <returns>
        /// An object map representation of type of the destination property 
        /// and type of the source property. 
        /// </returns>
        /// <exception cref="System.Exception">Invalid destination expression.</exception>
        public ObjectMap<TSource, TTarget> RemovePath<TTargetProperty>(
            Expression<Func<TTarget, TTargetProperty>> targetPropertyExpression)
        {
            if (targetPropertyExpression == null)
                throw new ArgumentNullException(nameof(targetPropertyExpression));

            var targetPropertyInfo = (targetPropertyExpression.Body as MemberExpression)?.Member as PropertyInfo;

            if (targetPropertyInfo == null)
                throw new ArgumentException("Invalid destination expression", nameof(targetPropertyExpression));

            var targetProperty = targetPropertyInfo.ToPropertyProxy();
            Remove(targetProperty);

            return this;
        }
    }
}