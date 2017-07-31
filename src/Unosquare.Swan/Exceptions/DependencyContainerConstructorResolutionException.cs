namespace Unosquare.Swan.Exceptions
{
    using System;

    /// <summary>
    /// Constructor Resolution Exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DependencyContainerConstructorResolutionException : Exception
    {
        private const string ErrorText = "Unable to resolve constructor for {0} using provided Expression.";

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerConstructorResolutionException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public DependencyContainerConstructorResolutionException(Type type)
            : base(string.Format(ErrorText, type.FullName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerConstructorResolutionException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerConstructorResolutionException(Type type, Exception innerException)
            : base(string.Format(ErrorText, type.FullName), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerConstructorResolutionException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
        public DependencyContainerConstructorResolutionException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerConstructorResolutionException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public DependencyContainerConstructorResolutionException(string message)
            : base(message)
        {
        }
    }
}
