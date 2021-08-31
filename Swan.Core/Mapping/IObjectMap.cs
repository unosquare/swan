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
    }
}