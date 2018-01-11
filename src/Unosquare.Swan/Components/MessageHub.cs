// ===============================================================================
// TinyIoC - TinyMessenger
//
// A simple messenger/event aggregator.
//
// https://github.com/grumpydev/TinyIoC/blob/master/src/TinyIoC/TinyMessenger.cs
// ===============================================================================
// Copyright © Steven Robbins.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
// ===============================================================================

namespace Unosquare.Swan.Components
{
    using System.Threading.Tasks;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    #region Message Types / Interfaces

    /// <summary>
    /// Represents a message subscription
    /// </summary>
    public interface IMessageHubSubscription
    {
        /// <summary>
        /// Token returned to the subscribed to reference this subscription
        /// </summary>
        MessageHubSubscriptionToken SubscriptionToken { get; }

        /// <summary>
        /// Whether delivery should be attempted.
        /// </summary>
        /// <param name="message">Message that may potentially be delivered.</param>
        /// <returns><c>true</c> - ok to send, <c>false</c> - should not attempt to send</returns>
        bool ShouldAttemptDelivery(IMessageHubMessage message);

        /// <summary>
        /// Deliver the message
        /// </summary>
        /// <param name="message">Message to deliver</param>
        void Deliver(IMessageHubMessage message);
    }

    /// <summary>
    /// Message proxy definition.
    /// 
    /// A message proxy can be used to intercept/alter messages and/or
    /// marshal delivery actions onto a particular thread.
    /// </summary>
    public interface IMessageHubProxy
    {
        /// <summary>
        /// Delivers the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="subscription">The subscription.</param>
        void Deliver(IMessageHubMessage message, IMessageHubSubscription subscription);
    }

    /// <summary>
    /// Default "pass through" proxy.
    /// 
    /// Does nothing other than deliver the message.
    /// </summary>
    public sealed class MessageHubDefaultProxy : IMessageHubProxy
    {
        private MessageHubDefaultProxy()
        {
            // placeholder
        }

        /// <summary>
        /// Singleton instance of the proxy.
        /// </summary>
        public static MessageHubDefaultProxy Instance { get; } = new MessageHubDefaultProxy();

        /// <summary>
        /// Delivers the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="subscription">The subscription.</param>
        public void Deliver(IMessageHubMessage message, IMessageHubSubscription subscription)
            => subscription.Deliver(message);
    }

    #endregion

    #region Hub Interface

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
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction </param>
        /// <param name="proxy">Proxy to use when delivering the messages</param>
        /// <returns>MessageSubscription used to unsubscribing</returns>
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
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="messageFilter">The message filter.</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction</param>
        /// <param name="proxy">Proxy to use when delivering the messages</param>
        /// <returns>
        /// MessageSubscription used to unsubscribing
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
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="subscriptionToken">Subscription token received from Subscribe</param>
        void Unsubscribe<TMessage>(MessageHubSubscriptionToken subscriptionToken)
            where TMessage : class, IMessageHubMessage;

        /// <summary>
        /// Publish a message to any subscribers
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="message">Message to deliver</param>
        void Publish<TMessage>(TMessage message)
            where TMessage : class, IMessageHubMessage;

