namespace Swan.Components
{
    /// <summary>
    /// Represents a message subscription.
    /// </summary>
    public interface IMessageHubSubscription
    {
        /// <summary>
        /// Token returned to the subscribed to reference this subscription.
        /// </summary>
        MessageHubSubscriptionToken SubscriptionToken { get; }

        /// <summary>
        /// Whether delivery should be attempted.
        /// </summary>
        /// <param name="message">Message that may potentially be delivered.</param>
        /// <returns><c>true</c> - ok to send, <c>false</c> - should not attempt to send.</returns>
        bool ShouldAttemptDelivery(IMessageHubMessage message);

        /// <summary>
        /// Deliver the message.
        /// </summary>
        /// <param name="message">Message to deliver.</param>
        void Deliver(IMessageHubMessage message);
    }
}