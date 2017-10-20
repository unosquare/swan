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

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="method">The method.</param>
        public DependencyContainerRegistrationException(Type type, string method)
            : base(string.Format(ConvertErrorText, type.FullName, method))
        {
        }
    }
}
