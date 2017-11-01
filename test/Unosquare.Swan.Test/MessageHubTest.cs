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
    }

    [TestFixture]
    public class MessageHubMessageBaseConstructor : MessageHubTest
    {
        [Test]
        public void NullSender_ThrowsArgumentNullException()
        {
            var content = new SimpleMessageMock(this, "UnoSquare Américas");

            Assert.Throws<ArgumentNullException>(() =>
            {
                var aux = new MessageHubGenericMessage<string>(nullSender, content.Content);
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
    }
}
