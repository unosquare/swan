namespace Swan.DependencyInjection
{
    using System.Collections.Generic;

    /// <summary>
    /// Resolution settings.
    /// </summary>
    public class DependencyContainerResolveOptions
    {
        /// <summary>
        /// Gets the default options (attempt resolution of unregistered types, fail on named resolution if name not found).
        /// </summary>
        public static DependencyContainerResolveOptions Default { get; } = new();

        /// <summary>
        /// Gets or sets the unregistered resolution action.
        /// </summary>
        /// <value>
        /// The unregistered resolution action.
        /// </value>
        public DependencyContainerUnregisteredResolutionAction UnregisteredResolutionAction { get; set; } =
            DependencyContainerUnregisteredResolutionAction.AttemptResolve;

        /// <summary>
        /// Gets or sets the named resolution failure action.
        /// </summary>
        /// <value>
        /// The named resolution failure action.
        /// </value>
        public DependencyContainerNamedResolutionFailureAction NamedResolutionFailureAction { get; set; } =
            DependencyContainerNamedResolutionFailureAction.Fail;

        /// <summary>
        /// Gets the constructor parameters.
        /// </summary>
        /// <value>
        /// The constructor parameters.
        /// </value>
        public Dictionary<string, object> ConstructorParameters { get; } = new();

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public DependencyContainerResolveOptions Clone() => new()
        {
            NamedResolutionFailureAction = NamedResolutionFailureAction,
            UnregisteredResolutionAction = UnregisteredResolutionAction,
        };
    }

    /// <summary>
    /// Defines Resolution actions.
    /// </summary>
    public enum DependencyContainerUnregisteredResolutionAction
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
        GenericsOnly,
    }

    /// <summary>
    /// Enumerates failure actions.
    /// </summary>
    public enum DependencyContainerNamedResolutionFailureAction
    {
        /// <summary>
        /// The attempt unnamed resolution
        /// </summary>
        AttemptUnnamedResolution,

        /// <summary>
        /// The fail
        /// </summary>
        Fail,
    }

    /// <summary>
    /// Enumerates duplicate definition actions.
    /// </summary>
    public enum DependencyContainerDuplicateImplementationAction
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
        Fail,
    }
}