using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Unosquare.Swan.Networking;

namespace Unosquare.Swan.Test
{
    [TestFixture]
    public class ConnectionTest
    {
        [TestCase(13245)]
        public async Task ConnectionOpenTest(int port)
        {
            try
            {
                using (var connectionListener = new ConnectionListener(port))
                {
                    using (var client = new TcpClient())
                    {
                        connectionListener.Start();

                        await client.ConnectAsync("localhost", port);
                        await Task.Delay(200);

                        var connection = new Connection(client);

                        Assert.IsTrue(connectionListener.IsListening);
                        Assert.IsTrue(connection.IsConnected);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // Ignore
            }
        }

        [TestCase(13246)]
        public async Task ConnectionWriteTest(int port)
        {
            var message = Encoding.ASCII.GetBytes("HOLA");

            try
            {
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
                        await Task.Delay(200);

                        var connection = new Connection(client, Encoding.ASCII, "\r\n", true, 0);
                        var response = await connection.ReadTextAsync();

                        Assert.IsNotNull(response);
                        Assert.AreEqual("HOLA", response);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // Ignore
            }
        }
    }
}