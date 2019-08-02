using System.Collections.Generic;
using System.Linq;

namespace Swan.Validators
{
    /// <summary>
    /// Defines a validation result containing all validation errors and their properties.
    /// </summary>
    public class ObjectValidationResult
    {
        /// <summary>
        /// A list of errors.
        /// </summary>
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();

        /// <summary>
        /// <c>true</c> if there are no errors; otherwise, <c>false</c>.
        /// </summary>
        public bool IsValid => !Errors.Any();

        /// <summary>
        /// Adds an error with a specified property name.
        /// </summary>
        /// <param name="propertyName">The property name.</param>
        /// <param name="errorMessage">The error message.</param>
        public void Add(string propertyName, string errorMessage) =>
            Errors.Add(new ValidationError { ErrorMessage = errorMessage, PropertyName = propertyName });

        /// <summary>
        /// Defines a validation error.
        /// </summary>
        public class ValidationError
        {
            /// <summary>
            /// The property name.
            /// </summary>
            public string PropertyName { get; set; }

            /// <summary>
            /// The message error.
            /// </summary>
            public string ErrorMessage { get; set; }
        }
    }
}