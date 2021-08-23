using Swan.Net.Dns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Swan.Net
{
    /// <summary>
    /// Provides miscellaneous network utilities such as a Public IP finder,
    /// a DNS client to query DNS records of any kind, and an NTP client.
    /// </summary>
    public static class Network
    {
        /// <summary>
        /// The DNS default port.
        /// </summary>
        public const int DnsDefaultPort = 53;

        /// <summary>
        /// The NTP default port.
        /// </summary>
        public const int NtpDefaultPort = 123;

        /// <summary>
        /// Gets the name of the host.
        /// </summary>
        /// <value>
        /// The name of the host.
        /// </value>
        public static string HostName => IPGlobalProperties.GetIPGlobalProperties().HostName;

        /// <summary>
        /// Gets the name of the network domain.
        /// </summary>
        /// <value>
        /// The name of the network domain.
        /// </value>
        public static string DomainName => IPGlobalProperties.GetIPGlobalProperties().DomainName;

        #region IP Addresses and Adapters Information Methods

        /// <summary>
        /// Gets the active IPv4 interfaces.
        /// Only those interfaces with a valid unicast address and a valid gateway will be returned in the collection.
        /// </summary>
        /// <returns>
        /// A collection of NetworkInterface/IPInterfaceProperties pairs
        /// that represents the active IPv4 interfaces.
        /// </returns>
        public static Dictionary<NetworkInterface, IPInterfaceProperties> GetIPv4Interfaces()
        {
            // zero conf ip address
            var zeroConf = new IPAddress(0);

            var adapters = NetworkInterface.GetAllNetworkInterfaces()
                .Where(network =>
                    network.OperationalStatus == OperationalStatus.Up
                    && network.NetworkInterfaceType != NetworkInterfaceType.Unknown
                    && network.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .ToArray();

            var result = new Dictionary<NetworkInterface, IPInterfaceProperties>();

            foreach (var adapter in adapters)
            {
                var properties = adapter.GetIPProperties();
                if (properties == null
                    || properties.GatewayAddresses.Count == 0
                    || properties.GatewayAddresses.All(gateway => Equals(gateway.Address, zeroConf))
                    || properties.UnicastAddresses.Count == 0
                    || properties.GatewayAddresses.All(address => Equals(address.Address, zeroConf))
                    || properties.UnicastAddresses.Any(a => a.Address.AddressFamily == AddressFamily.InterNetwork) ==
                    false)
                    continue;

                result[adapter] = properties;
            }

            return result;
        }

        /// <summary>
        /// Retrieves the local ip addresses.
        /// </summary>
        /// <param name="includeLoopback">if set to <c>true</c> [include loopback].</param>
        /// <returns>An array of local ip addresses.</returns>
        public static IPAddress[] GetIPv4Addresses(bool includeLoopback = true) =>
            GetIPv4Addresses(NetworkInterfaceType.Unknown, true, includeLoopback);

        /// <summary>
        /// Retrieves the local ip addresses.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="skipTypeFilter">if set to <c>true</c> [skip type filter].</param>
        /// <param name="includeLoopback">if set to <c>true</c> [include loopback].</param>
        /// <returns>An array of local ip addresses.</returns>
        public static IPAddress[] GetIPv4Addresses(
            NetworkInterfaceType interfaceType,
            bool skipTypeFilter = false,
            bool includeLoopback = false)
        {
            var addressList = new List<IPAddress>();
            var interfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni =>
                    !ni.IsReceiveOnly &&
                    (skipTypeFilter || ni.NetworkInterfaceType == interfaceType) &&
                    ni.OperationalStatus == OperationalStatus.Up)
                .ToArray();

            foreach (var networkInterface in interfaces)
            {
                var properties = networkInterface.GetIPProperties();

                if (properties.GatewayAddresses.All(g => g.Address.AddressFamily != AddressFamily.InterNetwork))
                    continue;

                addressList.AddRange(properties.UnicastAddresses
                    .Where(i => i.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(i => i.Address));
            }

            if (includeLoopback || interfaceType == NetworkInterfaceType.Loopback)
                addressList.Add(IPAddress.Loopback);

            return addressList.ToArray();
        }

        /// <summary>
        /// Gets the public IP address using ipify.org.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A public IP address of the result produced by this Task.</returns>
        public static async Task<IPAddress> GetPublicIPAddressAsync(CancellationToken cancellationToken = default)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(new Uri("https://api.ipify.org"), cancellationToken).ConfigureAwait(false);
            return IPAddress.Parse(await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// Gets the configured IPv4 DNS servers for the active network interfaces.
        /// </summary>
        /// <returns>
        /// A collection of NetworkInterface/IPInterfaceProperties pairs
        /// that represents the active IPv4 interfaces.
        /// </returns>
        public static IPAddress[] GetIPv4DnsServers()
            => GetIPv4Interfaces()
                .Select(a => a.Value.DnsAddresses.Where(d => d.AddressFamily == AddressFamily.InterNetwork))
                .SelectMany(d => d)
                .ToArray();

        #endregion

        #region DNS and NTP Clients

        /// <summary>
        /// Gets the DNS host entry (a list of IP addresses) for the domain name.
        /// </summary>
        /// <param name="fqdn">The FQDN.</param>
        /// <returns>An array of local ip addresses of the result produced by this task.</returns>
        public static Task<IPAddress[]> GetDnsHostEntryAsync(string fqdn)
        {
            var dnsServer = GetIPv4DnsServers().FirstOrDefault() ?? IPAddress.Parse("8.8.8.8");
            return GetDnsHostEntryAsync(fqdn, dnsServer, DnsDefaultPort);
        }

        /// <summary>
        /// Gets the DNS host entry (a list of IP addresses) for the domain name.
        /// </summary>
        /// <param name="fqdn">The FQDN.</param>
        /// <param name="dnsServer">The DNS server.</param>
        /// <param name="port">The port.</param>
        /// <returns>
        /// An array of local ip addresses of the result produced by this task.
        /// </returns>
        /// <exception cref="ArgumentNullException">fqdn.</exception>
        public static async Task<IPAddress[]> GetDnsHostEntryAsync(string fqdn, IPAddress dnsServer, int port)
        {
            if (fqdn == null)
                throw new ArgumentNullException(nameof(fqdn));

            if (!fqdn.Contains(".", StringComparison.Ordinal))
            {
                fqdn += "." + IPGlobalProperties.GetIPGlobalProperties().DomainName;
            }

            while (true)
            {
                if (!fqdn.EndsWith(".", StringComparison.OrdinalIgnoreCase)) break;

                fqdn = fqdn.Substring(0, fqdn.Length - 1);
            }

            var client = new DnsClient(dnsServer, port);
            var result = await client.Lookup(fqdn).ConfigureAwait(false);
            return result.ToArray();
        }

        /// <summary>
        /// Gets the reverse lookup FQDN of the given IP Address.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="dnsServer">The DNS server.</param>
        /// <param name="port">The port.</param>
        /// <returns>A <see cref="System.String" /> that represents the current object.</returns>
        public static Task<string> GetDnsPointerEntryAsync(IPAddress query, IPAddress dnsServer, int port)
        {
            var client = new DnsClient(dnsServer, port);
            return client.Reverse(query);
        }

        /// <summary>
        /// Gets the reverse lookup FQDN of the given IP Address.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>A <see cref="System.String" /> that represents the current object.</returns>
        public static Task<string> GetDnsPointerEntryAsync(IPAddress query)
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
        /// <returns>Queries the DNS server for the specified record type of the result produced by this Task.</returns>
        public static async Task<DnsQueryResult> QueryDnsAsync(string query, DnsRecordType recordType, IPAddress dnsServer, int port)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var client = new DnsClient(dnsServer, port);
            var response = await client.Resolve(query, recordType).ConfigureAwait(false);
            return new DnsQueryResult(response);
        }

        /// <summary>
        /// Queries the DNS server for the specified record type.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="recordType">Type of the record.</param>
        /// <returns>Queries the DNS server for the specified record type of the result produced by this Task.</returns>
        public static Task<DnsQueryResult> QueryDnsAsync(string query, DnsRecordType recordType) => QueryDnsAsync(query, recordType, GetIPv4DnsServers().FirstOrDefault(), DnsDefaultPort);

        /// <summary>
        /// Gets the UTC time by querying from an NTP server.
        /// </summary>
        /// <param name="ntpServerAddress">The NTP server address.</param>
        /// <param name="port">The port.</param>
        /// <returns>The UTC time by querying from an NTP server of the result produced by this Task.</returns>
        public static async Task<DateTime> GetNetworkTimeUtcAsync(IPAddress ntpServerAddress, int port = NtpDefaultPort)
        {
            if (ntpServerAddress == null)
                throw new ArgumentNullException(nameof(ntpServerAddress));

            // NTP message size - 16 bytes of the digest (RFC 2030)
            var ntpData = new byte[48];

            // Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B; // LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            // The UDP port number assigned to NTP is 123
            var endPoint = new IPEndPoint(ntpServerAddress, port);

            try
            {
                using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

#if !NET461
                await socket.ConnectAsync(endPoint).ConfigureAwait(false);
#else
                socket.Connect(endPoint);
#endif

                socket.ReceiveTimeout = 3000; // Stops code hang if NTP is blocked
                socket.Send(ntpData);
                socket.Receive(ntpData);
                socket.Dispose();
            }
            catch
            {
                return new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            }

            // Offset to get to the "Transmit Timestamp" field (time at which the reply 
            // departed the server for the client, in 64-bit timestamp format."
            const byte serverReplyTime = 40;

            // Get the seconds part
            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);

            // Get the seconds fraction
            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

            // Convert From big-endian to little-endian to match the platform
            if (BitConverter.IsLittleEndian)
            {
                intPart = intPart.SwapEndianness();
                fractPart = intPart.SwapEndianness();
            }

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            // The time is given in UTC
            return new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((long)milliseconds);
        }

        /// <summary>
        /// Gets the UTC time by querying from an NTP server.
        /// </summary>
        /// <param name="ntpServerName">The NTP server, by default pool.ntp.org.</param>
        /// <param name="port">The port, by default NTP 123.</param>
        /// <returns>The UTC time by querying from an NTP server of the result produced by this Task.</returns>
        public static async Task<DateTime> GetNetworkTimeUtcAsync(string ntpServerName = "pool.ntp.org",
            int port = NtpDefaultPort)
        {
            var addresses = await GetDnsHostEntryAsync(ntpServerName).ConfigureAwait(false);
            return await GetNetworkTimeUtcAsync(addresses.First(), port).ConfigureAwait(false);
        }

        #endregion
    }
}