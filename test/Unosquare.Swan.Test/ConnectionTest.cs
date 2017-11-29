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
        public async Task Connection_OpenTest()
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
        public async Task Connection_LocalAddress()
        {
            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client, Encoding.UTF8, "\r\n", true, 0))
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
        public async Task Read_LineAsync()
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
        public async Task Read_LineAsync_ThrowsInvalidOperationException()
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

        [Test]
        public async Task Read_DataAsync_ThrowsInvalidOperationException()
        {
            ConnectionListener.Start();
            await Client.ConnectAsync("localhost", Port);

            using (var cn = new Connection(Client, Encoding.UTF8, "\r\n", false, 0))
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

        [Test]
        public async Task Connection_WriteDataAsync()
        {
            // TODO: Write Data
        }
    }
}