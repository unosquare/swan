namespace Unosquare.Swan.Exceptions
{
    using System;

    /// <summary>
    /// An exception for dependency resolutions
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DependencyContainerResolutionException : Exception
    {
        private const string ErrorText = "Unable to resolve type: {0}";

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerResolutionException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public DependencyContainerResolutionException(Type type)
            : base(string.Format(ErrorText, type.FullName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerResolutionException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerResolutionException(Type type, Exception innerException)
            : base(string.Format(ErrorText, type.FullName), innerException)
        {
        }
    }
}
