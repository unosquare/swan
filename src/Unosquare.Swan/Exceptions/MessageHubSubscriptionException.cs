namespace Unosquare.Swan.Exceptions
{
    using System;

    /// <summary>
    /// Thrown when an exceptions occurs while subscribing to a message type
    /// </summary>
    public class MessageHubSubscriptionException : Exception
    {
        private const string ErrorText = "Unable to add subscription for {0} : {1}";

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHubSubscriptionException"/> class.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="reason">The reason.</param>
        public MessageHubSubscriptionException(Type messageType, string reason)
            : base(string.Format(ErrorText, messageType, reason))
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHubSubscriptionException"/> class.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="reason">The reason.</param>
        /// <param name="innerException">The inner exception.</param>
        public MessageHubSubscriptionException(Type messageType, string reason, Exception innerException)
            : base(string.Format(ErrorText, messageType, reason), innerException)
        {

        }
    }
}
