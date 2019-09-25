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
            _errors.Add(new ValidationError { ErrorMessage = errorMessage, PropertyName = propertyName });
    }
}
