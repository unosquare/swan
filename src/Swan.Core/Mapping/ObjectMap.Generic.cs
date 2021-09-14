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
    internal sealed class ObjectMap<TSource, TTarget> : ObjectMap, IObjectMap<TSource, TTarget>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ObjectMap{TSource, TTarget}"/> class.
        /// It also populates a default map of properties that have the same names
        /// and compatible types.
        /// </summary>
        /// <param name="context">The parent object mapper containing all other maps.</param>
        public ObjectMap(ObjectMapper context)
            : base(context, typeof(TSource).TypeInfo(), typeof(TTarget).TypeInfo())
        {
            // placeholder
        }

        /// <inheritdoc />
        public TTarget Apply(TSource source, TTarget target) => source is null
            ? throw new ArgumentNullException(nameof(source))
            : target is null
            ? throw new ArgumentNullException(nameof(target))
            : (TTarget)base.Apply(source, target);

        /// <inheritdoc />
        public TTarget Apply(TSource source) => source is null
            ? throw new ArgumentNullException(nameof(source))
            : (TTarget)base.Apply(source);

        /// <inheritdoc />
        public IObjectMap<TSource, TTarget> Add<TTargetMember, TSourceMember>(
                Expression<Func<TTarget, TTargetMember>> targetPropertyExpression,
                Func<TSource, TSourceMember> valueProvider)
        {
            if (targetPropertyExpression is null)
                throw new ArgumentNullException(nameof(targetPropertyExpression));

            if (valueProvider is null)
                throw new ArgumentNullException(nameof(valueProvider));

            var targetProperty = (targetPropertyExpression.Body as MemberExpression)?.Member as PropertyInfo;

            if (targetProperty is null)
                throw new ArgumentException("Invalid destination expression", nameof(targetPropertyExpression));

            var targetProxy = targetProperty.ToPropertyProxy();
            if (!targetProxy.CanWrite)
                throw new ArgumentException(
                    $"Target property '{targetProxy.PropertyName}' is read only.",
                    nameof(targetPropertyExpression));

            if (!targetProxy.IsAssignableFrom(typeof(TSourceMember)))
                throw new ArgumentException(
                    $"Target property '{targetProxy.PropertyName}' cannot be assigned a value of type {typeof(TTargetMember)}.",
                    nameof(valueProvider));

            this[targetProxy] = (s) => valueProvider.Invoke((TSource)s);

            return this;
        }

        /// <inheritdoc />
        public IObjectMap<TSource, TTarget> Remove<TTargetProperty>(
            Expression<Func<TTarget, TTargetProperty>> targetPropertyExpression)
        {
            if (targetPropertyExpression == null)
                throw new ArgumentNullException(nameof(targetPropertyExpression));

            var targetPropertyInfo = (targetPropertyExpression.Body as MemberExpression)?.Member as PropertyInfo;

            if (targetPropertyInfo == null)
                throw new ArgumentException("Invalid destination expression", nameof(targetPropertyExpression));

            var targetProperty = targetPropertyInfo.ToPropertyProxy();
            TryRemove(targetProperty, out _);

            return this;
        }
    }
}