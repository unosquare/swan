using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Unosquare.Swan.Test.Mocks;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class MessageHubTest
    {
        [Test]
        public void GetMessageHubTest()
        {
            Assert.IsNotNull(Runtime.Messages);
        }

        [Test]
        public void SendMessageTest()
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
        public async Task SendMessageAsyncTest()
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
