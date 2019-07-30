namespace Unosquare.Swan.Test.MessageHubTests
{
    using Components;
    using Mocks;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public abstract class MessageHubTest
    {
        [SetUp]
        public void OnSetup()
        {
            DependencyContainer.Current.Register<IMessageHub, MessageHub>();
        }
    }

    [TestFixture]
    public class MessageHubMessageBaseConstructor : MessageHubTest
    {
        [Test]
        public void NullSender_ThrowsArgumentNullException()
        {
            var content = new SimpleMessageMock(this);

            Assert.Throws<ArgumentNullException>(() =>
            {
                var result = new MessageHubGenericMessage<string>(null, content.Content);
            });
        }

        [Test]
        public void NotNullSender_ReturnsSuccess()
        {
            var content = new SimpleMessageMock(this);
            var message = new MessageHubGenericMessage<string>(this, content.Content);

            Assert.IsNotNull(message.Sender);
            Assert.IsNotNull(message.Content);
        }
    }

    [TestFixture]
    public class MessageHubSubscriptionTokenConstructor : MessageHubTest
    {
        [Test]
        public void NullHub_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var result = new MessageHubSubscriptionToken(null, typeof(string));
            });
        }

        [Test]
        public void MessageType_ThrowsArgumentOutOfRangeException()
        {
            var hub = new MessageHub();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var result = new MessageHubSubscriptionToken(hub, typeof(string));
            });
        }
    }

    [TestFixture]
    public class SendMessage : MessageHubTest
    {
        private readonly List<SimpleMessageMock> _messagesToSend = new List<SimpleMessageMock>();
        private readonly MessageHub _messageHub = DependencyContainer.Current.Resolve<IMessageHub>() as MessageHub;

        [Test]
        public void PublishMessage_MessagePublished()
        {
            var message = new SimpleMessageMock(this);
            var token = _messageHub.Subscribe<SimpleMessageMock>(_messagesToSend.Add);

            Assert.IsNotNull(token);

            _messageHub.Publish(message);

            Assert.IsTrue(_messagesToSend.Any());
            Assert.AreEqual(message, _messagesToSend.First());
        }

        [Test]
        public void PublishMessageWhenUnsubscribed_MessageNotPublished()
        {
            var hub = new MessageHub();
            var list = new List<SimpleMessageMock>();
            var message = new SimpleMessageMock(this);
            var token = hub.Subscribe<SimpleMessageMock>(list.Add);

            hub.Unsubscribe<SimpleMessageMock>(token);
            hub.Publish(message);

            Assert.IsFalse(list.Any());
        }

        [Test]
        public async Task PublishMessageAsync_MessagePublished()
        {
            var messagesToSend = new List<SimpleMessageMock>();

            var token = _messageHub.Subscribe<SimpleMessageMock>(messagesToSend.Add);
            Assert.IsNotNull(token);

            var message = new SimpleMessageMock(this);

            await _messageHub.PublishAsync(message);

            Assert.IsTrue(messagesToSend.Any());
            Assert.AreEqual(message, messagesToSend.First());
        }

        [Test]
        public void NullMessage_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _messageHub.Publish((SimpleMessageMock)null));
        }

        [Test]
        public void NotNullMessage_ReturnsSuccess()
        {
            var message = new SimpleMessageMock(this);

            Assert.IsNotNull(message.Sender);
            Assert.IsNotNull(message.Content);
        }

        [TestFixture]
        public class Subscribe
        {
            private readonly List<SimpleMessageMock> _messagesToSend = new List<SimpleMessageMock>();
            private readonly MessageHub _messageHub = DependencyContainer.Current.Resolve<IMessageHub>() as MessageHub;

            [Test]
            public void NullDeliveryAction_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    _messageHub.Subscribe<SimpleMessageMock>(null, x => true));
            }

            [Test]
            public void NullMessageFilter_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    _messageHub.Subscribe<SimpleMessageMock>(_messagesToSend.Add, null));
            }

            [Test]
            public void StrongReferenceFalse_ReturnsToken()
            {
                var token = _messageHub.Subscribe<SimpleMessageMock>(
                    _messagesToSend.Add,
                    x => false,
                    false,
                    MessageHubDefaultProxy.Instance);

                Assert.IsNotNull(token);
            }

            [Test]
            public void DeliveryActionAndStrongReferencesTrue_ReturnsToken()
            {
                var token = _messageHub.Subscribe<SimpleMessageMock>(_messagesToSend.Add);

                Assert.IsNotNull(token);
            }

            [Test]
            public void DeliveryActionAndStrongReferencesFalse_ReturnsToken()
            {
                var token = _messageHub.Subscribe<SimpleMessageMock>(_messagesToSend.Add, false);

                Assert.IsNotNull(token);
            }

            [Test]
            public void DeliveryActionWithStrongReferencesTrueAndProxy_ReturnsToken()
            {
                var token = _messageHub.Subscribe<SimpleMessageMock>(
                    _messagesToSend.Add,
                    true,
                    MessageHubDefaultProxy.Instance);

                Assert.IsNotNull(token);
            }

            [Test]
            public void DeliveryActionWithStrongReferencesFalseAndProxy_ReturnsToken()
            {
                var token = _messageHub.Subscribe<SimpleMessageMock>(
                    _messagesToSend.Add,
                    false,
                    MessageHubDefaultProxy.Instance);

                Assert.IsNotNull(token);
            }

            [Test]
            public void DeliveryActionAndMessageFilter_ReturnsToken()
            {
                var token = _messageHub.Subscribe<SimpleMessageMock>(_messagesToSend.Add, x => true);

                Assert.IsNotNull(token);
            }

            [Test]
            public void DeliveryActionWithFuncAndStrongReferencesTrue_ReturnsToken()
            {
                var token = _messageHub.Subscribe<SimpleMessageMock>(_messagesToSend.Add, x => true);

                Assert.IsNotNull(token);
            }

            [Test]
            public void NullToken_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    _messageHub.Unsubscribe<SimpleMessageMock>(null));
            }

            [Test]
            public void PublishWithStrongReference_ReturnsMessagePublished()
            {
                var messages = new List<SimpleMessageMock>();
                _messageHub.Subscribe<SimpleMessageMock>(messages.Add);
                var message = new SimpleMessageMock(this);

                _messageHub.Publish(message);

                Assert.IsTrue(messages.Any());
                Assert.AreEqual(message, messages.First());
            }

            [Test]
            public void PublishWithWeakReference_ReturnsMessagePublished()
            {
                var messages = new List<SimpleMessageMock>();
                _messageHub.Subscribe<SimpleMessageMock>(messages.Add, false);
                var message = new SimpleMessageMock(this);

                _messageHub.Publish(message);

                Assert.IsTrue(messages.Any());
                Assert.AreEqual(message, messages.First());
            }
        }
    }
}