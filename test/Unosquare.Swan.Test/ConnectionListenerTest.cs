using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Unosquare.Swan.Networking;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ConnectionListenerTest
    {
        private ConnectionListener connectionListener;
        private int port = 12345;
        private TcpClient client;

        [SetUp]
        public void InitSetUp()
        {
            connectionListener = new ConnectionListener(port);
            client = new TcpClient();
        }

        [Test]
        public async Task OnConnectionAcceptingTest()
        {
            var isAccepting = false;
            connectionListener.Start();
            connectionListener.OnConnectionAccepting += (s, e) =>
            {
                isAccepting = true;
            };
            await client.ConnectAsync("localhost", port);
            Assert.IsTrue(connectionListener.IsListening);
            Assert.IsTrue(client.Connected);
            Assert.IsTrue(isAccepting);
        }

        [Test]
        public async Task OnConnectionAcceptedTest()
        {
            var isAccepted = false;
            connectionListener.OnConnectionAccepted += (s, e) =>
            {
                isAccepted = true;
            };
            connectionListener.Start();
            await client.ConnectAsync("localhost", port);
            Assert.IsTrue(connectionListener.IsListening);
            Assert.IsTrue(client.Connected);
            Assert.IsTrue(isAccepted);
        }

        [Test]
        public async Task OnConnectionFailureTest()
        {
            var isFailure = false;
            connectionListener.OnConnectionFailure += (s, e) => 
            {
                isFailure = true;
            };

            connectionListener.Start();
            await client.ConnectAsync("localhost", 1234);
            Assert.IsTrue(isFailure);
        }

        [Test]
        public void OnListenerStoppedTest()
        {
            var isStopped = false;
            connectionListener.Start();
            connectionListener.OnListenerStopped += (s, e) =>
            {
                isStopped = true;
            };
            Assert.IsTrue(connectionListener.IsListening);
            connectionListener.Stop();
            Assert.IsFalse(connectionListener.IsListening);
            Assert.IsTrue(isStopped);
        }
    }
}