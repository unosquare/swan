namespace Unosquare.Swan.Test
{
    using System;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Networking;

    [TestFixture]
    public class ConnectionTest
    {
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