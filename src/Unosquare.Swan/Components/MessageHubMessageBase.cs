namespace Unosquare.Swan.Components
{
    using System;

    /// <summary>
    /// Base class for messages that provides weak reference storage of the sender
    /// </summary>
    public abstract class MessageHubMessageBase
        : IMessageHubMessage
    {
        /// <summary>
        /// Store a WeakReference to the sender just in case anyone is daft enough to
        /// keep the message around and prevent the sender from being collected.
        /// </summary>
        private readonly WeakReference _sender;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHubMessageBase"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <exception cref="System.ArgumentNullException">sender</exception>
        protected MessageHubMessageBase(object sender)
        {
            if (sender == null)
                throw new ArgumentNullException(nameof(sender));

            _sender = new WeakReference(sender);
        }

        /// <summary>
        /// The sender of the message, or null if not supported by the message implementation.
        /// </summary>
        public object Sender => _sender?.Target;
    }

    /// <summary>
    /// Generic message with user specified content
    /// </summary>
    /// <typeparam name="TContent">Content type to store</typeparam>
    public class MessageHubGenericMessage<TContent>
        : MessageHubMessageBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHubGenericMessage{TContent}"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="content">The content.</param>
        public MessageHubGenericMessage(object sender, TContent content)
            : base(sender)
        {
            Content = content;
        }

        /// <summary>
        /// Contents of the message
        /// </summary>
        public TContent Content { get; protected set; }
    }
}