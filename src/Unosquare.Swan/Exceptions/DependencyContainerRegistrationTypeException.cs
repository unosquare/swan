namespace Unosquare.Swan.Exceptions
{
    using System;

    /// <summary>
    /// Registration Type Exception
    /// </summary>
    /// <seealso cref="System.Exception" />
    public class DependencyContainerRegistrationTypeException : Exception
    {
        private const string RegisterErrorText = "Cannot register type {0} - abstract classes or interfaces are not valid implementation types for {1}.";

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationTypeException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="factory">The factory.</param>
        public DependencyContainerRegistrationTypeException(Type type, string factory)
            : base(string.Format(RegisterErrorText, type.FullName, factory))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyContainerRegistrationTypeException"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="factory">The factory.</param>
        /// <param name="innerException">The inner exception.</param>
        public DependencyContainerRegistrationTypeException(Type type, string factory, Exception innerException)
            : base(string.Format(RegisterErrorText, type.FullName, factory), innerException)
        {
        }
    }
}
