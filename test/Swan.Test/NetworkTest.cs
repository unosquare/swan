namespace Swan.Test.NetworkTests;

using NUnit.Framework;
using Net;
using Swan.Net.Extensions;
using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

public abstract class NetworkTest
{
    protected IPAddress PrivateIP { get; } = IPAddress.Parse("192.168.1.1");
    protected IPAddress PublicIP { get; } = IPAddress.Parse("200.1.1.1");
    protected IPAddress NullIP { get; } = null;
}

[TestFixture]
public class IsPrivateAddress : NetworkTest
{
    [Test]
    public void PrivateIPWithValidAddress_ReturnsTrue()
    {
        Assert.IsTrue(PrivateIP.IsPrivateAddress());
    }

    [Test]
    public void PublicIPWithValidAddress_ReturnsFalse()
    {
        Assert.IsFalse(PublicIP.IsPrivateAddress());
    }

    [Test]
    public void WithNullAddress_ReturnsFalse()
    {
        Assert.Throws<ArgumentNullException>(() => NullIP.IsPrivateAddress());
    }
}

[TestFixture]
public class ToUInt32 : NetworkTest
{
    [Test]
    public void PrivateIPWithValidAddress_ReturnsAddressAsInt()
    {
        Assert.AreEqual(3232235777, PrivateIP.ToUInt32());
    }

    [Test]
    public void PublicIPWithValidAddress_ReturnsAddressAsInt()
    {
        Assert.AreEqual(3355508993, PublicIP.ToUInt32());
    }

    [Test]
    public void WithNullAddress_ReturnsFalse()
    {
        Assert.Throws<ArgumentNullException>(() => NullIP.ToUInt32());
    }

    [Test]
    public void WithIPv6Address_ThrowsArgumentException()
    {
        var privateIP = IPAddress.Parse("2001:0db8:85a3:0000:1319:8a2e:0370:7344");

        Assert.Throws<ArgumentException>(() => privateIP.ToUInt32());
    }
}

[TestFixture]
public class GetIPv4Addresses : NetworkTest
{
    [Test]
    public void Wireless80211AsParam_ReturnsIPv4Address()
    {
        var networkType = Network.GetIPv4Addresses(NetworkInterfaceType.Wireless80211);

        Assert.IsNotNull(networkType);
    }

    [Test]
    public void LoopbackAsParam_ReturnsIPv4Address()
    {
        var networkType = Network.GetIPv4Addresses(NetworkInterfaceType.Loopback);

        Assert.AreEqual(networkType[0].ToString(), "127.0.0.1");
    }

    [Test]
    public void WithNoParam_ReturnsIPv4Address()
    {
        var networkType = Network.GetIPv4Addresses();

        Assert.IsNotNull(networkType);
    }
}

[TestFixture]
public class GetPublicIPAddress : NetworkTest
{
    [Test]
    public async Task WithNoParam_ReturnsIPAddress()
    {
        var publicIPAddress = await Network.GetPublicIPAddressAsync();

        Assert.IsNotEmpty(publicIPAddress.ToString());
    }
}
