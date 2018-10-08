namespace Unosquare.Swan.Exceptions
{
    using System;

    /// <summary>
    /// Represents errors that occurs requesting a JSON file through HTTP.
    /// </summary>
    /// <seealso cref="System.Exception" />
#if !NETSTANDARD1_3 && !UWP
    [Serializable]
#endif
    public class JsonRequestException
        : Exception
    {
        /// <inheritdoc />
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Unosquare.Swan.Exceptions.JsonRequestException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="httpErrorCode">The HTTP error code.</param>
        /// <param name="errorContent">Content of the error.</param>
        public JsonRequestException(string message, int httpErrorCode = 500, string errorContent = null)
            : base(message)
        {
            HttpErrorCode = httpErrorCode;
            HttpErrorContent = errorContent;
        }

        /// <summary>
        /// Gets the HTTP error code.
        /// </summary>
        /// <value>
        /// The HTTP error code.
        /// </value>
        public int HttpErrorCode { get; }

        /// <summary>
        /// Gets the content of the HTTP error.
        /// </summary>
        /// <value>
        /// The content of the HTTP error.
        /// </value>
        public string HttpErrorContent { get; }

        /// <inheritdoc />
        public override string ToString() => string.IsNullOrEmpty(HttpErrorContent) ? $"HTTP Response Status Code {HttpErrorCode} Error Message: {HttpErrorContent}" : base.ToString();
    }
}
