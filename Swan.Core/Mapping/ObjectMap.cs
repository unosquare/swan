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
    }

    /// <summary>
    /// Provides a basic implementation of a <see cref="IObjectMap"/>
    /// It's basically a dictionary of target properties to value providers.
    /// </summary>
    public class ObjectMap<TTarget> : ObjectMap
    {
        /// <summary>
        /// Creates a new instance of <see cref="ObjectMap{TTarget}"/> class.
        /// </summary>
        public ObjectMap()
            : base(typeof(TTarget).TypeInfo())
        {
            // placeholder
        }
    }
}
