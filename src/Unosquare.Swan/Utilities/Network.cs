namespace Unosquare.Swan.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;

    /// <summary>
    /// Provides miscellaneous network utilities
    /// </summary>
    static public class Network
    {

        /// <summary>
        /// Gets the name of the host.
        /// </summary>
        static public string HostName
        {
            get
            {
                var properties = IPGlobalProperties.GetIPGlobalProperties();
                return properties.HostName;
            }
        }

        /// <summary>
        /// Gets the name of the domain.
        /// </summary>
        static public string DomainName
        {
            get
            {
                var properties = IPGlobalProperties.GetIPGlobalProperties();
                return properties.DomainName;
            }
        }

        /// <summary>
        /// Gets the active IPv4 interfaces.
        /// Only those interfaces with a valid unicast address and a valid gateway will be returned in the collection
        /// </summary>
        /// <returns></returns>
        static public Dictionary<NetworkInterface, IPInterfaceProperties> GetActiveIPv4Interfaces()
        {
            // zero conf ip address
            var zeroConf = new IPAddress(0);

            var adapters = NetworkInterface.GetAllNetworkInterfaces()
                .Where(network =>
                    network.OperationalStatus == OperationalStatus.Up
                    && network.NetworkInterfaceType != NetworkInterfaceType.Unknown
                    && network.NetworkInterfaceType != NetworkInterfaceType.Loopback).ToArray();

            var result = new Dictionary<NetworkInterface, IPInterfaceProperties>();

            foreach (var adapter in adapters)
            {
                var properties = adapter.GetIPProperties();
                if (properties == null
                    || properties.GatewayAddresses.Count == 0
                    || properties.GatewayAddresses.All(gateway => gateway.Address == zeroConf)
                    || properties.UnicastAddresses.Count == 0
                    || properties.GatewayAddresses.All(address => address.Address == zeroConf)
                    || properties.UnicastAddresses.Any(a => a.Address.AddressFamily == AddressFamily.InterNetwork) == false)
                    continue;

                result[adapter] = properties;
            }

            return result;

        }

        /// <summary>
        /// Retrieves the local ip addresses.
        /// </summary>
        /// <param name="includeLoopback">if set to <c>true</c> [include loopback].</param>
        /// <returns></returns>
        static public IPAddress[] RetrieveLocalIPAddresses(bool includeLoopback = true)
        {
            return RetrieveLocalIPAddresses(NetworkInterfaceType.Unknown, true, includeLoopback);
        }

        /// <summary>
        /// Retrieves the local ip addresses.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <returns></returns>
        static public IPAddress[] RetrieveLocalIPAddresses(NetworkInterfaceType interfaceType)
        {
            return RetrieveLocalIPAddresses(interfaceType, false, false);
        }

        /// <summary>
        /// Retrieves the local ip addresses.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="skipTypeFilter">if set to <c>true</c> [skip type filter].</param>
        /// <param name="includeLoopback">if set to <c>true</c> [include loopback].</param>
        /// <returns></returns>
        private static IPAddress[] RetrieveLocalIPAddresses(NetworkInterfaceType interfaceType, bool skipTypeFilter, bool includeLoopback)
        {
            var addressList = new List<IPAddress>();
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni =>
                    (skipTypeFilter || ni.NetworkInterfaceType == interfaceType) &&
                    ni.OperationalStatus == OperationalStatus.Up
#if NET452
                    && ni.IsReceiveOnly == false
#endif
                    ).ToArray();

            foreach (var networkInterface in interfaces)
            {
                var properties = networkInterface.GetIPProperties();
                if (properties.GatewayAddresses.FirstOrDefault(
                    g => g.Address.AddressFamily == AddressFamily.InterNetwork) == null)
                    continue;

                var addresses = properties.UnicastAddresses
                    .Where(i => i.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(i => i.Address)
                    .ToArray();

                if (addresses.Length > 0)
                    addressList.AddRange(addresses);
            }

            if (includeLoopback || interfaceType == NetworkInterfaceType.Loopback)
                addressList.Add(IPAddress.Loopback);

            return addressList.ToArray();
        }

        /// <summary>
        /// Gets the configured IPv4 DNS servers for the active network interfaces.
        /// </summary>
        /// <returns></returns>
        public static IPAddress[] GetConfiguredIPv4DnsServers()
        {
            var adapters = GetActiveIPv4Interfaces();
            return adapters
                .Select(a => a.Value.DnsAddresses.Where(d => d.AddressFamily == AddressFamily.InterNetwork))
                .SelectMany(d => d).ToArray();
        }

        /// <summary>
        /// Gets the DNS host entry (a list of IP addresses) for the domain name.
        /// </summary>
        /// <param name="fqdn">The FQDN.</param>
        /// <returns></returns>
        public static IPAddress[] GetDnsHostEntry(string fqdn)
        {
            return GetDnsHostEntry(fqdn, GetConfiguredIPv4DnsServers().FirstOrDefault(), Constants.DnsDefaultPort);
        }

        /// <summary>
        /// Gets the DNS host entry (a list of IP addresses) for the domain name.
        /// </summary>
        /// <param name="fqdn">The FQDN.</param>
        /// <param name="dnsServer">The DNS server.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        public static IPAddress[] GetDnsHostEntry(string fqdn, IPAddress dnsServer, int port)
        {
            var client = new DnsClient(dnsServer, port);
            var result = client.Lookup(fqdn);
            return result.ToArray();
        }

        /// <summary>
        /// Gets the UTC time by querying from an NTP server
        /// </summary>
        /// <param name="ntpServer">The NTP server, by default pool.ntp.org.</param>
        /// <param name="port">The port, by default NTP 123.</param>
        /// <returns></returns>
        public static DateTime GetNetworkTimeUtc(string ntpServer = "pool.ntp.org", int port = Constants.NtpDefaultPort)
        {
            // NTP message size - 16 bytes of the digest (RFC 2030)
            var ntpData = new byte[48];

            //Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)
            var addresses = GetDnsHostEntry(ntpServer);

            //The UDP port number assigned to NTP is 123
            var ipEndPoint = new IPEndPoint(addresses[0], port);
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            socket.Connect(ipEndPoint);
            socket.ReceiveTimeout = 3000; //Stops code hang if NTP is blocked
            socket.Send(ntpData);
            socket.Receive(ntpData);
            socket.Dispose();
            socket = null;

            //Offset to get to the "Transmit Timestamp" field (time at which the reply 
            //departed the server for the client, in 64-bit timestamp format."
            const byte serverReplyTime = 40;

            //Get the seconds part
            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

            //Get the seconds fraction
            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

            //Convert From big-endian to little-endian to match the platform
            if (BitConverter.IsLittleEndian)
            {
                intPart = intPart.SwapEndianness();
                fractPart = intPart.SwapEndianness();
            }

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            // The time is given in UTC
            var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);
            return networkDateTime;
        }
    }
}
