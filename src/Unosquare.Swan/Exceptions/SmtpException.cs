#if NETSTANDARD1_3 || UWP
namespace Unosquare.Swan.Exceptions
{
    using Networking;

    /// <summary>
    /// Defines an SMTP Exceptions class
    /// </summary>
    public class SmtpException : System.Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpException" /> class with a message.
        /// </summary>
        /// <param name="message">The message.</param>
        public SmtpException(string message) 
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpException"/> class.
        /// </summary>
        /// <param name="replyCode">The SmtpStatusCode reply</param>
        /// <param name="message">The exception message</param>
        public SmtpException(SmtpStatusCode replyCode, string message) 
            : base($"{message} ReplyCode: {replyCode}")
        {
        }
    }
}
#endif