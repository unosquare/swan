namespace Unosquare.Swan.Exceptions
{
    using System;

    /// <summary>
    /// Represents errors that occurs requesting a JSON file through HTTP
    /// </summary>
    /// <seealso cref="System.Exception" />
#if !NETSTANDARD1_3 && !UWP
    [Serializable]
#endif
    public class JsonRequestException 
        : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonRequestException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="httpErrorCode">The HTTP error code.</param>
        public JsonRequestException(string message, int httpErrorCode = 500)
            : base(message)
        {
            HttpErrorCode = httpErrorCode;
        }

        /// <summary>
        /// Gets the HTTP error code.
        /// </summary>
        /// <value>
        /// The HTTP error code.
        /// </value>
        public int HttpErrorCode { get; }
    }
}
