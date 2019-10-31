using System;
using System.Threading.Tasks;

namespace Swan.Messaging
{
    /// <summary>
    /// Messenger hub responsible for taking subscriptions/publications and delivering of messages.
    /// </summary>
    public interface IMessageHub
    {
        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action.
        /// Messages will be delivered via the specified proxy.
        /// 
        /// All messages of this type will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message.</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered.</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction.</param>
        /// <param name="proxy">Proxy to use when delivering the messages.</param>
        /// <returns>MessageSubscription used to unsubscribing.</returns>
        MessageHubSubscriptionToken Subscribe<TMessage>(
            Action<TMessage> deliveryAction,
            bool useStrongReferences,
            IMessageHubProxy proxy)
            where TMessage : class, IMessageHubMessage;

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action with the given filter.
        /// Messages will be delivered via the specified proxy.
        /// All references are held with WeakReferences
        /// Only messages that "pass" the filter will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message.</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered.</param>
        /// <param name="messageFilter">The message filter.</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction.</param>
        /// <param name="proxy">Proxy to use when delivering the messages.</param>
        /// <returns>
        /// MessageSubscription used to unsubscribing.
        /// </returns>
        MessageHubSubscriptionToken Subscribe<TMessage>(
            Action<TMessage> deliveryAction,
            Func<TMessage, bool> messageFilter,
            bool useStrongReferences,
            IMessageHubProxy proxy)
            where TMessage : class, IMessageHubMessage;

        /// <summary>
        /// Unsubscribe from a particular message type.
        /// 
        /// Does not throw an exception if the subscription is not found.
        /// </summary>
        /// <typeparam name="TMessage">Type of message.</typeparam>
        /// <param name="subscriptionToken">Subscription token received from Subscribe.</param>
        void Unsubscribe<TMessage>(MessageHubSubscriptionToken subscriptionToken)
            where TMessage : class, IMessageHubMessage;

        /// <summary>
        /// Publish a message to any subscribers.
        /// </summary>
        /// <typeparam name="TMessage">Type of message.</typeparam>
        /// <param name="message">Message to deliver.</param>
        void Publish<TMessage>(TMessage message)
            where TMessage : class, IMessageHubMessage;

        /// <summary>
        /// Publish a message to any subscribers asynchronously.
        /// </summary>
        /// <typeparam name="TMessage">Type of message.</typeparam>
        /// <param name="message">Message to deliver.</param>
        /// <returns>A task from Publish action.</returns>
        Task PublishAsync<TMessage>(TMessage message)
            where TMessage : class, IMessageHubMessage;
    }
}
