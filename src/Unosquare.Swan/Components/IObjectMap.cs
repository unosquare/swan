namespace Unosquare.Swan.Components
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// Interface object map
    /// </summary>
    public interface IObjectMap
    {
        /// <summary>
        /// Gets or sets the map.
        /// </summary>
        Dictionary<PropertyInfo, List<PropertyInfo>> Map { get; }

        /// <summary>
        /// Gets or sets the type of the source.
        /// </summary>
        Type SourceType { get; }

        /// <summary>
        /// Gets or sets the type of the destination.
        /// </summary>
        Type DestinationType { get; }
    }
}
