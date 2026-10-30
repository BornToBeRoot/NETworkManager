using System;
using System.Net;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class IPScannerHostRoleHelperTests
{
    private static NetworkInterfaceInfo CreateInterface(bool isOperational = true)
    {
        return new NetworkInterfaceInfo
        {
            IsOperational = isOperational,
            IPv4Address =
                [new Tuple<IPAddress, IPAddress>(IPAddress.Parse("192.168.1.23"), IPAddress.Parse("255.255.255.0"))],
            IPv6Address = [IPAddress.Parse("2001:db8::23")],
            IPv6AddressLinkLocal = [IPAddress.Parse("fe80::23%12")],
            IPv4Gateway = [IPAddress.Parse("192.168.1.1")],
            IPv6Gateway = [IPAddress.Parse("fe80::1%12")],
            DNSServer = [IPAddress.Parse("192.168.1.1"), IPAddress.Parse("192.168.1.2")],
            DhcpServer = [IPAddress.Parse("192.168.1.1")]
        };
    }

    [Theory]
    [InlineData("192.168.1.23", IPScannerHostRole.LocalComputer)]
    [InlineData("2001:db8::23", IPScannerHostRole.LocalComputer)]
    [InlineData("fe80::23", IPScannerHostRole.LocalComputer)]
    [InlineData("fe80::23%12", IPScannerHostRole.LocalComputer)]
    [InlineData("192.168.1.1",
        IPScannerHostRole.DefaultGateway | IPScannerHostRole.DNSServer | IPScannerHostRole.DHCPServer)]
    [InlineData("fe80::1%3", IPScannerHostRole.DefaultGateway)]
    [InlineData("192.168.1.2", IPScannerHostRole.DNSServer)]
    [InlineData("192.168.1.50", IPScannerHostRole.None)]
    public void GetRole(string ipAddress, IPScannerHostRole expected)
    {
        var roles = IPScannerHostRoleHelper.GetRoles([CreateInterface()]);

        Assert.Equal(expected, IPScannerHostRoleHelper.GetRole(roles, IPAddress.Parse(ipAddress)));
    }

    [Fact]
    public void GetRoles_NotOperational_Skipped()
    {
        Assert.Empty(IPScannerHostRoleHelper.GetRoles([CreateInterface(false)]));
    }

    [Theory]
    [InlineData(IPScannerHostRole.None, IPScannerHostRole.None)]
    [InlineData(IPScannerHostRole.DHCPServer | IPScannerHostRole.DNSServer, IPScannerHostRole.DNSServer)]
    [InlineData(IPScannerHostRole.DNSServer | IPScannerHostRole.DefaultGateway, IPScannerHostRole.DefaultGateway)]
    [InlineData(IPScannerHostRole.LocalComputer | IPScannerHostRole.DNSServer, IPScannerHostRole.LocalComputer)]
    public void GetPrimaryRole(IPScannerHostRole role, IPScannerHostRole expected)
    {
        Assert.Equal(expected, IPScannerHostRoleHelper.GetPrimaryRole(role));
    }
}
