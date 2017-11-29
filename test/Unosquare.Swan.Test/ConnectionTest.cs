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

        [SetUp]
        public async Task Setup()
        {
            connectionListener = new ConnectionListener(port);
            client = new TcpClient();
                
            connectionListener.Start();
            await client.ConnectAsync("localhost", port);
        }

        [TearDown]
        public void GlobalTeardown()
        {
            connectionListener.Stop();
            client.Close();
        }
    }

    [TestFixture]
    public class ConnectionsTests : ConnectionTest
    {
        [Test]
        public void Connection_Test()
        {
            using (var cn = new Connection(client))
            {
                Assert.IsTrue(cn.IsConnected);
            }
        }

        [Test]
        public void Connection_LocalAddress()
        {
            using (var cn = new Connection(client))
            {
                Assert.AreEqual(IPAddress.Parse("127.0.0.1"), cn.LocalEndPoint.Address, "Local Address");
            }
        }
    }

    [TestFixture]
    public class ReadTest : ConnectionTest
    {
        private CancellationToken ct = new CancellationToken();

        [Test]
        public async Task Read_DataAsync()
        {
            var byteArray = Enumerable.Repeat<byte>(0x20, 100).ToArray();

            using (var cn = new Connection(client))
            {
                // TODO: Read tests
            }
        }
    }
}