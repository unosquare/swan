using Swan.Reflection;
using System.Collections.Generic;

namespace Swan.Mapping
{
    /// <summary>
    /// A delegate 
    /// </summary>
    /// <param name="instance">An instance of an object.</param>
    /// <returns>The value extracted from the instance.</returns>
    public delegate object? InstanceValueProvider(object instance);

    /// <summary>
    /// Interface object map.
    /// </summary>
    public interface IObjectMap : IDictionary<IPropertyProxy, InstanceValueProvider>
    {
        /// <summary>
        /// Gets or sets the type of the destination.
        /// </summary>
        ITypeProxy TargetType { get; }

        /// <summary>
        /// Creates an instance of the target type and evaluates the <see cref="InstanceValueProvider"/>
        /// delegates for each of the mapped properties.
        /// </summary>
        /// <param name="source">The source object passed to each of the value provider delegates.</param>
        /// <returns>An instance of <see cref="TargetType"/> with mapped values from the source object.</returns>
        object Apply(object source);

        /// <summary>
        /// Evaluates the <see cref= "InstanceValueProvider" />
        /// delegates for each of the mapped properties, riting the target properties if possible.
        /// </summary>
        /// <param name="source">The source object used to evaluate the delegate.</param>
        /// <param name="target">The target object used to evaluate the delegate.</param>
        /// <returns></returns>
        object Apply(object source, object target);
    }
}