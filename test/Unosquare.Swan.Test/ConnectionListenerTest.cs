namespace Unosquare.Swan.Test
{
    using System;
    using NUnit.Framework;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Networking;

    [TestFixture]
    public class ConnectionListenerTest
    {
        [Test]
        public async Task OnConnectionAcceptingTest()
        {
            const int port = 12345;

            using (var connectionListener = new ConnectionListener(port))
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

                    await client.ConnectAsync("localhost", port);
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
            const int port = 12346;

            using (var connectionListener = new ConnectionListener(System.Net.IPAddress.Parse("127.0.0.1"), port))
            {
                using (var client = new TcpClient())
                {
                    connectionListener.Start();
                    await client.ConnectAsync("localhost", port);
                    await Task.Delay(100);
                    Assert.IsTrue(connectionListener.IsListening);
                    Assert.IsTrue(client.Connected);
                }
            }
        }

        [Test]
        public async Task OnConnectionAcceptedTest()
        {
            const int port = 12347;

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

        [Test]
        public async Task OnConnectionFailureTest()
        {
            Assert.Inconclusive("Fix");

            const int port = 12348;
            
            using (var connectionListener = new ConnectionListener(port))
            {
                using (var client = new TcpClient())
                {
                    var isFailure = false;
                    connectionListener.OnConnectionAccepting += (s, e) =>
                    {
                        e.Cancel = true;
                    };
                    connectionListener.OnConnectionFailure += (s, e) =>
                    {
                        isFailure = true;
                    };

                    connectionListener.Start();
                    await client.ConnectAsync("localhost", port);
                    connectionListener.Stop();
                    
                    Assert.IsTrue(isFailure);
                }
            }
        }

        [Test]
        public void OnListenerStoppedTest()
        {
            const int port = 12349;

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