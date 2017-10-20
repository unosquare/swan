namespace Unosquare.Swan.Exceptions
{
    using System;

    /// <summary>
    /// Weak Reference Exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DependencyContainerWeakReferenceException : Exception
    {
        private const string ErrorText = "Unable to instantiate {0} - referenced object has been reclaimed";

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerWeakReferenceException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public DependencyContainerWeakReferenceException(Type type)
            : base(string.Format(ErrorText, type.FullName))
        {
        }
    }
}
