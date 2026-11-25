using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class PublicIPAddressCheckTests
{
    [Theory]
    [InlineData("203.0.113.10", "203.0.113.10")]
    [InlineData("203.0.113.10\n", "203.0.113.10")]
    [InlineData("  203.0.113.10  ", "203.0.113.10")]
    // Custom API with the address in a text / JSON
    [InlineData("{\"ip\":\"203.0.113.10\"}", "203.0.113.10")]
    [InlineData("Your IP: 203.0.113.10", "203.0.113.10")]
    public void ParseResponse_IPv4_Success(string content, string expected)
    {
        var result = PublicIPAddressCheck.ParseResponse(true, content, AddressFamily.InterNetwork);

        Assert.Equal(PublicIPAddressCheckStatus.Success, result.Status);
        Assert.Equal(IPAddress.Parse(expected), result.IPAddress);
    }

    [Theory]
    [InlineData("2001:db8:1234::10")]
    [InlineData("2001:db8:1234::10\n")]
    public void ParseResponse_IPv6_Success(string content)
    {
        var result = PublicIPAddressCheck.ParseResponse(true, content, AddressFamily.InterNetworkV6);

        Assert.Equal(PublicIPAddressCheckStatus.Success, result.Status);
        Assert.Equal(IPAddress.Parse("2001:db8:1234::10"), result.IPAddress);
    }

    [Theory]
    // e.g. the page of a captive portal or a proxy
    [InlineData("<html>Please sign in: http://192.168.1.1/login</html>", AddressFamily.InterNetwork)]
    [InlineData("10.1.0.1", AddressFamily.InterNetwork)]
    [InlineData("169.254.1.1", AddressFamily.InterNetwork)]
    [InlineData("127.0.0.1", AddressFamily.InterNetwork)]
    [InlineData("fd00::1", AddressFamily.InterNetworkV6)]
    public void ParseResponse_PrivateIPAddress(string content, AddressFamily addressFamily)
    {
        Assert.Equal(PublicIPAddressCheckStatus.PrivateIPAddress,
            PublicIPAddressCheck.ParseResponse(true, content, addressFamily).Status);
    }

    [Theory]
    [InlineData(false, "203.0.113.10", AddressFamily.InterNetwork)]
    [InlineData(true, "", AddressFamily.InterNetwork)]
    [InlineData(true, null, AddressFamily.InterNetwork)]
    [InlineData(true, "<html>Forbidden</html>", AddressFamily.InterNetwork)]
    // Address of the other address family
    [InlineData(true, "2001:db8::10", AddressFamily.InterNetwork)]
    [InlineData(true, "203.0.113.10", AddressFamily.InterNetworkV6)]
    public void ParseResponse_InvalidResponse(bool isSuccessStatusCode, string content, AddressFamily addressFamily)
    {
        var result = PublicIPAddressCheck.ParseResponse(isSuccessStatusCode, content, addressFamily);

        Assert.Equal(PublicIPAddressCheckStatus.InvalidResponse, result.Status);
        Assert.Null(result.IPAddress);
    }

    [Fact]
    public void FromException_NameResolutionFailed()
    {
        var exception = new HttpRequestException("No such host is known.",
            new SocketException((int)SocketError.HostNotFound));

        Assert.Equal(PublicIPAddressCheckStatus.NameResolutionFailed,
            PublicIPAddressCheck.FromException(exception).Status);
    }

    [Fact]
    public void FromException_RequestFailed()
    {
        Assert.Equal(PublicIPAddressCheckStatus.RequestFailed,
            PublicIPAddressCheck.FromException(new HttpRequestException("Connection refused",
                new SocketException((int)SocketError.ConnectionRefused))).Status);
        Assert.Equal(PublicIPAddressCheckStatus.RequestFailed,
            PublicIPAddressCheck.FromException(new TaskCanceledException("Timeout")).Status);
    }
}
