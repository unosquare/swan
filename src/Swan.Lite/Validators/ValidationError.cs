namespace Swan.Validators
{
    /// <summary>
    /// Defines a validation error.
    /// </summary>
    public class ValidationError
    {
        /// <summary>
        /// The property name.
        /// </summary>
        public string PropertyName { get; internal set; }

        /// <summary>
        /// The message error.
        /// </summary>
        public string ErrorMessage { get; internal set; }
    }
}
