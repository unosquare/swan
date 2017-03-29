using NUnit.Framework;
using System.Net.Sockets;
using System.Threading.Tasks;
using Unosquare.Swan.Networking;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ConnectionListenerTest
    {
        private const int Port = 12345;

        [Test]
        public async Task OnConnectionAcceptingTest()
        {
            using (var connectionListener = new ConnectionListener(Port))
            {
                using (var client = new TcpClient())
                {
                    var isAccepting = false;
                    connectionListener.Start();
                    connectionListener.OnConnectionAccepting += (s, e) =>
                    {
                        isAccepting = true;
                    };

                    await client.ConnectAsync("localhost", Port);
                    await Task.Delay(100);
                    Assert.IsTrue(connectionListener.IsListening);
                    Assert.IsTrue(client.Connected);
                    Assert.IsTrue(isAccepting);
                }
            }
        }

        [Test]
        public async Task OnConnectionAcceptedTest()
        {
            using (var connectionListener = new ConnectionListener(Port))
            {
                using (var client = new TcpClient())
                {
                    var isAccepted = false;
                    connectionListener.OnConnectionAccepted += (s, e) =>
                    {
                        isAccepted = true;
                    };
                    connectionListener.Start();
                    await client.ConnectAsync("localhost", Port);
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

            using (var connectionListener = new ConnectionListener(Port))
            {
                using (var client = new TcpClient())
                {
                    var isFailure = false;
                    connectionListener.OnConnectionFailure += (s, e) =>
                    {
                        isFailure = true;
                    };

                    connectionListener.Start();
                    await client.ConnectAsync("localhost", Port);
                    Assert.IsTrue(isFailure);
                }
            }
        }

        [Test]
        public void OnListenerStoppedTest()
        {
            using (var connectionListener = new ConnectionListener(Port))
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