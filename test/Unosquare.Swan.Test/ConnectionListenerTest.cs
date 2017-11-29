namespace Unosquare.Swan.Test
{
    using System;
    using NUnit.Framework;
    using System.Net.Sockets;
    using System.Threading.Tasks;
    using Networking;
    using System.Text;

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

                    client.Close();
                }

                connectionListener.Stop();
            }
        }

        [Test]
        public void UsingLoopback_CanListen()
        {
            const int port = 12346;

            using (var connectionListener = new ConnectionListener(System.Net.IPAddress.Parse("127.0.0.1"), port))
            {
                connectionListener.Start();
                Assert.IsTrue(connectionListener.IsListening);

                connectionListener.Stop();
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
                    client.Close();
                }

                connectionListener.Stop();
            }
        }

        [Test]
        public async Task OnConnectionFailureTest()
        {
            Assert.Ignore("Fix");

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
                connectionListener.Stop();
            }
        }

        [TestCase(13245)]
        public async Task ConnectionOpenTest(int port)
        {
            if (Environment.GetEnvironmentVariable("APPVEYOR") == "True")
                Assert.Inconclusive("Can not test in AppVeyor");

            using (var connectionListener = new ConnectionListener(port))
            {
                using (var client = new TcpClient())
                {
                    connectionListener.Start();

                    await client.ConnectAsync("localhost", port);
                    await Task.Delay(400);

                    using (var connection = new Connection(client))
                    {
                        Assert.IsTrue(connectionListener.IsListening);
                        Assert.IsTrue(connection.IsConnected);
                    }
                }
            }
        }

        [TestCase(13246)]
        public async Task ConnectionWriteTest(int port)
        {
            if (Environment.GetEnvironmentVariable("APPVEYOR") == "True")
                Assert.Inconclusive("Can not test in AppVeyor");

            var message = Encoding.ASCII.GetBytes("HOLA");

            using (var connectionListener = new ConnectionListener(port))
            {
                using (var client = new TcpClient())
                {
                    connectionListener.Start();
                    connectionListener.OnConnectionAccepting += (s, e) =>
                    {
                        e.Client?.GetStream().Write(message, 0, message.Length);
                    };

                    await client.ConnectAsync("localhost", port);
                    await Task.Delay(500);

                    using (var connection = new Connection(client, Encoding.ASCII, "\r\n", true, 0))
                    {
                        var response = await connection.ReadTextAsync();

                        Assert.IsNotNull(response);
                        Assert.AreEqual("HOLA", response);
                    }
                }
            }
        }
    }
}