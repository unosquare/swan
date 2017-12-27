namespace Unosquare.Swan.Components
{
    using System;
#if NETSTANDARD1_3 || UWP
    using System.Reflection;
#endif

    /// <summary>
    /// Represents an active subscription to a message
    /// </summary>
    public sealed class MessageHubSubscriptionToken
        : IDisposable
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
            if (_hub.IsAlive && _hub.Target is IMessageHub hub)
            {
                var unsubscribeMethod = typeof(IMessageHub).GetMethod(nameof(IMessageHub.Unsubscribe),
                    new[] {typeof(MessageHubSubscriptionToken)});
                unsubscribeMethod = unsubscribeMethod.MakeGenericMethod(_messageType);
                unsubscribeMethod.Invoke(hub, new object[] {this});
            }

            GC.SuppressFinalize(this);
        }
    }
}