namespace Unosquare.Swan.Test
{
    using System;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Networking;
    using System.Linq;
    using System.Threading;
    using System.Net;

    public abstract class ConnectionTest
    {
        public ConnectionListener connectionListener;
        public TcpClient client;
        public readonly int port = 12345;
        public CancellationToken ct;
        public byte[] message = Encoding.ASCII.GetBytes("Hello World!\r\n");

        [SetUp]
        public void Setup()
        {
            connectionListener = new ConnectionListener(port);
            client = new TcpClient();
            ct = default(CancellationToken);           
        }

        [TearDown]
        public void GlobalTeardown()
        {
            connectionListener.Dispose();
            client.Dispose();
        }
    }

    [TestFixture]
    public class ConnectionsTests : ConnectionTest
    {
        [Test]
        public async Task Connection_OpenTest()
        {
            connectionListener.Start();
            await client.ConnectAsync("localhost", port);

            using (var cn = new Connection(client, Encoding.UTF8, "\r\n", true, 0))
            {
                Assert.IsTrue(connectionListener.IsListening);
                Assert.IsTrue(cn.IsConnected);
            }
        }

        [Test]
        public async Task Connection_LocalAddress()
        {
            connectionListener.Start();
            await client.ConnectAsync("localhost", port);

            using (var cn = new Connection(client, Encoding.UTF8, "\r\n", true, 0))
            {
                Assert.AreEqual(IPAddress.Parse("127.0.0.1"), cn.LocalEndPoint.Address, "Local Address");
            }
        }
    }

    [TestFixture]
    public class ReadTest : ConnectionTest
    {
        [Test]
        public async Task Read_TextAsync()
        {
            connectionListener.OnConnectionAccepting += (s, e) =>
            {
                e.Client?.GetStream().Write(message, 0, message.Length);
            };

            connectionListener.Start();
            await client.ConnectAsync("localhost", port);

            using (var cn = new Connection(client, Encoding.ASCII, "\r\n", true, 0))
            {
                var response = await cn.ReadTextAsync();

                Assert.IsNotNull(response);
                Assert.AreEqual(message, response);
            }
        }

        [Test]
        public async Task Read_LineAsync()
        {
            connectionListener.OnConnectionAccepting += (s, e) =>
            {
                e.Client?.GetStream().Write(message, 0, message.Length);
            };

            connectionListener.Start();
            await client.ConnectAsync("localhost", port);

            using (var cn = new Connection(client, Encoding.ASCII, "\r\n", true, 0))
            {
                var response = await cn.ReadLineAsync(ct);

                Assert.IsNotNull(response);
                Assert.AreEqual(Encoding.ASCII.GetString(message).Remove(message.Length - 2), response);
            }
        }

        [Test]
        public async Task Read_LineAsync_ThrowsInvalidOperationException()
        {
            connectionListener.Start();
            await client.ConnectAsync("localhost", port);

            using (var cn = new Connection(client, Encoding.ASCII, "\r\n", false, 0))
            {
                Assert.ThrowsAsync<InvalidOperationException>(async () => 
                {
                    await cn.ReadLineAsync(ct);
                });
            }
        }

        [Test]
        public async Task Read_DataAsync_ThrowsInvalidOperationException()
        {
            connectionListener.Start();
            await client.ConnectAsync("localhost", port);

            using (var cn = new Connection(client, Encoding.UTF8, "\r\n", false, 0))
            {
                Assert.ThrowsAsync<InvalidOperationException>(async () =>
                {
                    await cn.ReadDataAsync(ct);
                });
            }
        }

        [Test]
        public async Task Read_DataAsync_ThrowsTimeOutException()
        {
            connectionListener.Start();
            await client.ConnectAsync("localhost", port);

            using (var cn = new Connection(client,Encoding.UTF8, "\r\n", true, 0))
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
            connectionListener.OnConnectionAccepting += (s, e) =>
            {
                e.Client?.GetStream().Write(message, 0, message.Length);
            };

            connectionListener.Start();
            await client.ConnectAsync("localhost", port);

            using (var cn = new Connection(client, Encoding.ASCII, "\r\n", true, 0))
            {
                var response = await cn.ReadTextAsync();

                Assert.IsNotNull(response);
                Assert.AreEqual(message, response);
            }
        }

        [Test]
        public async Task Connection_WriteDataAsync()
        {
            await client.ConnectAsync("localhost", 80);

            using (var cn = new Connection(client, Encoding.ASCII, "\r\n", false, 0))
            {
                await cn.WriteDataAsync(message, false, ct);
            }
        }
    }
}