namespace Unosquare.Swan.Components
{
    /// <summary>
    /// A Message to be published/delivered by Messenger
    /// </summary>
    public interface IMessageHubMessage
    {
        /// <summary>
        /// The sender of the message, or null if not supported by the message implementation.
        /// </summary>
        object Sender { get; }
    }
}
