using System;
using System.Linq;
using System.Net;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class NetworkInterfaceLocalSubnetsTests
{
    private static NetworkInterfaceInfo CreateInterface(string name, bool isOperational,
        params (string ipAddress, string subnetmask)[] addresses)
    {
        return new NetworkInterfaceInfo
        {
            Name = name,
            IsOperational = isOperational,
            IPv4Address = addresses.Select(x => new Tuple<IPAddress, IPAddress>(IPAddress.Parse(x.ipAddress),
                x.subnetmask == null ? null : IPAddress.Parse(x.subnetmask))).ToArray()
        };
    }

    [Fact]
    public void GetLocalSubnets_NetworkAddressAndCidr()
    {
        var subnets = NetworkInterface.GetLocalSubnets([
            CreateInterface("Ethernet", true, ("192.168.1.23", "255.255.255.0"), ("10.10.5.7", "255.255.0.0"))
        ]);

        Assert.Equal(["192.168.1.0/24 Ethernet", "10.10.0.0/16 Ethernet"],
            subnets.Select(x => $"{x.Subnet} {x.InterfaceName}"));
        Assert.All(subnets, x => Assert.False(x.IsLinkLocal));
        Assert.Equal("192.168.1.0/24 (Ethernet)", subnets[0].DisplayName);
    }

    [Fact]
    public void GetLocalSubnets_SkipsNotOperationalNoMaskAndSingleAddresses()
    {
        var subnets = NetworkInterface.GetLocalSubnets([
            CreateInterface("Down", false, ("192.168.2.1", "255.255.255.0")),
            CreateInterface("VPN", true, ("10.8.0.2", "255.255.255.255"), ("10.9.0.2", null)),
            CreateInterface("Wi-Fi", true, ("172.16.0.5", "255.255.240.0"))
        ]);

        Assert.Equal(["172.16.0.0/20"], subnets.Select(x => x.Subnet));
    }

    [Fact]
    public void GetLocalSubnets_LinkLocalAtTheEnd()
    {
        var subnets = NetworkInterface.GetLocalSubnets([
            CreateInterface("Ethernet 2", true, ("169.254.10.20", "255.255.0.0")),
            CreateInterface("Ethernet", true, ("192.168.1.23", "255.255.255.0"))
        ]);

        Assert.Equal(["192.168.1.0/24", "169.254.0.0/16"], subnets.Select(x => x.Subnet));
        Assert.True(subnets[1].IsLinkLocal);
    }

    [Fact]
    public void GetLocalSubnets_SameSubnetOncePerInterface()
    {
        var subnets = NetworkInterface.GetLocalSubnets([
            CreateInterface("Ethernet", true, ("192.168.1.23", "255.255.255.0"), ("192.168.1.24", "255.255.255.0")),
            CreateInterface("Wi-Fi", true, ("192.168.1.50", "255.255.255.0"))
        ]);

        Assert.Equal(["192.168.1.0/24 Ethernet", "192.168.1.0/24 Wi-Fi"],
            subnets.Select(x => $"{x.Subnet} {x.InterfaceName}"));
    }
}
