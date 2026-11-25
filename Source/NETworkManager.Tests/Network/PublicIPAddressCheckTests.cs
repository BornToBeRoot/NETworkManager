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
    // Custom API with the address in a JSON object
    [InlineData("{\"ip\":\"203.0.113.10\"}", "203.0.113.10")]
    [InlineData("{ \"country\": \"DE\", \"ip\": \"203.0.113.10\" }", "203.0.113.10")]
    public void ParseResponse_IPv4_Success(string content, string expected)
    {
        var result = PublicIPAddressCheck.ParseResponse(true, content, AddressFamily.InterNetwork);

        Assert.Equal(PublicIPAddressCheckStatus.Success, result.Status);
        Assert.Equal(IPAddress.Parse(expected), result.IPAddress);
    }

    [Theory]
    [InlineData("2001:db8:1234::10")]
    [InlineData("2001:db8:1234::10\n")]
    [InlineData("{\"ip\":\"2001:db8:1234::10\"}")]
    public void ParseResponse_IPv6_Success(string content)
    {
        var result = PublicIPAddressCheck.ParseResponse(true, content, AddressFamily.InterNetworkV6);

        Assert.Equal(PublicIPAddressCheckStatus.Success, result.Status);
        Assert.Equal(IPAddress.Parse("2001:db8:1234::10"), result.IPAddress);
    }

    [Theory]
    // e.g. the answer of a captive portal or a proxy
    [InlineData("10.1.0.1", AddressFamily.InterNetwork)]
    [InlineData("{\"ip\":\"192.168.1.1\"}", AddressFamily.InterNetwork)]
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
    // HTML page of a captive portal with a link to its (public) login server
    [InlineData(true, "<html><a href=\"http://203.0.113.5/login\">Sign in</a></html>", AddressFamily.InterNetwork)]
    [InlineData(true, "<html>Sign in: http://192.168.1.1/login</html>", AddressFamily.InterNetwork)]
    [InlineData(true, "Your IP: 203.0.113.10", AddressFamily.InterNetwork)]
    // JSON without an address of the family
    [InlineData(true, "{\"ip\":\"2001:db8::10\"}", AddressFamily.InterNetwork)]
    // Address of the other address family
    [InlineData(true, "2001:db8::10", AddressFamily.InterNetwork)]
    [InlineData(true, "203.0.113.10", AddressFamily.InterNetworkV6)]
    public void ParseResponse_InvalidResponse(bool isSuccessStatusCode, string content, AddressFamily addressFamily)
    {
        var result = PublicIPAddressCheck.ParseResponse(isSuccessStatusCode, content, addressFamily);

        Assert.Equal(PublicIPAddressCheckStatus.InvalidResponse, result.Status);
        Assert.Null(result.IPAddress);
    }

    [Theory]
    // IPAddress.TryParse accepts these as IPv4 (e.g. an HTTP status code as answer)
    [InlineData("429", AddressFamily.InterNetwork)]
    [InlineData("1", AddressFamily.InterNetwork)]
    [InlineData("10.1", AddressFamily.InterNetwork)]
    [InlineData("0x7f000001", AddressFamily.InterNetwork)]
    // Unspecified, multicast, reserved / broadcast
    [InlineData("0.0.0.0", AddressFamily.InterNetwork)]
    [InlineData("224.0.0.1", AddressFamily.InterNetwork)]
    [InlineData("255.255.255.255", AddressFamily.InterNetwork)]
    [InlineData("::", AddressFamily.InterNetworkV6)]
    [InlineData("ff02::1", AddressFamily.InterNetworkV6)]
    // Scope id
    [InlineData("fe80::1%12", AddressFamily.InterNetworkV6)]
    public void ParseResponse_NotAnIPAddressOrReserved_InvalidResponse(string content, AddressFamily addressFamily)
    {
        var result = PublicIPAddressCheck.ParseResponse(true, content, addressFamily);

        Assert.Equal(PublicIPAddressCheckStatus.InvalidResponse, result.Status);
        Assert.Null(result.IPAddress);
    }

    [Theory]
    [InlineData("203.0.113.10", PublicIPAddressCheckStatus.Success)]
    [InlineData("2001:db8::10", PublicIPAddressCheckStatus.Success)]
    [InlineData("100.64.0.1", PublicIPAddressCheckStatus.PrivateIPAddress)]
    [InlineData("100.127.255.254", PublicIPAddressCheckStatus.PrivateIPAddress)]
    [InlineData("100.128.0.1", PublicIPAddressCheckStatus.Success)]
    [InlineData("172.16.0.1", PublicIPAddressCheckStatus.PrivateIPAddress)]
    [InlineData("::1", PublicIPAddressCheckStatus.PrivateIPAddress)]
    [InlineData("fe80::1", PublicIPAddressCheckStatus.PrivateIPAddress)]
    [InlineData("0.1.2.3", PublicIPAddressCheckStatus.InvalidResponse)]
    [InlineData("::ffff:10.1.2.3", PublicIPAddressCheckStatus.InvalidResponse)]
    [InlineData("::ffff:203.0.113.10", PublicIPAddressCheckStatus.InvalidResponse)]
    [InlineData("fec0::1", PublicIPAddressCheckStatus.PrivateIPAddress)]
    [InlineData("240.0.0.1", PublicIPAddressCheckStatus.InvalidResponse)]
    public void GetStatus(string ipAddress, PublicIPAddressCheckStatus expected)
    {
        Assert.Equal(expected, PublicIPAddressCheck.GetStatus(IPAddress.Parse(ipAddress)));
    }

    // Answer formats of public IP address APIs (examples in the documentation and other common ones), with
    // documentation addresses. checkip.amazonaws.com: real answer (line break at the end).
    [Theory]
    [InlineData("api.ipify.org, ip4.seeip.org, ifconfig.me/ip", "203.0.113.10", AddressFamily.InterNetwork, "203.0.113.10")]
    [InlineData("icanhazip.com, ifconfig.co/ip, ipinfo.io/ip, ident.me, checkip.amazonaws.com", "203.0.113.10\n", AddressFamily.InterNetwork, "203.0.113.10")]
    [InlineData("ip-api.com/line/?fields=query", "203.0.113.10\n", AddressFamily.InterNetwork, "203.0.113.10")]
    [InlineData("api.ipify.org?format=json, api.seeip.org/jsonip", "{\"ip\":\"203.0.113.10\"}", AddressFamily.InterNetwork, "203.0.113.10")]
    [InlineData("JSON with other values (e.g. success, type)", "{\"success\":true,\"ip\":\"203.0.113.10\",\"type\":\"IPv4\"}", AddressFamily.InterNetwork, "203.0.113.10")]
    [InlineData("ip-api.com/json", "{\"status\":\"success\",\"country\":\"Germany\",\"countryCode\":\"DE\",\"region\":\"BE\",\"regionName\":\"Land Berlin\",\"city\":\"Berlin\",\"zip\":\"10115\",\"lat\":52.5,\"lon\":13.4,\"timezone\":\"Europe/Berlin\",\"isp\":\"Example ISP\",\"org\":\"\",\"as\":\"AS64496 Example\",\"query\":\"203.0.113.10\"}", AddressFamily.InterNetwork, "203.0.113.10")]
    [InlineData("ipinfo.io/json", "{\n  \"ip\": \"203.0.113.10\",\n  \"hostname\": \"host.example.com\",\n  \"city\": \"Berlin\",\n  \"loc\": \"52.5244,13.4105\",\n  \"readme\": \"https://ipinfo.io/missingauth\"\n}", AddressFamily.InterNetwork, "203.0.113.10")]
    [InlineData("ifconfig.co/json", "{\n  \"ip\": \"203.0.113.10\",\n  \"ip_decimal\": 3405803786,\n  \"country\": \"Germany\"\n}", AddressFamily.InterNetwork, "203.0.113.10")]
    [InlineData("api6.ipify.org, v6.ident.me", "2001:db8:85a3::8a2e:370:7334", AddressFamily.InterNetworkV6, "2001:db8:85a3::8a2e:370:7334")]
    [InlineData("ipv6.icanhazip.com", "2001:db8:85a3::8a2e:370:7334\n", AddressFamily.InterNetworkV6, "2001:db8:85a3::8a2e:370:7334")]
    [InlineData("api64.ipify.org?format=json", "{\"ip\":\"2001:db8:85a3::8a2e:370:7334\"}", AddressFamily.InterNetworkV6, "2001:db8:85a3::8a2e:370:7334")]
    [InlineData("JSON with other values (IPv6)", "{\"success\":true,\"ip\":\"2001:db8:85a3::8a2e:370:7334\",\"type\":\"IPv6\"}", AddressFamily.InterNetworkV6, "2001:db8:85a3::8a2e:370:7334")]
    // A dual-stack API answering with IPv6 to the IPv4 request: no valid answer for IPv4
    [InlineData("api64.ipify.org (IPv6 answer)", "2001:db8:85a3::8a2e:370:7334", AddressFamily.InterNetwork, null)]
    // Cloudflare trace (key=value lines, h= is the host, not the client): not supported
    [InlineData("1.1.1.1/cdn-cgi/trace", "fl=123f45\nh=1.1.1.1\nip=203.0.113.10\nts=1700000000.123\n", AddressFamily.InterNetwork, null)]
    public void ParseResponse_KnownAPIs(string api, string content, AddressFamily addressFamily, string expected)
    {
        var result = PublicIPAddressCheck.ParseResponse(true, content, addressFamily);

        Assert.True(expected == null
                ? result.Status == PublicIPAddressCheckStatus.InvalidResponse
                : result.Status == PublicIPAddressCheckStatus.Success && result.IPAddress.Equals(IPAddress.Parse(expected)),
            $"{api}: {result.Status} {result.IPAddress}");
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
