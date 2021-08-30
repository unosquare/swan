using Swan.Reflection;

namespace Swan.Mapping
{
    /// <summary>
    /// Interface object map.
    /// </summary>
    public interface IObjectMap
    {
        /// <summary>
        /// Gets or sets the map.
        /// </summary>
        MapPathSet Paths { get; }

        /// <summary>
        /// Gets or sets the type of the source.
        /// </summary>
        ITypeProxy SourceType { get; }

        /// <summary>
        /// Gets or sets the type of the destination.
        /// </summary>
        ITypeProxy TargetType { get; }
    }
}