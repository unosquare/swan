namespace Unosquare.Swan.Models
{
    /// <summary>
    /// Represents a Ok value or Error value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TError">The type of the error.</typeparam>
    public class OkOrError<T, TError>
    {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is ok.
        /// </summary>
        public bool IsOk { get; set; }

        /// <summary>
        /// Gets or sets the ok.
        /// </summary>
        public T Ok { get; set; } = default(T);

        /// <summary>
        /// Gets or sets the error.
        /// </summary>
        public TError Error { get; set; } = default(TError);
    }
}
