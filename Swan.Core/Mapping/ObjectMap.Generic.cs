using Swan.Reflection;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Swan.Mapping
{
    /// <summary>
    /// Represents strongly-typed version of an object map.
    /// </summary>
    /// <typeparam name="TTarget">The type of the destination.</typeparam>
    /// <typeparam name="TSource">The type of the destination.</typeparam>
    /// <seealso cref="IObjectMap" />
    public sealed class ObjectMap<TSource, TTarget> : ObjectMap
    {
        /// <summary>
        /// Creates a default mapping between the ource and target types.
        /// </summary>
        public ObjectMap()
            : base(typeof(TTarget).TypeInfo())
        {
            foreach (var targetProperty in TargetType.Properties())
            {
                if (!targetProperty.CanWrite)
                    continue;

                if (!SourceType.Properties.TryGetValue(targetProperty.PropertyName, out var sourceProperty))
                    continue;

                if (!sourceProperty.CanRead)
                    continue;

                this[targetProperty] = (source) => sourceProperty.TryGetValue(source, out var value)
                    ? value
                    : targetProperty.DefaultValue;
            }
        }

        /// <summary>
        /// Gets the source type.
        /// </summary>
        public ITypeProxy SourceType { get; } = typeof(TSource).TypeInfo();

        /// <summary>
        /// Adds or replaces a path to populate a target via a source delegate.
        /// </summary>
        /// <typeparam name="TTargetMember"></typeparam>
        /// <param name="targetPropertyExpression"></param>
        /// <param name="valueProvider"></param>
        /// <returns></returns>
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
        /// 
        /// </summary>
        /// <typeparam name="TTargetProperty"></typeparam>
        /// <param name="targetPropertyExpression"></param>
        /// <returns></returns>
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