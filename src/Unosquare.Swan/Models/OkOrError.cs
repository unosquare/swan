namespace Unosquare.Swan.Models
{
    /// <summary>
    /// Represents a Ok value or Error value.
    /// </summary>
    /// <typeparam name="T">The type of OK value.</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    public class OkOrError<T, TError>
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is Ok.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is ok; otherwise, <c>false</c>.
        /// </value>
        public bool IsOk => Ok != null;

        /// <summary>
        /// Gets or sets the ok.
        /// </summary>
        /// <value>
        /// The ok.
        /// </value>
        public T Ok { get; set; }

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public TError Error { get; set; }

        /// <summary>
        /// Creates a new OkOrError from the specified Ok object.
        /// </summary>
        /// <param name="ok">The ok.</param>
        /// <returns>OkOrError instance.</returns>
        public static OkOrError<T, TError> FromOk(T ok) => new OkOrError<T, TError> { Ok = ok };

        /// <summary>
        /// Creates a new OkOrError from the specified Error object.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns>OkOrError instance.</returns>
        public static OkOrError<T, TError> FromError(TError error) => new OkOrError<T, TError> { Error = error };
    }
}
