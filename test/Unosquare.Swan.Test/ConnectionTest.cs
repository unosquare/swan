namespace Unosquare.Swan.Test
{
    using System;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Networking;
    using System.Threading;
    using System.Net;
    using Mocks;
    using System.IO;

    public abstract class ConnectionTest
    {
        public const int HttpPort = 3000;
        public const string Message = "Hello World!\r\n";
        public ConnectionListener ConnectionListener;
        public TcpClient Client;
        public int Port;
        public CancellationToken ct;
        public byte[] MessageBytes = Encoding.UTF8.GetBytes(Message);
        private int _defaultPort = 12445;

        [SetUp]
        public void Setup()
        {
            Port = _defaultPort++;
            ConnectionListener = new ConnectionListener(Port);
            Client = new TcpClient();
            ct = default(CancellationToken);
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
            Client.Connect("localhost", HttpPort);

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
            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                e.Client?.GetStream().Write(MessageBytes, 0, MessageBytes.Length);
            };

            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client))
            {
                var response = await cn.ReadTextAsync();

                Assert.IsNotNull(response);
                Assert.AreEqual(MessageBytes, response);
            }
        }

        [Test]
        public async Task ReadTextAsync_DataSentIdleDuration()
        {
            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                e.Client?.GetStream().Write(MessageBytes, 0, MessageBytes.Length);
            };

            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client))
            {
                var response = await cn.ReadTextAsync();

                Assert.AreEqual(MessageBytes, response);
                Assert.NotNull(cn.DataSentIdleDuration);
            }
        }

        [Test]
        public async Task ReadTextAsync_DataReceivedIdleDuration()
        {
            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                e.Client?.GetStream().Write(MessageBytes, 0, MessageBytes.Length);
            };

            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client))
            {
                var response = await cn.ReadTextAsync();

                Assert.AreEqual(MessageBytes, response);
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
            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                e.Client?.GetStream().Write(MessageBytes, 0, MessageBytes.Length);
            };

            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client))
            {
                var response = await cn.ReadLineAsync(ct);

                Assert.IsNotNull(response);
                Assert.AreEqual(Message.Remove(MessageBytes.Length - 2), response);
            }
        }

        [Test]
        public async Task StreamWithoutWrite_ThrowsInvalidOperationException()
        {
            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                using (var cn = new Connection(Client, Encoding.ASCII, "\r\n", false, 0))
                {
                    await cn.ReadLineAsync(ct);
                }
            });
        }
    }

    [TestFixture]
    public class ReadDataAsync : ConnectionTest
    {
        [Test]
        public async Task ReadDataAsync_MessageEqualsResponse()
        {
            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                e.Client?.GetStream().Write(MessageBytes, 0, MessageBytes.Length);
            };

            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client))
            {
                var response = await cn.ReadDataAsync(ct);

                Assert.IsNotNull(response);
                Assert.AreEqual(MessageBytes, response);
            }
        }

        [Test]
        public async Task ContinuousReadingEnabled_ThrowsInvalidOperationException()
        {
            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                e.Client?.GetStream().Write(MessageBytes, 0, MessageBytes.Length);
            };

            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                using (var cn = new Connection(Client))
                {
                    await cn.ReadDataAsync(TimeSpan.FromSeconds(5), ct);
                }
            });
        }

        [Test]
        public async Task ReadDataAsync_ThrowsTimeOutException()
        {
            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            Assert.ThrowsAsync<TimeoutException>(async () =>
            {
                using (var cn = new Connection(Client, Encoding.UTF8, "\r\n", true, 0))
                {
                    await cn.ReadDataAsync(TimeSpan.FromMilliseconds(100), ct);
                }
            });
        }
    }

    [TestFixture]
    public class Write : ConnectionTest
    {
        [Test]
        public async Task Connection_WriteTest()
        {
            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                e.Client?.GetStream().Write(MessageBytes, 0, MessageBytes.Length);
            };

            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client))
            {
                var response = await cn.ReadTextAsync();

                Assert.IsNotNull(response);
                Assert.AreEqual(MessageBytes, response);
            }
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
                using (var cn = new Connection(e.Client, Encoding.ASCII, "\r\n", false, 0))
                {
                    cn.WriteDataAsync(MessageBytes, false, ct).Wait();
                }
            };
            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client))
            {
                var response = await cn.ReadDataAsync(ct);

                Assert.IsNotNull(response);
                Assert.AreEqual(MessageBytes, response);
            }
        }

        [Test]
        public async Task WriteDataAsyncFlushEnabled_MessageEqualsResponse()
        {
            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                using (var cn = new Connection(e.Client, Encoding.ASCII, "\r\n", false, 0))
                {
                    cn.WriteDataAsync(MessageBytes, true, ct).Wait();
                }
            };
            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client))
            {
                var response = await cn.ReadDataAsync(ct);

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
                    cn.WriteTextAsync(Message, ct).Wait();
                }
            };
            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client))
            {
                var response = await cn.ReadDataAsync(ct);

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
                    cn.WriteLineAsync(Message, ct).Wait();
                }
            };
            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client))
            {
                var response = await cn.ReadLineAsync(ct);

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
            var certificate = CertificateHelper.CreateOrLoadCertificate(tempPath, "localhost", "password");     

            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

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