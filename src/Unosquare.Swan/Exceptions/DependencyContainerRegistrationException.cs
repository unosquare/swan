namespace Unosquare.Swan.Exceptions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Generic Constraint Registration Exception
    /// </summary>
    /// <seealso cref="Exception" />
    public class DependencyContainerRegistrationException : Exception
    {
        private const string ConvertErrorText = "Cannot convert current registration of {0} to {1}";
        private const string RegisterErrorText =
            "Cannot register type {0} - abstract classes or interfaces are not valid implementation types for {1}.";
        private const string ErrorText = "Duplicate implementation of type {0} found ({1}).";

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationException"/> class.
        /// </summary>
        /// <param name="registerType">Type of the register.</param>
        /// <param name="types">The types.</param>
        public DependencyContainerRegistrationException(Type registerType, IEnumerable<Type> types)
            : base(string.Format(ErrorText, registerType, GetTypesString(types)))
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationException" /> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="method">The method.</param>
        /// <param name="isTypeFactory">if set to <c>true</c> [is type factory].</param>
        public DependencyContainerRegistrationException(Type type, string method, bool isTypeFactory = false)
            : base(isTypeFactory
                ? string.Format(RegisterErrorText, type.FullName, method)
                : string.Format(ConvertErrorText, type.FullName, method))
        {
        }

        private static string GetTypesString(IEnumerable<Type> types)
        {
            return string.Join(",", types.Select(type => type.FullName));
        }
    }
}