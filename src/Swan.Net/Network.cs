namespace Swan.Net;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides miscellaneous network utilities such as a Public IP finder,
/// a DNS client to query DNS records of any kind, and an NTP client.
/// </summary>
public static class Network
{
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
            if (properties.GatewayAddresses.Count == 0
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
}
