using NUnit.Framework;
using System.Net.Sockets;
using System.Threading.Tasks;
using Unosquare.Swan.Networking;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ConnectionListenerTest
    {
        [TestCase(12345)]
        public async Task OnConnectionAcceptingTest(int port)
        {
            using (var connectionListener = new ConnectionListener(port))
            {
                using (var client = new TcpClient())
                {
                    var isAccepting = false;
                    connectionListener.Start();
                    connectionListener.OnConnectionAccepting += (s, e) =>
                    {
                        isAccepting = true;
                    };

                    await client.ConnectAsync("localhost", port);
                    await Task.Delay(100);
                    Assert.IsTrue(connectionListener.IsListening);
                    Assert.IsTrue(client.Connected);
                    Assert.IsTrue(isAccepting);
                }
            }
        }

        [TestCase(12346)]
        public async Task OnConnectionAcceptedTest(int port)
        {
            using (var connectionListener = new ConnectionListener(port))
            {
                using (var client = new TcpClient())
                {
                    var isAccepted = false;
                    connectionListener.OnConnectionAccepted += (s, e) =>
                    {
                        isAccepted = true;
                    };
                    connectionListener.Start();
                    await client.ConnectAsync("localhost", port);
                    await Task.Delay(100);
                    Assert.IsTrue(connectionListener.IsListening, "Connection Listerner is listening");
                    Assert.IsTrue(client.Connected, "Client is connected");
                    Assert.IsTrue(isAccepted, "The flag was set");
                }
            }
        }

        [TestCase(12347)]
        public async Task OnConnectionFailureTest(int port)
        {
            Assert.Inconclusive("How to throw a failure?");

            using (var connectionListener = new ConnectionListener(port))
            {
                using (var client = new TcpClient())
                {
                    var isFailure = false;
                    connectionListener.OnConnectionFailure += (s, e) =>
                    {
                        isFailure = true;
                    };

                    connectionListener.Start();
                    await client.ConnectAsync("localhost", port);
                    Assert.IsTrue(isFailure);
                }
            }
        }

        [TestCase(12348)]
        public void OnListenerStoppedTest(int port)
        {
            using (var connectionListener = new ConnectionListener(port))
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