// ===============================================================================
// TinyIoC - TinyMessenger
//
// A simple messenger/event aggregator.
//
// http://hg.grumpydev.com/tinyioc
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
    using System.Reflection;

    #region Message Types / Interfaces

    /// <summary>
    /// A TinyMessage to be published/delivered by TinyMessenger
    /// </summary>
    public interface IMessageHubMessage
    {
        /// <summary>
        /// The sender of the message, or null if not supported by the message implementation.
        /// </summary>
        object Sender { get; }
    }

    /// <summary>
    /// Base class for messages that provides weak reference storage of the sender
    /// </summary>
    public abstract class MessageHubMessageBase : IMessageHubMessage
    {
        /// <summary>
        /// Store a WeakReference to the sender just in case anyone is daft enough to
        /// keep the message around and prevent the sender from being collected.
        /// </summary>
        private readonly WeakReference _sender;

        /// <summary>
        /// The sender of the message, or null if not supported by the message implementation.
        /// </summary>
        public object Sender => _sender?.Target;

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
    }

    /// <summary>
    /// Generic message with user specified content
    /// </summary>
    /// <typeparam name="TContent">Content type to store</typeparam>
    public class MessageHubGenericMessage<TContent> : MessageHubMessageBase
    {
        /// <summary>
        /// Contents of the message
        /// </summary>
        public TContent Content { get; protected set; }

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
    }

    /// <summary>
    /// Basic "cancellable" generic message
    /// </summary>
    /// <typeparam name="TContent">Content type to store</typeparam>
    public class MessageHubCancellableGenericMessage<TContent> : MessageHubMessageBase
    {
        /// <summary>
        /// Cancel action
        /// </summary>
        public Action Cancel { get; protected set; }

        /// <summary>
        /// Contents of the message
        /// </summary>
        public TContent Content { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHubCancellableGenericMessage{TContent}"/> class.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="content">The content.</param>
        /// <param name="cancelAction">The cancel action.</param>
        /// <exception cref="System.ArgumentNullException">cancelAction</exception>
        public MessageHubCancellableGenericMessage(object sender, TContent content, Action cancelAction)
            : base(sender)
        {
            Content = content;
            Cancel = cancelAction ?? throw new ArgumentNullException(nameof(cancelAction));
        }
    }

    /// <summary>
    /// Represents an active subscription to a message
    /// </summary>
    public sealed class MessageHubSubscriptionToken : IDisposable
    {
        private readonly WeakReference _hub;
        private readonly Type _messageType;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHubSubscriptionToken"/> class.
        /// </summary>
        /// <param name="hub">The hub.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <exception cref="System.ArgumentNullException">hub</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">messageType</exception>
        public MessageHubSubscriptionToken(IMessageHub hub, Type messageType)
        {
            if (hub == null)
            {
                throw new ArgumentNullException(nameof(hub));
            }

            if (!typeof(IMessageHubMessage).IsAssignableFrom(messageType))
            {
                throw new ArgumentOutOfRangeException(nameof(messageType));
            }

            _hub = new WeakReference(hub);
            _messageType = messageType;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_hub.IsAlive)
            {
                if (_hub.Target is IMessageHub hub)
                {
                    var unsubscribeMethod = typeof(IMessageHub).GetMethod("Unsubscribe",
                        new[] {typeof(MessageHubSubscriptionToken)});
                    unsubscribeMethod = unsubscribeMethod.MakeGenericMethod(_messageType);
                    unsubscribeMethod.Invoke(hub, new object[] {this});
                }
            }

            GC.SuppressFinalize(this);
        }
    }

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
        /// <returns><c>true</c> - ok to send, False - should not attempt to send</returns>
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
        /// <summary>
        /// Singleton instance of the proxy.
        /// </summary>
        public static MessageHubDefaultProxy Instance { get; } = new MessageHubDefaultProxy();

        private MessageHubDefaultProxy()
        {
            // placeholder
        }

        /// <summary>
        /// Delivers the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="subscription">The subscription.</param>
        public void Deliver(IMessageHubMessage message, IMessageHubSubscription subscription)
        {
            subscription.Deliver(message);
        }
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
        /// All references are held with WeakReferences
        /// 
        /// All messages of this type will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        MessageHubSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction)
            where TMessage : class, IMessageHubMessage;

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action.
        /// Messages will be delivered via the specified proxy.
        /// All references (apart from the proxy) are held with WeakReferences
        /// 
        /// All messages of this type will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="proxy">Proxy to use when delivering the messages</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        MessageHubSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, IMessageHubProxy proxy)
            where TMessage : class, IMessageHubMessage;

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action.
        /// 
        /// All messages of this type will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction </param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        MessageHubSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, bool useStrongReferences)
            where TMessage : class, IMessageHubMessage;

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
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        MessageHubSubscriptionToken Subscribe<TMessage>(
            Action<TMessage> deliveryAction, 
            bool useStrongReferences,
            IMessageHubProxy proxy)
            where TMessage : class, IMessageHubMessage;

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action with the given filter.
        /// All references are held with WeakReferences
        /// Only messages that "pass" the filter will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="messageFilter">The message filter.</param>
        /// <returns>
        /// TinyMessageSubscription used to unsubscribing
        /// </returns>
        MessageHubSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction,
            Func<TMessage, bool> messageFilter)
            where TMessage : class, IMessageHubMessage;

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action with the given filter.
        /// Messages will be delivered via the specified proxy.
        /// All references (apart from the proxy) are held with WeakReferences
        /// Only messages that "pass" the filter will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="messageFilter">The message filter.</param>
        /// <param name="proxy">Proxy to use when delivering the messages</param>
        /// <returns>
        /// TinyMessageSubscription used to unsubscribing
        /// </returns>
        MessageHubSubscriptionToken Subscribe<TMessage>(
            Action<TMessage> deliveryAction,
            Func<TMessage, bool> messageFilter, 
            IMessageHubProxy proxy)
            where TMessage : class, IMessageHubMessage;

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action with the given filter.
        /// All references are held with WeakReferences
        /// Only messages that "pass" the filter will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="messageFilter">The message filter.</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction</param>
        /// <returns>
        /// TinyMessageSubscription used to unsubscribing
        /// </returns>
        MessageHubSubscriptionToken Subscribe<TMessage>(
            Action<TMessage> deliveryAction,
            Func<TMessage, bool> messageFilter, 
            bool useStrongReferences) 
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
        /// TinyMessageSubscription used to unsubscribing
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

    /// <summary>
    /// Messenger hub responsible for taking subscriptions/publications and delivering of messages.
    /// </summary>
    public sealed class MessageHub : IMessageHub
    {
        #region Private Types and Interfaces

        private class WeakTinyMessageSubscription<TMessage> : IMessageHubSubscription
            where TMessage : class, IMessageHubMessage
        {
            private readonly WeakReference _deliveryAction;
            private readonly WeakReference _messageFilter;

            public MessageHubSubscriptionToken SubscriptionToken { get; }

            public bool ShouldAttemptDelivery(IMessageHubMessage message)
            {
                if (!(message is TMessage) || !_deliveryAction.IsAlive || !_messageFilter.IsAlive)
                    return false;

                return ((Func<TMessage, bool>) _messageFilter.Target).Invoke((TMessage) message);
            }

            public void Deliver(IMessageHubMessage message)
            {
                if (!(message is TMessage))
                {
                    throw new ArgumentException("Message is not the correct type");
                }

                if (!_deliveryAction.IsAlive)
                {
                    return;
                }

                ((Action<TMessage>) _deliveryAction.Target).Invoke((TMessage) message);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="WeakTinyMessageSubscription{TMessage}"/> class.
            /// </summary>
            /// <param name="subscriptionToken">The subscription token.</param>
            /// <param name="deliveryAction">The delivery action.</param>
            /// <param name="messageFilter">The message filter.</param>
            /// <exception cref="System.ArgumentNullException">
            /// subscriptionToken
            /// or
            /// deliveryAction
            /// or
            /// messageFilter
            /// </exception>
            public WeakTinyMessageSubscription(
                MessageHubSubscriptionToken subscriptionToken,
                Action<TMessage> deliveryAction, 
                Func<TMessage, bool> messageFilter)
            {
                if (deliveryAction == null)
                {
                    throw new ArgumentNullException(nameof(deliveryAction));
                }

                if (messageFilter == null)
                {
                    throw new ArgumentNullException(nameof(messageFilter));
                }

                SubscriptionToken = subscriptionToken ?? throw new ArgumentNullException(nameof(subscriptionToken));
                _deliveryAction = new WeakReference(deliveryAction);
                _messageFilter = new WeakReference(messageFilter);
            }
        }

        private class StrongTinyMessageSubscription<TMessage> : IMessageHubSubscription
            where TMessage : class, IMessageHubMessage
        {
            private readonly Action<TMessage> _deliveryAction;
            private readonly Func<TMessage, bool> _messageFilter;

            public MessageHubSubscriptionToken SubscriptionToken { get; }

            public bool ShouldAttemptDelivery(IMessageHubMessage message)
            {
                if (!(message is TMessage))
                {
                    return false;
                }

                return _messageFilter.Invoke((TMessage) message);
            }

            public void Deliver(IMessageHubMessage message)
            {
                if (!(message is TMessage))
                {
                    throw new ArgumentException("Message is not the correct type");
                }

                _deliveryAction.Invoke((TMessage) message);
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="StrongTinyMessageSubscription{TMessage}"/> class.
            /// </summary>
            /// <param name="subscriptionToken">The subscription token.</param>
            /// <param name="deliveryAction">The delivery action.</param>
            /// <param name="messageFilter">The message filter.</param>
            /// <exception cref="System.ArgumentNullException">
            /// subscriptionToken
            /// or
            /// deliveryAction
            /// or
            /// messageFilter
            /// </exception>
            public StrongTinyMessageSubscription(
                MessageHubSubscriptionToken subscriptionToken,
                Action<TMessage> deliveryAction,
                Func<TMessage, bool> messageFilter)
            {
                SubscriptionToken = subscriptionToken ?? throw new ArgumentNullException(nameof(subscriptionToken));
                _deliveryAction = deliveryAction ?? throw new ArgumentNullException(nameof(deliveryAction));
                _messageFilter = messageFilter ?? throw new ArgumentNullException(nameof(messageFilter));
            }
        }

        #endregion

        #region Subscription dictionary

        private class SubscriptionItem
        {
            public IMessageHubProxy Proxy { get; }
            public IMessageHubSubscription Subscription { get; }

            public SubscriptionItem(IMessageHubProxy proxy, IMessageHubSubscription subscription)
            {
                Proxy = proxy;
                Subscription = subscription;
            }
        }

        private readonly object _subscriptionsPadlock = new object();

        private readonly Dictionary<Type, List<SubscriptionItem>> _subscriptions =
            new Dictionary<Type, List<SubscriptionItem>>();

        #endregion

        #region Public API

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action.
        /// All references are held with strong references
        /// 
        /// All messages of this type will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        public MessageHubSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction)
            where TMessage : class, IMessageHubMessage
        {
            return AddSubscriptionInternal(deliveryAction, (m) => true, true, MessageHubDefaultProxy.Instance);
        }

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action.
        /// Messages will be delivered via the specified proxy.
        /// All references (apart from the proxy) are held with strong references
        /// 
        /// All messages of this type will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="proxy">Proxy to use when delivering the messages</param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        public MessageHubSubscriptionToken Subscribe<TMessage>(Action<TMessage> deliveryAction, IMessageHubProxy proxy)
            where TMessage : class, IMessageHubMessage
        {
            return AddSubscriptionInternal(deliveryAction, (m) => true, true, proxy);
        }

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action.
        /// 
        /// All messages of this type will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction </param>
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        public MessageHubSubscriptionToken Subscribe<TMessage>(
            Action<TMessage> deliveryAction, 
            bool useStrongReferences)
            where TMessage : class, IMessageHubMessage
        {
            return AddSubscriptionInternal(
                deliveryAction, 
                (m) => true, 
                useStrongReferences,
                MessageHubDefaultProxy.Instance);
        }

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
        /// <returns>TinyMessageSubscription used to unsubscribing</returns>
        public MessageHubSubscriptionToken Subscribe<TMessage>(
            Action<TMessage> deliveryAction, 
            bool useStrongReferences,
            IMessageHubProxy proxy) 
            where TMessage : class, IMessageHubMessage
        {
            return AddSubscriptionInternal(deliveryAction, (m) => true, useStrongReferences, proxy);
        }

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action with the given filter.
        /// All references are held with WeakReferences
        /// Only messages that "pass" the filter will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="messageFilter">The message filter.</param>
        /// <returns>
        /// TinyMessageSubscription used to unsubscribing
        /// </returns>
        public MessageHubSubscriptionToken Subscribe<TMessage>(
            Action<TMessage> deliveryAction,
            Func<TMessage, bool> messageFilter)
            where TMessage : class, IMessageHubMessage
        {
            return AddSubscriptionInternal(
                deliveryAction, 
                messageFilter, 
                true,
                MessageHubDefaultProxy.Instance);
        }

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action with the given filter.
        /// Messages will be delivered via the specified proxy.
        /// All references (apart from the proxy) are held with WeakReferences
        /// Only messages that "pass" the filter will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="messageFilter">The message filter.</param>
        /// <param name="proxy">Proxy to use when delivering the messages</param>
        /// <returns>
        /// TinyMessageSubscription used to unsubscribing
        /// </returns>
        public MessageHubSubscriptionToken Subscribe<TMessage>(
            Action<TMessage> deliveryAction,
            Func<TMessage, bool> messageFilter, 
            IMessageHubProxy proxy) 
            where TMessage : class, IMessageHubMessage
        {
            return AddSubscriptionInternal(deliveryAction, messageFilter, true, proxy);
        }

        /// <summary>
        /// Subscribe to a message type with the given destination and delivery action with the given filter.
        /// All references are held with WeakReferences
        /// Only messages that "pass" the filter will be delivered.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="deliveryAction">Action to invoke when message is delivered</param>
        /// <param name="messageFilter">The message filter.</param>
        /// <param name="useStrongReferences">Use strong references to destination and deliveryAction</param>
        /// <returns>
        /// TinyMessageSubscription used to unsubscribing
        /// </returns>
        public MessageHubSubscriptionToken Subscribe<TMessage>(
            Action<TMessage> deliveryAction,
            Func<TMessage, bool> messageFilter, 
            bool useStrongReferences) 
            where TMessage : class, IMessageHubMessage
        {
            return AddSubscriptionInternal(
                deliveryAction, 
                messageFilter, 
                useStrongReferences,
                MessageHubDefaultProxy.Instance);
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
        /// TinyMessageSubscription used to unsubscribing
        /// </returns>
        public MessageHubSubscriptionToken Subscribe<TMessage>(
            Action<TMessage> deliveryAction,
            Func<TMessage, bool> messageFilter, 
            bool useStrongReferences, 
            IMessageHubProxy proxy)
            where TMessage : class, IMessageHubMessage
        {
            return AddSubscriptionInternal(deliveryAction, messageFilter, useStrongReferences, proxy);
        }

        /// <summary>
        /// Unsubscribe from a particular message type.
        /// 
        /// Does not throw an exception if the subscription is not found.
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="subscriptionToken">Subscription token received from Subscribe</param>
        public void Unsubscribe<TMessage>(MessageHubSubscriptionToken subscriptionToken)
            where TMessage : class, IMessageHubMessage
        {
            RemoveSubscriptionInternal<TMessage>(subscriptionToken);
        }

        /// <summary>
        /// Publish a message to any subscribers
        /// </summary>
        /// <typeparam name="TMessage">Type of message</typeparam>
        /// <param name="message">Message to deliver</param>
        public void Publish<TMessage>(TMessage message) 
            where TMessage : class, IMessageHubMessage
        {
            PublishInternal(message);
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
            return Task.Factory.StartNew(() =>
            {
                PublishInternal(message);
            });
        }

        #endregion

        #region Internal Methods

        private MessageHubSubscriptionToken AddSubscriptionInternal<TMessage>(
            Action<TMessage> deliveryAction,
            Func<TMessage, bool> messageFilter, 
            bool strongReference, 
            IMessageHubProxy proxy)
            where TMessage : class, IMessageHubMessage
        {
            if (deliveryAction == null)
                throw new ArgumentNullException(nameof(deliveryAction));

            if (messageFilter == null)
                throw new ArgumentNullException(nameof(messageFilter));

            if (proxy == null)
                throw new ArgumentNullException(nameof(proxy));

            lock (_subscriptionsPadlock)
            {
                if (!_subscriptions.TryGetValue(typeof(TMessage), out var currentSubscriptions))
                {
                    currentSubscriptions = new List<SubscriptionItem>();
                    _subscriptions[typeof(TMessage)] = currentSubscriptions;
                }

                var subscriptionToken = new MessageHubSubscriptionToken(this, typeof(TMessage));

                IMessageHubSubscription subscription;
                if (strongReference)
                {
                    subscription = new StrongTinyMessageSubscription<TMessage>(
                        subscriptionToken,
                        deliveryAction,
                        messageFilter);
                }
                else
                {
                    subscription = new WeakTinyMessageSubscription<TMessage>(
                        subscriptionToken,
                        deliveryAction,
                        messageFilter);
                }

                currentSubscriptions.Add(new SubscriptionItem(proxy, subscription));

                return subscriptionToken;
            }
        }

        private void RemoveSubscriptionInternal<TMessage>(MessageHubSubscriptionToken subscriptionToken)
            where TMessage : class, IMessageHubMessage
        {
            if (subscriptionToken == null)
                throw new ArgumentNullException(nameof(subscriptionToken));

            lock (_subscriptionsPadlock)
            {
                if (!_subscriptions.TryGetValue(typeof(TMessage), out var currentSubscriptions))
                    return;

                var currentlySubscribed =
                    currentSubscriptions.Where(
                        sub => ReferenceEquals(sub.Subscription.SubscriptionToken, subscriptionToken)).ToList();

                currentlySubscribed.ForEach(sub => currentSubscriptions.Remove(sub));
            }
        }

        private void PublishInternal<TMessage>(TMessage message)
            where TMessage : class, IMessageHubMessage
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            List<SubscriptionItem> currentlySubscribed;
            lock (_subscriptionsPadlock)
            {
                if (!_subscriptions.TryGetValue(typeof(TMessage), out var currentSubscriptions))
                    return;

                currentlySubscribed = (from sub in currentSubscriptions
                    where sub.Subscription.ShouldAttemptDelivery(message)
                    select sub).ToList();
            }

            currentlySubscribed.ForEach(sub =>
            {
                try
                {
                    sub.Proxy.Deliver(message, sub.Subscription);
                }
                catch (Exception)
                {
                    // Ignore any errors and carry on
                }
            });
        }

        #endregion
    }

    #endregion
}