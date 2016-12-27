namespace Unosquare.Swan.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides miscellaneous network utilities such as a Public IP finder,
    /// a DNS client to query DNS records of any kind, and an NTP client.
    /// </summary>
    public static class Network
    {
        #region Properties

        /// <summary>
        /// Gets the name of the host.
        /// </summary>
        public static string HostName
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
        public static string DomainName
        {
            get
            {
                var properties = IPGlobalProperties.GetIPGlobalProperties();
                return properties.DomainName;
            }
        }

        #endregion

        #region IP Addresses and Adapters Information Methods

        /// <summary>
        /// Gets the active IPv4 interfaces.
        /// Only those interfaces with a valid unicast address and a valid gateway will be returned in the collection
        /// </summary>
        /// <returns></returns>
        public static Dictionary<NetworkInterface, IPInterfaceProperties> GetIPv4Interfaces()
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
        public static IPAddress[] GetIPv4Addresses(bool includeLoopback = true)
        {
            return GetIPv4Addresses(NetworkInterfaceType.Unknown, true, includeLoopback);
        }

        /// <summary>
        /// Retrieves the local IP addresses.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <returns></returns>
        public static IPAddress[] GetIPv4Addresses(NetworkInterfaceType interfaceType)
        {
            return GetIPv4Addresses(interfaceType, false, false);
        }

        /// <summary>
        /// Retrieves the local ip addresses.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="skipTypeFilter">if set to <c>true</c> [skip type filter].</param>
        /// <param name="includeLoopback">if set to <c>true</c> [include loopback].</param>
        /// <returns></returns>
        private static IPAddress[] GetIPv4Addresses(NetworkInterfaceType interfaceType, bool skipTypeFilter, bool includeLoopback)
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
        /// Gets the public IP address using ipify.org.
        /// </summary>
        /// <returns></returns>
        public static async Task<IPAddress> GetPublicIPAddressAsync()
        {
            using (var client = new HttpClient())
            {
                return IPAddress.Parse(await client.GetStringAsync("https://api.ipify.org"));
            }
        }

        /// <summary>
        /// Gets the public IP address using ipify.org.
        /// </summary>
        /// <returns></returns>
        public static IPAddress GetPublicIPAddress()
        {
            return GetPublicIPAddressAsync().Result;
        }

        /// <summary>
        /// Gets the configured IPv4 DNS servers for the active network interfaces.
        /// </summary>
        /// <returns></returns>
        public static IPAddress[] GetIPv4DnsServers()
        {
            var adapters = GetIPv4Interfaces();
            return adapters
                .Select(a => a.Value.DnsAddresses.Where(d => d.AddressFamily == AddressFamily.InterNetwork))
                .SelectMany(d => d).ToArray();
        }

        #endregion

        #region DNS and NTP Clients

        /// <summary>
        /// Gets the DNS host entry (a list of IP addresses) for the domain name.
        /// </summary>
        /// <param name="fqdn">The FQDN.</param>
        /// <returns></returns>
        public static IPAddress[] GetDnsHostEntry(string fqdn)
        {
            return GetDnsHostEntry(fqdn, GetIPv4DnsServers().FirstOrDefault(), Constants.DnsDefaultPort);
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
        /// Gets the reverse lookup FQDN of the given IP Address.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="dnsServer">The DNS server.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        public static string GetDnsPointerEntry(IPAddress query, IPAddress dnsServer, int port)
        {
            var client = new DnsClient(dnsServer, port);
            return client.Reverse(query);
        }

        /// <summary>
        /// Gets the reverse lookup FQDN of the given IP Address.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns></returns>
        public static string GetDnsPointerEntry(IPAddress query)
        {
            var client = new DnsClient(GetIPv4DnsServers().FirstOrDefault());
            return client.Reverse(query);
        }

        /// <summary>
        /// Queries the DNS server for the specified record type.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="recordType">Type of the record.</param>
        /// <param name="dnsServer">The DNS server.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        public static DnsQueryResult QueryDns(string query, DnsRecordType recordType, IPAddress dnsServer, int port)
        {
            var client = new DnsClient(dnsServer, port);
            var response = client.Resolve(query, recordType);
            return new DnsQueryResult(response);
        }

        /// <summary>
        /// Queries the DNS server for the specified record type.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="recordType">Type of the record.</param>
        /// <returns></returns>
        public static DnsQueryResult QueryDns(string query, DnsRecordType recordType)
        {
            return QueryDns(query, recordType, GetIPv4DnsServers().FirstOrDefault(), Constants.DnsDefaultPort);
        }

        /// <summary>
        /// Gets the UTC time by querying from an NTP server
        /// </summary>
        /// <param name="ntpServerAddress">The NTP server address.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        public static DateTime GetNetworkTimeUtc(IPAddress ntpServerAddress, int port = Constants.NtpDefaultPort)
        {
            // NTP message size - 16 bytes of the digest (RFC 2030)
            var ntpData = new byte[48];

            //Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            //The UDP port number assigned to NTP is 123
            var ipEndPoint = new IPEndPoint(ntpServerAddress, port);
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

        /// <summary>
        /// Gets the UTC time by querying from an NTP server
        /// </summary>
        /// <param name="ntpServerName">The NTP server, by default pool.ntp.org.</param>
        /// <param name="port">The port, by default NTP 123.</param>
        /// <returns></returns>
        public static DateTime GetNetworkTimeUtc(string ntpServerName = "pool.ntp.org", int port = Constants.NtpDefaultPort)
        {
            var addresses = GetDnsHostEntry(ntpServerName);
            return GetNetworkTimeUtc(addresses.First(), port);
        }

        #endregion
    }
}
