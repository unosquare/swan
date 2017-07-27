namespace Unosquare.Swan.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Auto-registration Exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DependencyContainerAutoRegistrationException : Exception
    {
        private const string ErrorText = "Duplicate implementation of type {0} found ({1}).";

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerAutoRegistrationException"/> class.
        /// </summary>
        /// <param name="registerType">Type of the register.</param>
        /// <param name="types">The types.</param>
        public DependencyContainerAutoRegistrationException(Type registerType, IEnumerable<Type> types)
            : base(string.Format(ErrorText, registerType, GetTypesString(types)))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerAutoRegistrationException"/> class.
        /// </summary>
        /// <param name="registerType">Type of the register.</param>
        /// <param name="types">The types.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerAutoRegistrationException(Type registerType, IEnumerable<Type> types,
            Exception innerException)
            : base(string.Format(ErrorText, registerType, GetTypesString(types)), innerException)
        {
        }

        /// <summary>
        /// Gets the types string.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <returns></returns>
        private static string GetTypesString(IEnumerable<Type> types)
        {
            return string.Join(",", types.Select(type => type.FullName));
        }
    }
}