using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unosquare.Swan.Components;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test.MessageHubTests
{
    public abstract class MessageHubTest
    {
        protected object nullSender = null;
        protected object sender = "alexey.turlapov@unosquare.com";
        protected bool cancel;
        protected SimpleMessageMock content;
        protected List<SimpleMessageMock> messagesToSend = new List<SimpleMessageMock>();
    }

    [TestFixture]
    public class MessageHubMessageBaseConstructor : MessageHubTest
    {
        [Test]
        public void NullSender_ThrowsArgumentNullException()
        {
            content = new SimpleMessageMock(this, "Unosquare Américas");

            Assert.Throws<ArgumentNullException>(() =>
            {
                var message = new MessageHubGenericMessage<string>(nullSender, content.Content);
            });
        }

        [Test]
        public void NotNullSender_ReturnsSuccess()
        {
            content = new SimpleMessageMock(this, "Unosquare Américas");
            var message = new MessageHubGenericMessage<string>(sender, content.Content);

            Assert.IsNotNull(message.Sender);
            Assert.IsNotNull(message.Content);
        }
    }

    [TestFixture]
    public class MessageHubCancellableGenericMessageConstructor : MessageHubTest
    {
        [Test]
        public void NullCancel_ThrowsArgumentNullException()
        {
            content = new SimpleMessageMock(this, "Unosquare Américas");

            Assert.Throws<ArgumentNullException>(() =>
            {
                var message = new MessageHubCancellableGenericMessage<string>(sender, content.Content, null);
            });
        }

        [Test]
        public void ValidCancel_ReturnsSuccess()
        {
            Action act = () => cancel = true;
            content = new SimpleMessageMock(this, "Unosquare Américas");

            var message = new MessageHubCancellableGenericMessage<string>(sender, content.Content, act);

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
            var message = "Hello, World!";

            Assert.Throws<ArgumentNullException>(() =>
            {
                var result = new MessageHubSubscriptionToken(null, message.GetType());
            });
        }

        [Test]
        public void MessageType_ThrowsArgumentOutOfRangeException()
        {
            var hub = new MessageHub();
            var message = "Hello, World!";

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var result = new MessageHubSubscriptionToken(hub, message.GetType());
            });
        }
    }

    [TestFixture]
    public class Messages : MessageHubTest
    {
        [Test]
        public void GetMessagesHub_ReturnsMessage()
        {
            Assert.IsNotNull(Runtime.Messages);
        }
    }

    [TestFixture]
    public class SendMessage : MessageHubTest
    {
        [Test]
        public void PublishMessage_ReturnsSuccess()
        {
            var messages = new List<SimpleMessageMock>();

            var token = Runtime.Messages.Subscribe<SimpleMessageMock>(messages.Add);
            Assert.IsNotNull(token);

            var message = new SimpleMessageMock(this, "HOLA");

            Runtime.Messages.Publish(message);

            Assert.IsTrue(messages.Any());
            Assert.AreEqual(message, messages.First());

            Runtime.Messages.Unsubscribe<SimpleMessageMock>(token);

            Runtime.Messages.Publish(message);
            Assert.IsFalse(messages.Skip(1).Any());
        }

        [Test]
        public async Task PublishMessageAsync_ReturnsSuccess()
        {
            var messages = new List<SimpleMessageMock>();

            var token = Runtime.Messages.Subscribe<SimpleMessageMock>(messages.Add);
            Assert.IsNotNull(token);

            var message = new SimpleMessageMock(this, "HOLA");

            await Runtime.Messages.PublishAsync(message);

            Assert.IsTrue(messages.Any());
            Assert.AreEqual(message, messages.First());
        }

        [Test]
        public void NullMessage_ThrowsArgumentNullException()
        {
            var message = new SimpleMessageMock(sender, "Unosquare Américas");
            message = null;

            Assert.Throws<ArgumentNullException>(() =>
            {
                Runtime.Messages.Publish(message);
            });
        }

        [Test]
        public void NotNullMessage_ReturnsSuccess()
        {
            var message = new SimpleMessageMock(sender, "Unosquare Américas");

            Assert.IsNotNull(message.Sender);
            Assert.IsNotNull(message.Content);
        }

        [Test]
        public void NullDeliveryAction_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                Runtime.Messages.Subscribe<SimpleMessageMock>(null, MessageHubDefaultProxy.Instance);
            });
        }
    }
}
