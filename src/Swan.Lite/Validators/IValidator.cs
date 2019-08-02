namespace Swan.Validators
{
    /// <summary>
    /// A simple Validator interface.
    /// </summary>
    public interface IValidator
    {
        /// <summary>
        /// The error message.
        /// </summary>
        string ErrorMessage { get; }

        /// <summary>
        /// Checks if a value is valid.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="value"> The value.</param>
        /// <returns>True if it is valid.False if it is not.</returns>
        bool IsValid<T>(T value);
    }
}