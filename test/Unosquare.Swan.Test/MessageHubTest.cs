namespace Unosquare.Swan.Test.MessageHubTests
{
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Components;
    using Mocks;

    [TestFixture]
    public class MessageHubMessageBaseConstructor
    {
        [Test]
        public void NullSender_ThrowsArgumentNullException()
        {
            var content = new SimpleMessageMock(this);

            Assert.Throws<ArgumentNullException>(() => new MessageHubGenericMessage<string>(null, content.Content));
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
    public class MessageHubCancellableGenericMessageConstructor
    {
        [Test]
        public void NullCancel_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var message = new MessageHubCancellableGenericMessage<string>(this, "Unosquare Américas", null);
            });
        }

        [Test]
        public void ValidCancel_ReturnsSuccess()
        {
            // TODO: Rewrite this action to really check the cancel
            bool cancel;
            var message =
                new MessageHubCancellableGenericMessage<string>(this, "Unosquare Américas", () => cancel = true);

            Assert.IsNotNull(message.Sender);
            Assert.IsNotNull(message.Content);
        }
    }

    [TestFixture]
    public class MessageHubSubscriptionTokenConstructor
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
    public class Messages
    {
        [Test]
        public void GetMessagesHub_ReturnsMessage()
        {
            Assert.IsNotNull(Runtime.Messages);
        }
    }

    [TestFixture]
    public class SendMessage
    {
        private readonly List<SimpleMessageMock> _messagesToSend = new List<SimpleMessageMock>();

        [Test]
        public void PublishMessage_MessagePublished()
        {
            var message = new SimpleMessageMock(this);
            var token = Runtime.Messages.Subscribe<SimpleMessageMock>(_messagesToSend.Add);

            Assert.IsNotNull(token);

            Runtime.Messages.Publish(message);

            Assert.IsTrue(_messagesToSend.Any());
            Assert.AreEqual(message, _messagesToSend.First());
        }

        [Test]
        public void PublishMessageWhenUnsubscribed_MessageNotPublished()
        {
            var message = new SimpleMessageMock(this);
            var token = Runtime.Messages.Subscribe<SimpleMessageMock>(_messagesToSend.Add);

            Assert.IsNotNull(token);

            Runtime.Messages.Unsubscribe<SimpleMessageMock>(token);
            Runtime.Messages.Publish(message);

            Assert.IsTrue(_messagesToSend.Any());
        }

        [Test]
        public async Task PublishMessageAsync_MessagePublished()
        {
            var messagesToSend = new List<SimpleMessageMock>();

            var token = Runtime.Messages.Subscribe<SimpleMessageMock>(messagesToSend.Add);
            Assert.IsNotNull(token);

            var message = new SimpleMessageMock(this);

            await Runtime.Messages.PublishAsync(message);

            Assert.IsTrue(messagesToSend.Any());
            Assert.AreEqual(message, messagesToSend.First());
        }

        [Test]
        public void NullMessage_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
                Runtime.Messages.Publish((SimpleMessageMock) null));
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

            [Test]
            public void NullDeliveryAction_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Runtime.Messages.Subscribe<SimpleMessageMock>(null, x => true));
            }

            [Test]
            public void NullMessageFilter_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Runtime.Messages.Subscribe<SimpleMessageMock>(_messagesToSend.Add, null));
            }

            [Test]
            public void StrongReferenceFalse_ReturnsToken()
            {
                var token = Runtime.Messages.Subscribe<SimpleMessageMock>(
                    _messagesToSend.Add, x => false,
                    false,
                    MessageHubDefaultProxy.Instance);

                Assert.IsNotNull(token);
            }

            [Test]
            public void DeliveryActionAndStrongReferencesTrue_ReturnsToken()
            {
                var token = Runtime.Messages.Subscribe<SimpleMessageMock>(_messagesToSend.Add);

                Assert.IsNotNull(token);
            }

            [Test]
            public void DeliveryActionAndStrongReferencesFalse_ReturnsToken()
            {
                var token = Runtime.Messages.Subscribe<SimpleMessageMock>(_messagesToSend.Add, false);

                Assert.IsNotNull(token);
            }

            [Test]
            public void DeliveryActionWithStrongReferencesTrueAndProxy_ReturnsToken()
            {
                var token = Runtime.Messages.Subscribe<SimpleMessageMock>(
                    _messagesToSend.Add,
                    true,
                    MessageHubDefaultProxy.Instance);

                Assert.IsNotNull(token);
            }

            [Test]
            public void DeliveryActionWithStrongReferencesFalseAndProxy_ReturnsToken()
            {
                var token = Runtime.Messages.Subscribe<SimpleMessageMock>(
                    _messagesToSend.Add,
                    false,
                    MessageHubDefaultProxy.Instance);

                Assert.IsNotNull(token);
            }

            [Test]
            public void DeliveryActionAndMessageFilter_ReturnsToken()
            {
                var token = Runtime.Messages.Subscribe<SimpleMessageMock>(_messagesToSend.Add, x => true);

                Assert.IsNotNull(token);
            }

            [Test]
            public void DeliveryActionWithFuncAndStrongReferencesTrue_ReturnsToken()
            {
                var token = Runtime.Messages.Subscribe<SimpleMessageMock>(_messagesToSend.Add, x => true);

                Assert.IsNotNull(token);
            }

            [Test]
            public void NullToken_ThrowsArgumentNullException()
            {
                Assert.Throws<ArgumentNullException>(() =>
                    Runtime.Messages.Unsubscribe<SimpleMessageMock>(null));
            }

            [Test]
            public void PublishWithStrongReference_ReturnsMessagePublished()
            {
                var messages = new List<SimpleMessageMock>();
                Runtime.Messages.Subscribe<SimpleMessageMock>(messages.Add);
                var message = new SimpleMessageMock(this);

                Runtime.Messages.Publish(message);

                Assert.IsTrue(messages.Any());
                Assert.AreEqual(message, messages.First());
            }

            [Test]
            public void PublishWithWeakReference_ReturnsMessagePublished()
            {
                var messages = new List<SimpleMessageMock>();
                Runtime.Messages.Subscribe<SimpleMessageMock>(messages.Add, false);
                var message = new SimpleMessageMock(this);

                Runtime.Messages.Publish(message);

                Assert.IsTrue(messages.Any());
                Assert.AreEqual(message, messages.First());
            }
        }
    }
}