namespace Swan.Components
{
    using System;

    /// <summary>
    /// An exception for dependency resolutions.
    /// </summary>
    /// <seealso cref="System.Exception" />
    [Serializable]
    public class DependencyContainerResolutionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerResolutionException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public DependencyContainerResolutionException(Type type)
            : base($"Unable to resolve type: {type.FullName}")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerResolutionException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerResolutionException(Type type, Exception innerException)
            : base($"Unable to resolve type: {type.FullName}", innerException)
        {
        }
    }
}
