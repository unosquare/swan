﻿namespace Unosquare.Swan.Test
{
    using System;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Networking;
    using System.Threading;
    using System.Net;
    using Unosquare.Swan.Test.Mocks;
    using System.IO;

    public abstract class ConnectionTest
    {
        public ConnectionListener ConnectionListener;
        public TcpClient Client;
        public int Port;
        public CancellationToken ct;
        public byte[] Message = Encoding.ASCII.GetBytes("Hello World!\r\n");
        private int _defaultPort = 12345;

        [SetUp]
        public void Setup()
        {
            _defaultPort++;
            Port = _defaultPort;
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
    public class ConnectionsTests : ConnectionTest
    {
        [Test]
        public async Task OpenConnection_Connected()
        {
            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client, Encoding.UTF8, "\r\n", true, 0))
            {
                Assert.IsTrue(ConnectionListener.IsListening);
                Assert.IsTrue(cn.IsConnected);
            }
        }

        [Test]
        public async Task OpenConnection_LocalAddress()
        {
            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client, Encoding.UTF8, "\r\n", true, 0))
            {
                Assert.AreEqual(IPAddress.Parse("127.0.0.1"), cn.LocalEndPoint.Address, "Local Address");
            }
        }

        [Test]
        public async Task OpenConnection_ConnectionStartTime()
        {
            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);
            var dateNow = DateTime.UtcNow.Ticks;

            using (var cn = new Connection(Client, Encoding.UTF8, "\r\n", true, 0))
            {
                Assert.GreaterOrEqual(cn.ConnectionStartTime.Ticks, dateNow);
            }
        }

        [Test]
        public async Task OpenConnection_ConnectionDuration()
        {
            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);
            var dateNow = DateTime.UtcNow.Ticks;

            using (var cn = new Connection(Client, Encoding.UTF8, "\r\n", true, 0))
            {
                Assert.GreaterOrEqual(cn.ConnectionDuration, dateNow);
            }
        }

        [Test]
        public async Task OpenConnection_ArgumentException()
        {
            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            Assert.Throws<ArgumentException>(() =>
            {
                var cn = new Connection(Client, Encoding.UTF8, null, true, 0);
            });
        }
    }

    [TestFixture]
    public class ReadTextAsyncTest : ConnectionTest
    {
        [Test]
        public async Task ReadTextAsync_MessageEqualsResponse()
        {
            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                e.Client?.GetStream().Write(Message, 0, Message.Length);
            };

            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client, Encoding.ASCII, "\r\n", true, 0))
            {
                var response = await cn.ReadTextAsync();

                Assert.IsNotNull(response);
                Assert.AreEqual(Message, response);
            }
        }

        [Test]
        public async Task ReadTextAsync_DataSentIdleDuration()
        {
            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                e.Client?.GetStream().Write(Message, 0, Message.Length);
            };

            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client, Encoding.ASCII, "\r\n", true, 0))
            {
                var response = await cn.ReadTextAsync();
                var duration = TimeSpan.FromSeconds(5);

                Assert.AreEqual(Message, response);
                Assert.NotNull(cn.DataSentIdleDuration);
            }
        }

        [Test]
        public async Task ReadTextAsync_DataReceivedIdleDuration()
        {
            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                e.Client?.GetStream().Write(Message, 0, Message.Length);
            };

            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client, Encoding.ASCII, "\r\n", true, 0))
            {
                var response = await cn.ReadTextAsync();
                var duration = TimeSpan.FromSeconds(5);

                Assert.AreEqual(Message, response);
                Assert.IsNotNull(cn.DataReceivedIdleDuration);
            }
        }
    }

    [TestFixture]
    public class ReadLineAsyncTest : ConnectionTest
    {
        [Test]
        public async Task ReadLineAsync_MessageEqualsResponse()
        {
            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                e.Client?.GetStream().Write(Message, 0, Message.Length);
            };

            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client, Encoding.ASCII, "\r\n", true, 0))
            {
                var response = await cn.ReadLineAsync(ct);

                Assert.IsNotNull(response);
                Assert.AreEqual(Encoding.ASCII.GetString(Message).Remove(Message.Length - 2), response);
            }
        }

        [Test]
        public async Task ReadLineAsync_ThrowsInvalidOperationException()
        {
            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client, Encoding.ASCII, "\r\n", false, 0))
            {
                Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    await cn.ReadLineAsync(ct);
                });
            }
        }
    }

    [TestFixture]
    public class ReadDataAsyncTest : ConnectionTest
    {
        [Test]
        public async Task ReadDataAsync_MessageEqualsResponse()
        {
            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                e.Client?.GetStream().Write(Message, 0, Message.Length);
            };

            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client, Encoding.ASCII, "\r\n", true, 0))
            {
                var response = await cn.ReadDataAsync(ct);

                Assert.IsNotNull(response);
                Assert.AreEqual(Message, response);
            }
        }

        [Test]
        public async Task ReadDataAsync_ThrowsInvalidOperationException()
        {
            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client, Encoding.UTF8, "\r\n", false, 0))
            {
                Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    await cn.ReadDataAsync(TimeSpan.FromMilliseconds(100),ct);
                });
            }
        }

        [Test]
        public async Task ReadDataAsync_ThrowsTimeOutException()
        {
            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client,Encoding.UTF8, "\r\n", true, 0))
            {
                Assert.ThrowsAsync<TimeoutException>(async () => 
                {
                    await cn.ReadDataAsync(TimeSpan.FromMilliseconds(100), ct);
                });
            }
        }
    }

    [TestFixture]
    public class WriteTest : ConnectionTest
    {
        [Test]
        public async Task Connection_WriteTest()
        {
            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                e.Client?.GetStream().Write(Message, 0, Message.Length);
            };

            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client, Encoding.ASCII, "\r\n", true, 0))
            {
                var response = await cn.ReadTextAsync();

                Assert.IsNotNull(response);
                Assert.AreEqual(Message, response);
            }
        }
    }

    [TestFixture]
    public class WriteDataAsyncTest : ConnectionTest
    {
        [Test]
        public async Task WriteDataAsync_MessageEqualsResponse()
        {
            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var connection = new Connection(Client, Encoding.ASCII, "\r\n", true, 0))
            {
                var otherMessage = Encoding.ASCII.GetBytes("Other Message!\r\n");
                await connection.WriteLineAsync(Encoding.ASCII.GetString(otherMessage), ct);

                var response = await connection.ReadDataAsync(TimeSpan.FromSeconds(5),ct);

                Assert.IsNotNull(response);
            }
        }
    }

    [TestFixture]
    public class UpgradeToSecureAsServerAsync : ConnectionTest
    {
        [Test]
        public async Task UpgradeToSecureAsServerAsync_true()
        {
            ConnectionListener.OnConnectionAccepting += (s, e) =>
            {
                e.Client?.GetStream().Write(Message, 0, Message.Length);
            };

            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            var tempPath = Path.GetTempPath() + "certificate.pfx";
            var certificate = CertificateHelper.CreateOrLoadCertificate(tempPath, "localhost", "password");

            using (var cn = new Connection(Client, Encoding.ASCII, "\r\n", false, 0))
            {
                var result = await cn.UpgradeToSecureAsServerAsync(certificate);

                Assert.IsTrue(result);
            }
        }
    }
}