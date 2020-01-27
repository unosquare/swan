using System.Collections.Generic;
using System.Linq;

namespace Swan.Validators
{
    /// <summary>
    /// Defines a validation result containing all validation errors and their properties.
    /// </summary>
    public class ObjectValidationResult
    {
        private readonly List<ValidationError> _errors = new List<ValidationError>();

        /// <summary>
        /// A list of errors.
        /// </summary>
        public IReadOnlyList<ValidationError> Errors => _errors;

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
            _errors.Add(new ValidationError(errorMessage, propertyName));

        /// <summary>
        /// Defines a validation error.
        /// </summary>
        public class ValidationError
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ValidationError"/> class.
            /// </summary>
            /// <param name="propertyName">Name of the property.</param>
            /// <param name="errorMessage">The error message.</param>
            public ValidationError(string propertyName, string errorMessage)
            {
                PropertyName = propertyName;
                ErrorMessage = errorMessage;
            }

            /// <summary>
            /// The property name.
            /// </summary>
            public string PropertyName { get; }

            /// <summary>
            /// The message error.
            /// </summary>
            public string ErrorMessage { get; }
        }
    }
}
