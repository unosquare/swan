using Swan.Reflection;
using System;
using System.Collections.Generic;

namespace Swan.Mapping
{
    /// <summary>
    /// Provides a basic implementation of a <see cref="IObjectMap"/>
    /// It's basically a dictionary of target properties to value providers.
    /// </summary>
    public class ObjectMap : Dictionary<IPropertyProxy, InstanceValueProvider>, IObjectMap
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ObjectMap"/> class.
        /// </summary>
        /// <param name="targetType">The target type for this object map.</param>
        public ObjectMap(ITypeProxy targetType)
            : base(64)
        {
            if (targetType is null)
                throw new ArgumentNullException(nameof(targetType));

            TargetType = targetType;
        }

        /// <inheritdoc />
        public ITypeProxy TargetType { get; }

        /// <inheritdoc />
        public virtual object Apply(object source) =>
            Apply(source, TargetType.CreateInstance());

        /// <inheritdoc />
        public virtual object Apply(object source, object target)
        {
            if (target is null)
                throw new ArgumentNullException(nameof(target));

            if (target.GetType().TypeInfo() != TargetType)
                throw new ArgumentException($"Parameter {nameof(target)} must be of type '{TargetType.ProxiedType}'");

            foreach (var path in this)
            {
                var targetProperty = path.Key;
                var targetValue = path.Value.Invoke(source);
                targetProperty.TrySetValue(target, targetValue);
            }

            return target;
        }
    }
}
