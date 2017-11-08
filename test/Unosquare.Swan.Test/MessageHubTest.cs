using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.Swan.Components;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test.MessageHubTests
{
    [TestFixture]
    public class MessageHubMessageBaseConstructor
    {
        [Test]
        public void NullSender_ThrowsArgumentNullException()
        {
            var content = new SimpleMessageMock(this, "Unosquare Américas");

            Assert.Throws<ArgumentNullException>(() => new MessageHubGenericMessage<string>(null, content.Content));
        }

        [Test]
        public void NotNullSender_ReturnsSuccess()
        {
            var content = new SimpleMessageMock(this, "Unosquare Américas");
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
                new MessageHubCancellableGenericMessage<string>(this, "Unosquare Américas", null));
        }

        [Test]
        public void ValidCancel_ReturnsSuccess()
        {
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
        private List<SimpleMessageMock> messagesToSend = new List<SimpleMessageMock>();

        [Test]
        public void PublishMessage_MessagePublished()
        {
            var message = new SimpleMessageMock(this, "Unosquare Labs");
            var token = Runtime.Messages.Subscribe<SimpleMessageMock>(messagesToSend.Add);

            Assert.IsNotNull(token);

            Runtime.Messages.Publish(message);

            Assert.IsTrue(messagesToSend.Any());
            Assert.AreEqual(message, messagesToSend.First());
        }

        [Test]
        public void PublishMessageWhenUnsubscribed_MessageNotPublished()
        {
            var message = new SimpleMessageMock(this, "Unosquare Labs");
            var token = Runtime.Messages.Subscribe<SimpleMessageMock>(messagesToSend.Add);

            Assert.IsNotNull(token);

            Runtime.Messages.Unsubscribe<SimpleMessageMock>(token);
            Runtime.Messages.Publish(message);

            Assert.IsTrue(messagesToSend.Any());
        }

        [Test]
        public async Task PublishMessageAsync_ReturnsSuccess()
        {
            var messagesToSend = new List<SimpleMessageMock>();

            var token = Runtime.Messages.Subscribe<SimpleMessageMock>(messagesToSend.Add);
            Assert.IsNotNull(token);

            var message = new SimpleMessageMock(this, "Unosquare Labs");

            await Runtime.Messages.PublishAsync(message);

            Assert.IsTrue(messagesToSend.Any());
            Assert.AreEqual(message, messagesToSend.First());
        }

        [Test]
        public void NullMessage_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                SimpleMessageMock message = null;
                Runtime.Messages.Publish(message);
            });
        }

        [Test]
        public void NotNullMessage_ReturnsSuccess()
        {
            var message = new SimpleMessageMock(this, "Unosquare Américas");

            Assert.IsNotNull(message.Sender);
            Assert.IsNotNull(message.Content);
        }

        [Test]
        public void NullDeliveryAction_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var result = Runtime.Messages.Subscribe<SimpleMessageMock>(null, x => true);
            });
        }

        public void NullMessageFilter_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var result = Runtime.Messages.Subscribe<SimpleMessageMock>(messagesToSend.Add, null);
            });
        }

        [Test]
        public void StrongReferenceFalse_ReturnsToken()
        {
            var token = Runtime.Messages.Subscribe<SimpleMessageMock>(messagesToSend.Add, x => false, false,
                MessageHubDefaultProxy.Instance);

            Assert.IsNotNull(token);
        }

        [Test]
        public void DeliveryActionAndStrongReferencesTrue_ReturnsToken()
        {
            var token = Runtime.Messages.Subscribe<SimpleMessageMock>(messagesToSend.Add);

            Assert.IsNotNull(token);
        }

        [Test]
        public void DeliveryActionAndStrongReferencesFalse_ReturnsToken()
        {
            var token = Runtime.Messages.Subscribe<SimpleMessageMock>(messagesToSend.Add, false);

            Assert.IsNotNull(token);
        }

        [Test]
        public void DeliveryActionWithStrongReferencesTrueAndProxy_ReturnsToken()
        {
            var token = Runtime.Messages.Subscribe<SimpleMessageMock>(messagesToSend.Add, true,
                MessageHubDefaultProxy.Instance);

            Assert.IsNotNull(token);
        }

        [Test]
        public void DeliveryActionWithStrongReferencesFalseAndProxy_ReturnsToken()
        {
            var token = Runtime.Messages.Subscribe<SimpleMessageMock>(messagesToSend.Add, false,
                MessageHubDefaultProxy.Instance);

            Assert.IsNotNull(token);
        }

        [Test]
        public void DeliveryActionAndMessageFilter_ReturnsToken()
        {
            var token = Runtime.Messages.Subscribe<SimpleMessageMock>(messagesToSend.Add, x => true);

            Assert.IsNotNull(token);
        }

        [Test]
        public void DeliveryActionWithFuncAndStrongReferencesTrue_ReturnsToken()
        {
            var token = Runtime.Messages.Subscribe<SimpleMessageMock>(messagesToSend.Add, x => true);

            Assert.IsNotNull(token);
        }

        [Test]
        public void NullToken_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                Runtime.Messages.Unsubscribe<SimpleMessageMock>(null);
            });
        }

        [Test]
        public void PublishWithStrongReference_ReturnsMessagePublished()
        {
            var messages = new List<SimpleMessageMock>();
            Runtime.Messages.Subscribe<SimpleMessageMock>(messages.Add);
            var message = new SimpleMessageMock(this, "Unosquare Américas");

            Runtime.Messages.Publish(message);

            Assert.IsTrue(messages.Any());
            Assert.AreEqual(message, messages.First());
        }

        [Test]
        public void PublishWithWeakReference_ReturnsMessagePublished()
        {
            var messages = new List<SimpleMessageMock>();
            Runtime.Messages.Subscribe<SimpleMessageMock>(messages.Add, false);
            var message = new SimpleMessageMock(this, "Unosquare Américas");

            Runtime.Messages.Publish(message);

            Assert.IsTrue(messages.Any());
            Assert.AreEqual(message, messages.First());
        }
    }
}