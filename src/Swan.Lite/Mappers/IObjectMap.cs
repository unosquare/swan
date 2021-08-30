using Swan.Reflection;
using System.Collections.Generic;

namespace Swan.Mappers
{
    /// <summary>
    /// Interface object map.
    /// </summary>
    public interface IObjectMap
    {
        /// <summary>
        /// Gets or sets the map.
        /// </summary>
        Dictionary<IPropertyProxy, List<IPropertyProxy>> Map { get; }

        /// <summary>
        /// Gets or sets the type of the source.
        /// </summary>
        ITypeProxy SourceInfo { get; }

        /// <summary>
        /// Gets or sets the type of the destination.
        /// </summary>
        ITypeProxy TargetInfo { get; }
    }
}