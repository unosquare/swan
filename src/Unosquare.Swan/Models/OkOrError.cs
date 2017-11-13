namespace Unosquare.Swan.Models
{
    /// <summary>
    /// Represents a Ok value or Error value
    /// </summary>
    /// <typeparam name="T">The type of OK value</typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    public class OkOrError<T, TError>
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is ok.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is ok; otherwise, <c>false</c>.
        /// </value>
        public bool IsOk { get; set; }

        /// <summary>
        /// Gets or sets the ok.
        /// </summary>
        /// <value>
        /// The ok.
        /// </value>
        public T Ok { get; set; } = default(T);

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        /// <value>
        /// The error.
        /// </value>
        public TError Error { get; set; } = default(TError);
    }
}
