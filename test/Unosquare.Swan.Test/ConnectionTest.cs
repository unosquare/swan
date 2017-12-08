namespace Unosquare.Swan.Test
{
    using System;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Networking;
    using System.Net;
    using Mocks;
    using System.IO;

    public abstract class ConnectionTest
    {
        public const int DefaultPort = 1337;
        public const string Message = "Hello World!\r\n";
        public const string Localhost = "localhost";

        public ConnectionListener ConnectionListener;
        public TcpClient Client;
        public int Port;
        public byte[] MessageBytes = Encoding.UTF8.GetBytes(Message);
        private int _defaultPort = 12445;

        [SetUp]
        public void Setup()
        {
            Port = _defaultPort++;
            ConnectionListener = new ConnectionListener(Port);
            Client = new TcpClient();
        }

        [TearDown]
        public void GlobalTeardown()
        {
            ConnectionListener.Dispose();
            Client.Dispose();
        }
    }

    [TestFixture]
    public class Connections : ConnectionTest
    {
        [Test]
        public void OpenConnection_Connected()
        {
            Client.Connect(Localhost, DefaultPort);

            using (var cn = new Connection(Client))
            {
                Assert.IsTrue(cn.IsConnected, "It's connected");
                Assert.AreEqual(IPAddress.Parse("127.0.0.1"), cn.LocalEndPoint.Address, "Local Address");
                Assert.IsNotNull(cn.ConnectionStartTime, "Connection Start Time");
                Assert.IsNotNull(cn.ConnectionDuration, "Connection Duration");
            }
        }

        [Test]
        public void NullNewLineSequence_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var cn = new Connection(Client, Encoding.UTF8, null, true, 0);
            });
        }
    }

    [TestFixture]
    public class ReadTextAsync : ConnectionTest
    {
        [Test]
        public async Task ReadTextAsync_MessageEqualsResponse()
        {
            await Client.ConnectAsync(Localhost, DefaultPort);

            using (var cn = new Connection(Client, Encoding.UTF8, "\r\n", true, 0))
            {
                var response = await cn.ReadTextAsync();

                Assert.AreEqual(Message, response);
            }
        }

        [Test]
        public async Task ReadTextAsync_DataSentIdleDuration()
        {
            await Client.ConnectAsync(Localhost, DefaultPort);

            using (var cn = new Connection(Client, Encoding.UTF8, "\r\n", true, 0))
            {
                await cn.ReadTextAsync();

                Assert.NotNull(cn.DataSentIdleDuration);
            }
        }

        [Test]
        public async Task ReadTextAsync_DataReceivedIdleDuration()
        {
            await Client.ConnectAsync(Localhost, DefaultPort);

            using (var cn = new Connection(Client, Encoding.UTF8, "\r\n", true, 0))
            {
                await cn.ReadTextAsync();

                Assert.IsNotNull(cn.DataReceivedIdleDuration);
            }
        }
    }

    [TestFixture]
    public class ReadLineAsync : ConnectionTest
    {
        [Test]
        public async Task ReadLineAsync_MessageEqualsResponse()
        {
            await Client.ConnectAsync(Localhost, DefaultPort);

            using (var cn = new Connection(Client, Encoding.UTF8, "\r\n", true, 0))
            {
                var response = await cn.ReadLineAsync();

                Assert.AreEqual(Message.Trim(), response);
            }
        }

        [Test]
        public async Task EnableContinousReading_ThrowsInvalidOperationException()
        {
            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                using (var cn = new Connection(e.Client))
                {
                    cn.WriteDataAsync(MessageBytes, true).Wait();
                }
            };

            ConnectionListener.Start();
            await Client.ConnectAsync(Localhost, Port);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                using (var cn = new Connection(Client))
                {
                    Assert.IsTrue(cn.IsContinuousReadingEnabled);
                    await cn.ReadLineAsync();
                }
            });
        }
    }

    [TestFixture]
    public class ReadDataAsync : ConnectionTest
    {
        [Test]
        public async Task ValidConnection_MessageEqualsResponse()
        {
            await Client.ConnectAsync(Localhost, DefaultPort);

            using (var cn = new Connection(Client, Encoding.UTF8, "\r\n", true, 0))
            {
                var response = await cn.ReadDataAsync();

                Assert.IsNotNull(response);
                Assert.AreEqual(MessageBytes, response);
            }
        }

        [Test]
        public async Task ContinuousReadingEnabled_ThrowsInvalidOperationException()
        {
            await Client.ConnectAsync(Localhost, DefaultPort);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                using (var cn = new Connection(Client))
                {
                    await cn.ReadDataAsync();
                }
            });
        }

        [Test]
        public async Task SmallTimeOut_ThrowsTimeOutException()
        {
            await Client.ConnectAsync(Localhost, DefaultPort);

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                using (var cn = new Connection(Client, Encoding.UTF8, "\r\n", true, 0))
                {
                    await cn.ReadDataAsync(TimeSpan.FromMilliseconds(1));
                }
            });
        }
    }

    [TestFixture]
    public class WriteDataAsync : ConnectionTest
    {
        [Test]
        public async Task WriteDataAsync_MessageEqualsResponse()
        {
            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                using (var cn = new Connection(e.Client))
                {
                    cn.WriteDataAsync(MessageBytes, false).Wait();
                }
            };

            ConnectionListener.Start();
            await Client.ConnectAsync(Localhost, Port);

            using (var cn = new Connection(Client, Encoding.UTF8, "\r\n", true, 0))
            {
                var response = await cn.ReadDataAsync();

                Assert.IsNotNull(response);
                Assert.AreEqual(MessageBytes, response);
            }
        }

        [Test]
        public async Task WriteDataAsyncFlushEnabled_MessageEqualsResponse()
        {
            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                using (var cn = new Connection(e.Client))
                {
                    cn.WriteDataAsync(MessageBytes, true).Wait();
                }
            };

            ConnectionListener.Start();
            await Client.ConnectAsync(Localhost, Port);

            using (var cn = new Connection(Client, Encoding.UTF8, "\r\n", true, 0))
            {
                var response = await cn.ReadDataAsync();

                Assert.IsNotNull(response);
                Assert.AreEqual(MessageBytes, response);
            }
        }
    }

    [TestFixture]
    public class WriteTextAsync : ConnectionTest
    {
        [Test]
        public async Task WriteTextAsync_MessageEqualResponse()
        {
            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                using (var cn = new Connection(e.Client))
                {
                    cn.WriteTextAsync(Message).Wait();
                }
            };

            ConnectionListener.Start();
            await Client.ConnectAsync(Localhost, Port);

            using (var cn = new Connection(Client, Encoding.UTF8, "\r\n", true, 0))
            {
                var response = await cn.ReadDataAsync();

                Assert.IsNotNull(response);
                Assert.AreEqual(MessageBytes, response);
            }
        }
    }

    [TestFixture]
    public class WriteLineAsync : ConnectionTest
    {
        [Test]
        public async Task WriteLineAsync_MessageEqualResponse()
        {
            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                using (var cn = new Connection(e.Client))
                {
                    cn.WriteLineAsync(Message).Wait();
                }
            };

            ConnectionListener.Start();
            await Client.ConnectAsync(Localhost, Port);

            using (var cn = new Connection(Client, Encoding.UTF8, "\r\n", true, 0))
            {
                var response = await cn.ReadLineAsync();

                Assert.IsNotNull(response);
                Assert.AreEqual(Message.Remove(MessageBytes.Length - 2), response);
            }
        }
    }

#if NET46
    [TestFixture]
    public class UpgradeToSecureAsServerAsync : ConnectionTest
    {
        [Test]
        public async Task UpgradeToSecureAsServerAndClientAsync_ReturnTrue()
        {
            Assert.Ignore();

            var tempPath = Path.GetTempPath() + "certificate.pfx";
            var certificate = CertificateHelper.CreateOrLoadCertificate(tempPath, Localhost, "password");

            ConnectionListener.Start();
            await Client.ConnectAsync(Localhost, Port);

            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                using (var cn = new Connection(e.Client))
                {
                    cn.UpgradeToSecureAsServerAsync(certificate).Wait();
                }
            };

            using (var cn = new Connection(Client))
            {
                var result = await cn.UpgradeToSecureAsClientAsync();

                Assert.IsTrue(result);
                Assert.IsTrue(cn.IsActiveStreamSecure);
            }
        }
    }
#endif
}