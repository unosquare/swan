namespace Unosquare.Swan.Exceptions
{
    using System;

    /// <summary>
    /// Generic Constraint Registration Exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DependencyContainerRegistrationException : Exception
    {
        private const string ConvertErrorText = "Cannot convert current registration of {0} to {1}";
        private const string GenericConstraintErrorText = "Type {1} is not valid for a registration of type {0}";

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="method">The method.</param>
        public DependencyContainerRegistrationException(Type type, string method)
            : base(string.Format(ConvertErrorText, type.FullName, method))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="method">The method.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerRegistrationException(Type type, string method, Exception innerException)
            : base(string.Format(ConvertErrorText, type.FullName, method), innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationException"/> class.
        /// </summary>
        /// <param name="registerType">Type of the register.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        public DependencyContainerRegistrationException(Type registerType, Type implementationType)
            : base(string.Format(GenericConstraintErrorText, registerType.FullName, implementationType.FullName))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationException"/> class.
        /// </summary>
        /// <param name="registerType">Type of the register.</param>
        /// <param name="implementationType">Type of the implementation.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerRegistrationException(Type registerType, Type implementationType, Exception innerException)
            : base(string.Format(GenericConstraintErrorText, registerType.FullName, implementationType.FullName), innerException)
        {
        }
    }
}