        /// <summary>
        /// Publish a message to any subscribers asynchronously
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="message">Message to deliver</param>
        /// <returns>A task from Publish action</returns>
        Task PublishAsync<TMessage>(TMessage message)
            where TMessage : class, IMessageHubMessage;
    }

    #endregion

    #region Hub Implementation

    /// <inheritdoc />
    public sealed class MessageHub : IMessageHub
    {
        #region Private Types and Interfaces

        private readonly object _subscriptionsPadlock = new object();

        private readonly Dictionary<Type, List<SubscriptionItem>> _subscriptions =
            new Dictionary<Type, List<SubscriptionItem>>();

        private class WeakMessageSubscription<TMessage> : IMessageHubSubscription
            where TMessage : class, IMessageHubMessage
        {
            private readonly WeakReference _deliveryAction;
            private readonly WeakReference _messageFilter;

            /// <summary>
            /// Initializes a new instance of the <see cref="WeakMessageSubscription{TMessage}" /> class.
            /// </summary>
            /// <param name="subscriptionToken">The subscription token.</param>
            /// <param name="deliveryAction">The delivery action.</param>
            /// <param name="messageFilter">The message filter.</param>
            /// <exception cref="ArgumentNullException">subscriptionToken
            /// or
            /// deliveryAction
            /// or
            /// messageFilter</exception>
            public WeakMessageSubscription(
                MessageHubSubscriptionToken subscriptionToken,
                Action<TMessage> deliveryAction,
                Func<TMessage, bool> messageFilter)
            {
                SubscriptionToken = subscriptionToken ?? throw new ArgumentNullException(nameof(subscriptionToken));
                _deliveryAction = new WeakReference(deliveryAction);
                _messageFilter = new WeakReference(messageFilter);
            }

            public MessageHubSubscriptionToken SubscriptionToken { get; }

            public bool ShouldAttemptDelivery(IMessageHubMessage message)
            {
                return _deliveryAction.IsAlive && _messageFilter.IsAlive &&
                       ((Func<TMessage, bool>) _messageFilter.Target).Invoke((TMessage) message);
            }

            public void Deliver(IMessageHubMessage message)
            {
                if (_deliveryAction.IsAlive)
                {
                    ((Action<TMessage>) _deliveryAction.Target).Invoke((TMessage) message);
                }
            }
        }

        private class StrongMessageSubscription<TMessage> : IMessageHubSubscription
            where TMessage : class, IMessageHubMessage
        {
            private readonly Action<TMessage> _deliveryAction;
            private readonly Func<TMessage, bool> _messageFilter;

            /// <summary>
            /// Initializes a new instance of the <see cref="StrongMessageSubscription{TMessage}" /> class.
            /// </summary>
            /// <param name="subscriptionToken">The subscription token.</param>
            /// <param name="deliveryAction">The delivery action.</param>
            /// <param name="messageFilter">The message filter.</param>
            /// <exception cref="ArgumentNullException">subscriptionToken
            /// or
            /// deliveryAction
            /// or
            /// messageFilter</exception>
            public StrongMessageSubscription(
                MessageHubSubscriptionToken subscriptionToken,
                Action<TMessage> deliveryAction,
                Func<TMessage, bool> messageFilter)
            {
                SubscriptionToken = subscriptionToken ?? throw new ArgumentNullException(nameof(subscriptionToken));
                _deliveryAction = deliveryAction;
                _messageFilter = messageFilter;
            }

            public MessageHubSubscriptionToken SubscriptionToken { get; }

            public bool ShouldAttemptDelivery(IMessageHubMessage message) => _messageFilter.Invoke((TMessage) message);

            public void Deliver(IMessageHubMessage message) => _deliveryAction.Invoke((TMessage) message);
        }

        #endregion

        #region Subscription dictionary

        private class SubscriptionItem
        {
            public SubscriptionItem(IMessageHubProxy proxy, IMessageHubSubscription subscription)
            {
                Proxy = proxy;
                Subscription = subscription;
            }

            public IMessageHubProxy Proxy { get; }
            public IMessageHubSubscription Subscription { get; }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action.
        /// Messages will be delivered via the specified proxy.
        /// 
        /// All messages of this type will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction </param>
        /// <param name="proxy">Proxy to use when delivering the messages</param>
        /// <returns>MessageSubscription used to unsubscribing</returns>
        public MessageHubSubscriptionToken Subscribe<TMessage>(
            Action<TMessage> deliveryAction,
            bool useStrongReferences = true,
            IMessageHubProxy proxy = null)
            where TMessage : class, IMessageHubMessage
        {
            return Subscribe(deliveryAction, m => true, useStrongReferences, proxy);
        }

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action with the given filter.
        /// Messages will be delivered via the specified proxy.
        /// All references are held with WeakReferences
        /// Only messages that "pass" the filter will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="messageFilter">The message filter.</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction</param>
        /// <param name="proxy">Proxy to use when delivering the messages</param>
        /// <returns>
        /// MessageSubscription used to unsubscribing
        /// </returns>
        public MessageHubSubscriptionToken Subscribe<TMessage>(
            Action<TMessage> deliveryAction,
            Func<TMessage, bool> messageFilter,
            bool useStrongReferences = true,
            IMessageHubProxy proxy = null)
            where TMessage : class, IMessageHubMessage
        {
            if (deliveryAction == null)
                throw new ArgumentNullException(nameof(deliveryAction));

            if (messageFilter == null)
                throw new ArgumentNullException(nameof(messageFilter));

            lock (_subscriptionsPadlock)
            {
                if (!_subscriptions.TryGetValue(typeof(TMessage), out var currentSubscriptions))
                {
                    currentSubscriptions = new List<SubscriptionItem>();
                    _subscriptions[typeof(TMessage)] = currentSubscriptions;
                }

                var subscriptionToken = new MessageHubSubscriptionToken(this, typeof(TMessage));

                IMessageHubSubscription subscription;
                if (useStrongReferences)
                {
                    subscription = new StrongMessageSubscription<TMessage>(
                        subscriptionToken,
                        deliveryAction,
                        messageFilter);
                }
                else
                {
                    subscription = new WeakMessageSubscription<TMessage>(
                        subscriptionToken,
                        deliveryAction,
                        messageFilter);
                }

                currentSubscriptions.Add(new SubscriptionItem(proxy ?? MessageHubDefaultProxy.Instance, subscription));

                return subscriptionToken;
            }
        }

        /// <inheritdoc />
        public void Unsubscribe<TMessage>(MessageHubSubscriptionToken subscriptionToken)
            where TMessage : class, IMessageHubMessage
        {
            if (subscriptionToken == null)
                throw new ArgumentNullException(nameof(subscriptionToken));

            lock (_subscriptionsPadlock)
            {
                if (!_subscriptions.TryGetValue(typeof(TMessage), out var currentSubscriptions))
                    return;

                var currentlySubscribed = currentSubscriptions
                    .Where(sub => ReferenceEquals(sub.Subscription.SubscriptionToken, subscriptionToken))
                    .ToList();

                currentlySubscribed.ForEach(sub => currentSubscriptions.Remove(sub));
            }
        }

        /// <summary>
        /// Publish a message to any subscribers
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="message">Message to deliver</param>
        public void Publish<TMessage>(TMessage message)
            where TMessage : class, IMessageHubMessage
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            List<SubscriptionItem> currentlySubscribed;
            lock (_subscriptionsPadlock)
            {
                if (!_subscriptions.TryGetValue(typeof(TMessage), out var currentSubscriptions))
                    return;

                currentlySubscribed = currentSubscriptions
                    .Where(sub => sub.Subscription.ShouldAttemptDelivery(message))
                    .ToList();
            }

            currentlySubscribed.ForEach(sub =>
            {
                try
                {
                    sub.Proxy.Deliver(message, sub.Subscription);
                }
                catch
                {
                    // Ignore any errors and carry on
                }
            });
        }

        /// <summary>
        /// Publish a message to any subscribers asynchronously
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="message">Message to deliver</param>
        /// <returns>A task with the publish</returns>
        public Task PublishAsync<TMessage>(TMessage message)
            where TMessage : class, IMessageHubMessage
        {
            return Task.Factory.StartNew(() => Publish(message));
        }

        #endregion
    }

    #endregion
}