using Swan.Reflection;
using System;
using System.Collections.Generic;

namespace Swan.Mapping
{
    /// <summary>
    /// Provides a base implementation of a <see cref="IObjectMap"/>
    /// It's basically a dictionary of target names and paths to sources.
    /// </summary>
    public abstract class ObjectMapBase : Dictionary<IPropertyProxy, InstanceValueProvider>, IObjectMap
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ObjectMapBase"/> class.
        /// </summary>
        /// <param name="targetType">The target type for this object map.</param>
        protected ObjectMapBase(ITypeProxy targetType)
            : base(64)
        {
            if (targetType is null)
                throw new ArgumentNullException(nameof(targetType));

            TargetType = targetType;
        }

        /// <inheritdoc />
        public ITypeProxy TargetType { get; }
    }
}
