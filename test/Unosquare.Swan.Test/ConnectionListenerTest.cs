using NUnit.Framework;
using System.Net.Sockets;
using System.Threading.Tasks;
using Unosquare.Swan.Networking;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ConnectionListenerTest
    {
        [Test]
        public async Task OnConnectionAcceptingTest()
        {
            using (var connectionListener = new ConnectionListener(12345))
            {
                using (var client = new TcpClient())
                {
                    var isAccepting = false;
                    connectionListener.Start();
                    connectionListener.OnConnectionAccepting += (s, e) =>
                    {
                        Assert.IsTrue(e.Client.Connected);
                        
                        isAccepting = true;
                    };

                    await client.ConnectAsync("localhost", 12345);
                    await Task.Delay(100);

                    Assert.IsTrue(connectionListener.IsListening);
                    Assert.IsTrue(client.Connected);
                    Assert.IsTrue(isAccepting);
                }
            }
        }

        [Test]
        public async Task UsingLoopback_CanListen()
        {
            using (var connectionListener = new ConnectionListener(System.Net.IPAddress.Parse("127.0.0.1"), 12345))
            {
                using (var client = new TcpClient())
                {
                    connectionListener.Start();
                    await client.ConnectAsync("localhost", 12345);
                    await Task.Delay(100);
                    Assert.IsTrue(connectionListener.IsListening);
                    Assert.IsTrue(client.Connected);
                }
            }
        }

        [Test]
        public async Task OnConnectionAcceptedTest()
        {
            using (var connectionListener = new ConnectionListener(12345))
            {
                using (var client = new TcpClient())
                {
                    var isAccepted = false;
                    connectionListener.OnConnectionAccepted += (s, e) =>
                    {
                        isAccepted = true;
                    };
                    connectionListener.Start();
                    await client.ConnectAsync("localhost", 12345);
                    await Task.Delay(100);
                    Assert.IsTrue(connectionListener.IsListening, "Connection Listerner is listening");
                    Assert.IsTrue(client.Connected, "Client is connected");
                    Assert.IsTrue(isAccepted, "The flag was set");
                }
            }
        }

        [Test]
        public async Task OnConnectionFailureTest()
        {
            Assert.Inconclusive("How to throw a failure?");

            using (var connectionListener = new ConnectionListener(12345))
            {
                using (var client = new TcpClient())
                {
                    var isFailure = false;
                    connectionListener.OnConnectionFailure += (s, e) =>
                    {
                        isFailure = true;
                    };

                    connectionListener.Start();
                    await client.ConnectAsync("localhost", 12345);
                    Assert.IsTrue(isFailure);
                }
            }
        }

        [Test]
        public void OnListenerStoppedTest()
        {
            using (var connectionListener = new ConnectionListener(12345))
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
}