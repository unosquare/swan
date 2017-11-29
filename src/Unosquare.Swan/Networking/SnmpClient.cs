namespace Unosquare.Swan.Networking
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a little SNMP client based on http://www.java2s.com/Code/CSharp/Network/SimpleSNMP.htm
    /// </summary>
    public class SnmpClient
    {
        private static readonly byte[] DiscoverMessage =
        {
            48, 41, 2, 1, 1, 4, 6, 112, 117, 98, 108, 105, 99, 160, 28, 2, 4, 111, 81, 45, 144, 2, 1, 0, 2, 1, 0, 48,
            14, 48, 12, 6, 8, 43, 6, 1, 2, 1, 1, 1, 0, 5, 0
        };

        /// <summary>
        /// Discovers the specified SNMP time out.
        /// </summary>
        /// <param name="snmpTimeOut">The SNMP time out.</param>
        /// <returns>An array of network endpoint as an IP address and a port number</returns>
        public static IPEndPoint[] Discover(int snmpTimeOut = 6000)
        {
            var endpoints = new List<IPEndPoint>();

            Task[] tasks =
            {
                Task.Factory.StartNew(async () =>
                {
                    using (var udp = new UdpClient(IPAddress.Broadcast.AddressFamily))
                    {
                        udp.EnableBroadcast = true;
                        await udp.SendAsync(
                            DiscoverMessage,
                            DiscoverMessage.Length,
                            new IPEndPoint(IPAddress.Broadcast, 161));

                        while (true)
                        {
                            try
                            {
                                var buffer = new byte[udp.Client.ReceiveBufferSize];
                                EndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                                udp.Client.ReceiveFrom(buffer, ref remote);
                                endpoints.Add(remote as IPEndPoint);
                            }
                            catch
                            {
                                break;
                            }
                        }
#if NET452
                        udp.Close();
#endif
                    }
                }),
                Task.Delay(snmpTimeOut)
            };

            Task.WaitAny(tasks);

            return endpoints.ToArray();
        }

        /// <summary>
        /// Gets the name of the public.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <returns>
        /// A string that contains the results of decoding the specified sequence 
        /// of bytes ref=GetString"
        /// </returns>
        public static string GetPublicName(IPEndPoint host) => GetString(host, "1.3.6.1.2.1.1.5.0");

        /// <summary>
        /// Gets the uptime.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="mibstring">The mibstring.</param>
        /// <returns>
        ///  A time interval that represents a specified number of seconds, 
        ///  where the specification is accurate to the nearest millisecond
        ///  </returns>
        public static TimeSpan GetUptime(IPEndPoint host, string mibstring = "1.3.6.1.2.1.1.3.0")
        {
            var response = Get(host, mibstring);
            if (response[0] == 0xff) return TimeSpan.Zero;

            // If response, get the community name and MIB lengths
            var commlength = Convert.ToInt16(response[6]);
            var miblength = Convert.ToInt16(response[23 + commlength]);

            // Extract the MIB data from the SNMP response
            var datalength = Convert.ToInt16(response[25 + commlength + miblength]);
            var datastart = 26 + commlength + miblength;

            var uptime = 0;

            while (datalength > 0)
            {
                uptime = (uptime << 8) + response[datastart++];
                datalength--;
            }

            return TimeSpan.FromSeconds(uptime);
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="mibstring">The mibstring.</param>
        /// <returns>A <see cref="System.String" /> that contains the results of decoding the specified sequence of bytes</returns>
        public static string GetString(IPEndPoint host, string mibstring)
        {
            var response = Get(host, mibstring);
            if (response[0] == 0xff) return string.Empty;

            // If response, get the community name and MIB lengths
            var commlength = Convert.ToInt16(response[6]);
            var miblength = Convert.ToInt16(response[23 + commlength]);

            // Extract the MIB data from the SNMP response
            var datalength = Convert.ToInt16(response[25 + commlength + miblength]);
            var datastart = 26 + commlength + miblength;

            return Encoding.ASCII.GetString(response, datastart, datalength);
        }

        /// <summary>
        /// Gets the specified host.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="mibstring">The mibstring.</param>
        /// <returns>A byte array containing the results of encoding the specified set of characters</returns>
        public static byte[] Get(IPEndPoint host, string mibstring)
        {
            return Get("get", host, "public", mibstring);
        }

        /// <summary>
        /// Gets the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="host">The host.</param>
        /// <param name="community">The community.</param>
        /// <param name="mibstring">The mibstring.</param>
        /// <returns>A byte array containing the results of encoding the specified set of characters</returns>
        public static byte[] Get(string request, IPEndPoint host, string community, string mibstring)
        {
            var packet = new byte[1024];
            var mib = new byte[1024];
            var comlen = community.Length;
            var mibvals = mibstring.Split('.');
            var miblen = mibvals.Length;
            var cnt = 0;
            var orgmiblen = miblen;
            var pos = 0;

            // Convert the string MIB into a byte array of integer values
            // Unfortunately, values over 128 require multiple bytes
            // which also increases the MIB length
            for (var i = 0; i < orgmiblen; i++)
            {
                int temp = Convert.ToInt16(mibvals[i]);
                if (temp > 127)
                {
                    mib[cnt] = Convert.ToByte(128 + (temp / 128));
                    mib[cnt + 1] = Convert.ToByte(temp - ((temp / 128) * 128));
                    cnt += 2;
                    miblen++;
                }
                else
                {
                    mib[cnt] = Convert.ToByte(temp);
                    cnt++;
                }
            }

            var snmplen = 29 + comlen + miblen - 1;

            // The SNMP sequence start
            packet[pos++] = 0x30; // Sequence start
            packet[pos++] = Convert.ToByte(snmplen - 2); // sequence size

            // SNMP version
            packet[pos++] = 0x02; // Integer type
            packet[pos++] = 0x01; // length
            packet[pos++] = 0x00; // SNMP version 1

            // Community name
            packet[pos++] = 0x04; // String type
            packet[pos++] = Convert.ToByte(comlen); // length
            // Convert community name to byte array
            var data = Encoding.ASCII.GetBytes(community);

            foreach (var t in data)
            {
                packet[pos++] = t;
            }

            // Add GetRequest or GetNextRequest value
            if (request == "get")
                packet[pos++] = 0xA0;
            else
                packet[pos++] = 0xA1;

            packet[pos++] = Convert.ToByte(20 + miblen - 1); // Size of total MIB

            // Request ID
            packet[pos++] = 0x02; // Integer type
            packet[pos++] = 0x04; // length
            packet[pos++] = 0x00; // SNMP request ID
            packet[pos++] = 0x00;
            packet[pos++] = 0x00;
            packet[pos++] = 0x01;

            // Error status
            packet[pos++] = 0x02; // Integer type
            packet[pos++] = 0x01; // length
            packet[pos++] = 0x00; // SNMP error status

            // Error index
            packet[pos++] = 0x02; // Integer type
            packet[pos++] = 0x01; // length
            packet[pos++] = 0x00; // SNMP error index

            // Start of variable bindings
            packet[pos++] = 0x30; // Start of variable bindings sequence

            packet[pos++] = Convert.ToByte(6 + miblen - 1); // Size of variable binding

            packet[pos++] = 0x30; // Start of first variable bindings sequence
            packet[pos++] = Convert.ToByte(6 + miblen - 1 - 2); // size
            packet[pos++] = 0x06; // Object type
            packet[pos++] = Convert.ToByte(miblen - 1); // length

            // Start of MIB
            packet[pos++] = 0x2b;

            // Place MIB array in packet
            for (var i = 2; i < miblen; i++)
                packet[pos++] = Convert.ToByte(mib[i]);

            packet[pos++] = 0x05; // Null object value
            packet[pos] = 0x00; // Null

            // Send packet to destination
            SendPacket(host, packet, snmplen);

            return packet;
        }

        private static void SendPacket(IPEndPoint host, byte[] packet, int snmplen)
        {
            var sock = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Dgram,
                ProtocolType.Udp);
            sock.SetSocketOption(
                SocketOptionLevel.Socket,
                SocketOptionName.ReceiveTimeout,
                5000);
            var ep = (EndPoint) host;
            sock.SendTo(packet, snmplen, SocketFlags.None, host);

            // Receive response from packet
            try
            {
                sock.ReceiveFrom(packet, ref ep);
            }
            catch (SocketException)
            {
                packet[0] = 0xff;
            }
        }
    }
}