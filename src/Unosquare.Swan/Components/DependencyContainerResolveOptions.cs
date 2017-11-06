namespace Unosquare.Swan.Components
{
    /// <summary>
    /// Resolution settings
    /// </summary>
    public class DependencyContainerResolveOptions
    {
        /// <summary>
        /// Gets the default options (attempt resolution of unregistered types, fail on named resolution if name not found)
        /// </summary>
        public static DependencyContainerResolveOptions Default { get; } = new DependencyContainerResolveOptions();

        /// <summary>
        /// Gets or sets the unregistered resolution action.
        /// </summary>
        /// <value>
        /// The unregistered resolution action.
        /// </value>
        public DependencyContainerUnregisteredResolutionActions UnregisteredResolutionAction { get; set; } =
            DependencyContainerUnregisteredResolutionActions.AttemptResolve;

        /// <summary>
        /// Gets or sets the named resolution failure action.
        /// </summary>
        /// <value>
        /// The named resolution failure action.
        /// </value>
        public DependencyContainerNamedResolutionFailureActions NamedResolutionFailureAction { get; set; } =
            DependencyContainerNamedResolutionFailureActions.Fail;
    }

    /// <summary>
    /// Defines Resolution actions
    /// </summary>
    public enum DependencyContainerUnregisteredResolutionActions
    {
        /// <summary>
        /// Attempt to resolve type, even if the type isn't registered.
        /// 
        /// Registered types/options will always take precedence.
        /// </summary>
        AttemptResolve,

        /// <summary>
        /// Fail resolution if type not explicitly registered
        /// </summary>
        Fail,

        /// <summary>
        /// Attempt to resolve unregistered type if requested type is generic
        /// and no registration exists for the specific generic parameters used.
        /// 
        /// Registered types/options will always take precedence.
        /// </summary>
        GenericsOnly
    }

    /// <summary>
    /// Enumerates failure actions
    /// </summary>
    public enum DependencyContainerNamedResolutionFailureActions
    {
        /// <summary>
        /// The attempt unnamed resolution
        /// </summary>
        AttemptUnnamedResolution,

        /// <summary>
        /// The fail
        /// </summary>
        Fail
    }

    /// <summary>
    /// Enumerates duplicate definition actions
    /// </summary>
    public enum DependencyContainerDuplicateImplementationActions
    {
        /// <summary>
        /// The register single
        /// </summary>
        RegisterSingle,

        /// <summary>
        /// The register multiple
        /// </summary>
        RegisterMultiple,

        /// <summary>
        /// The fail
        /// </summary>
        Fail
    }
}