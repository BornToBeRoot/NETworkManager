using System.Net;
using System.Net.Sockets;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using Xunit;

namespace NETworkManager.Tests.Network;

public class NetworkConnectionCheckTests
{
    private static IPAddress Ip(string ipAddress) => ipAddress == null ? null : IPAddress.Parse(ipAddress);

    [Theory]
    [InlineData("192.168.1.10", true, ConnectionState.OK, ConnectionCheckReason.None)]
    [InlineData(null, false, ConnectionState.Critical, ConnectionCheckReason.NoIPv4Address)]
    // DHCP failed (#3288: the address is still shown)
    [InlineData("169.254.10.20", false, ConnectionState.Warning, ConnectionCheckReason.IPv4LinkLocal)]
    [InlineData("169.254.10.20", true, ConnectionState.Warning, ConnectionCheckReason.IPv4LinkLocal)]
    // No route: address of the first active network adapter (fallback)
    [InlineData("192.168.1.10", false, ConnectionState.Warning, ConnectionCheckReason.NoRoute)]
    public void GetLocalIPAddressResult_IPv4(string ipAddress, bool isRouted, ConnectionState state,
        ConnectionCheckReason reason)
    {
        Assert.Equal(new ConnectionCheckResult(state, reason),
            NetworkConnectionCheck.GetLocalIPAddressResult(Ip(ipAddress), AddressFamily.InterNetwork, isRouted));
    }

    [Theory]
    [InlineData("2001:db8::10", true, ConnectionState.OK, ConnectionCheckReason.None)]
    [InlineData("fd00::10", true, ConnectionState.OK, ConnectionCheckReason.None)]
    // IPv4-only network: not an error
    [InlineData(null, false, ConnectionState.Info, ConnectionCheckReason.IPv6NotAvailable)]
    [InlineData("fe80::10", false, ConnectionState.Info, ConnectionCheckReason.IPv6NotAvailable)]
    // Unique local address without a route to the internet (e.g. some routers)
    [InlineData("fd00::10", false, ConnectionState.Info, ConnectionCheckReason.IPv6NotAvailable)]
    public void GetLocalIPAddressResult_IPv6(string ipAddress, bool isRouted, ConnectionState state,
        ConnectionCheckReason reason)
    {
        Assert.Equal(new ConnectionCheckResult(state, reason),
            NetworkConnectionCheck.GetLocalIPAddressResult(Ip(ipAddress), AddressFamily.InterNetworkV6, isRouted));
    }

    private static DNSClientResultString Resolved(string hostname) => new(hostname, "192.168.1.1:53");

    private static DNSClientResultString NotFound() => new(true, "Non-Existent Domain", "192.168.1.1:53")
        { IsNotFound = true };

    private static DNSClientResultString Error() => new(true, "Timeout");

    [Fact]
    public void ShouldResolveIPv6()
    {
        Assert.True(NetworkConnectionCheck.ShouldResolveIPv6(null));
        Assert.True(NetworkConnectionCheck.ShouldResolveIPv6(NotFound()));
        Assert.False(NetworkConnectionCheck.ShouldResolveIPv6(Resolved("pc.example.com")));
        // The same DNS server would time out again
        Assert.False(NetworkConnectionCheck.ShouldResolveIPv6(Error()));
    }

    [Fact]
    public void GetDNSResult_ResolvedIPv4()
    {
        var (result, hostname) = NetworkConnectionCheck.GetDNSResult(Resolved("pc.example.com"), null);

        Assert.Equal(new ConnectionCheckResult(ConnectionState.OK), result);
        Assert.Equal("pc.example.com", hostname);
    }

    [Fact]
    public void GetDNSResult_ResolvedIPv6AfterIPv4NotFound()
    {
        var (result, hostname) = NetworkConnectionCheck.GetDNSResult(NotFound(), Resolved("pc6.example.com"));

        Assert.Equal(ConnectionState.OK, result.State);
        Assert.Equal("pc6.example.com", hostname);
    }

    [Fact]
    public void GetDNSResult_NotFoundWinsOverError()
    {
        // e.g. IPv4 has no PTR while IPv6 times out: no record, not a broken DNS setup
        var (result, hostname) = NetworkConnectionCheck.GetDNSResult(NotFound(), Error());

        Assert.Equal(new ConnectionCheckResult(ConnectionState.Info, ConnectionCheckReason.PTRNotFound), result);
        Assert.Null(hostname);
    }

    [Fact]
    public void GetDNSResult_Error()
    {
        Assert.Equal(new ConnectionCheckResult(ConnectionState.Critical, ConnectionCheckReason.DNSError),
            NetworkConnectionCheck.GetDNSResult(Error(), null).Result);
    }

    [Fact]
    public void GetDNSResult_NothingToResolve()
    {
        Assert.Equal(new ConnectionCheckResult(ConnectionState.Info, ConnectionCheckReason.NoIPAddressToResolve),
            NetworkConnectionCheck.GetDNSResult(null, null).Result);
    }

    [Theory]
    [InlineData("192.168.1.1", true, true, ConnectionState.OK, ConnectionCheckReason.None)]
    // Some routers block ICMP
    [InlineData("192.168.1.1", true, false, ConnectionState.Warning, ConnectionCheckReason.GatewayNoReply)]
    // Routed via an adapter without a gateway (e.g. VPN)
    [InlineData(null, true, false, ConnectionState.Warning, ConnectionCheckReason.NoGateway)]
    // No route and no gateway
    [InlineData(null, false, false, ConnectionState.Critical, ConnectionCheckReason.NoGateway)]
    public void GetGatewayResult(string gateway, bool isRouted, bool isReachable, ConnectionState state,
        ConnectionCheckReason reason)
    {
        Assert.Equal(new ConnectionCheckResult(state, reason),
            NetworkConnectionCheck.GetGatewayResult(Ip(gateway), isRouted, isReachable));
    }

    [Theory]
    [InlineData(PublicIPAddressCheckStatus.Success, ConnectionState.OK, ConnectionCheckReason.None)]
    [InlineData(PublicIPAddressCheckStatus.PrivateIPAddress, ConnectionState.Warning, ConnectionCheckReason.PublicIPAddressPrivate)]
    [InlineData(PublicIPAddressCheckStatus.InvalidResponse, ConnectionState.Critical, ConnectionCheckReason.PublicIPAddressRequestFailed)]
    [InlineData(PublicIPAddressCheckStatus.RequestFailed, ConnectionState.Critical, ConnectionCheckReason.PublicIPAddressRequestFailed)]
    [InlineData(PublicIPAddressCheckStatus.NameResolutionFailed, ConnectionState.Critical, ConnectionCheckReason.NameResolutionFailed)]
    public void GetPublicIPAddressResult(PublicIPAddressCheckStatus status, ConnectionState state,
        ConnectionCheckReason reason)
    {
        Assert.Equal(new ConnectionCheckResult(state, reason),
            NetworkConnectionCheck.GetPublicIPAddressResult(new PublicIPAddressCheckResult(status)));
    }

    private static PublicIPAddressCheckResult Api(PublicIPAddressCheckStatus? status) =>
        status == null ? null : new PublicIPAddressCheckResult(status.Value);

    [Theory]
    // API returned the public IP address
    [InlineData(true, PublicIPAddressCheckStatus.Success, InternetConnectivityLevel.InternetAccess, ConnectionState.OK, ConnectionCheckReason.None)]
    [InlineData(false, PublicIPAddressCheckStatus.Success, InternetConnectivityLevel.InternetAccess, ConnectionState.OK, ConnectionCheckReason.None)]
    // API disabled: ping decides
    [InlineData(true, null, InternetConnectivityLevel.InternetAccess, ConnectionState.OK, ConnectionCheckReason.None)]
    [InlineData(false, null, InternetConnectivityLevel.LocalAccess, ConnectionState.Critical, ConnectionCheckReason.InternetNotReachable)]
    // Captive portal (Windows), unless the API returned a public IP address (e.g. a web filter intercepts the probe
    // of Windows, the internet works)
    [InlineData(true, PublicIPAddressCheckStatus.Success, InternetConnectivityLevel.ConstrainedInternetAccess, ConnectionState.OK, ConnectionCheckReason.None)]
    [InlineData(true, null, InternetConnectivityLevel.ConstrainedInternetAccess, ConnectionState.Warning, ConnectionCheckReason.CaptivePortal)]
    [InlineData(false, null, InternetConnectivityLevel.ConstrainedInternetAccess, ConnectionState.Warning, ConnectionCheckReason.CaptivePortal)]
    [InlineData(false, PublicIPAddressCheckStatus.PrivateIPAddress, InternetConnectivityLevel.ConstrainedInternetAccess, ConnectionState.Warning, ConnectionCheckReason.CaptivePortal)]
    // Private IP address from the API (e.g. proxy)
    [InlineData(true, PublicIPAddressCheckStatus.PrivateIPAddress, InternetConnectivityLevel.InternetAccess, ConnectionState.Warning, ConnectionCheckReason.PublicIPAddressPrivate)]
    // Ping works, API blocked / DNS broken
    [InlineData(true, PublicIPAddressCheckStatus.RequestFailed, InternetConnectivityLevel.InternetAccess, ConnectionState.Warning, ConnectionCheckReason.PublicIPAddressRequestFailed)]
    [InlineData(true, PublicIPAddressCheckStatus.NameResolutionFailed, InternetConnectivityLevel.InternetAccess, ConnectionState.Warning, ConnectionCheckReason.NameResolutionFailed)]
    // Ping and API failed, but Windows reports internet access (ICMP and API blocked, e.g. proxy)
    [InlineData(false, PublicIPAddressCheckStatus.RequestFailed, InternetConnectivityLevel.InternetAccess, ConnectionState.Warning, ConnectionCheckReason.InternetAccessReportedByWindows)]
    [InlineData(false, null, InternetConnectivityLevel.InternetAccess, ConnectionState.Warning, ConnectionCheckReason.InternetAccessReportedByWindows)]
    // Everything failed
    [InlineData(false, PublicIPAddressCheckStatus.RequestFailed, InternetConnectivityLevel.LocalAccess, ConnectionState.Critical, ConnectionCheckReason.InternetNotReachable)]
    [InlineData(false, PublicIPAddressCheckStatus.InvalidResponse, InternetConnectivityLevel.Unknown, ConnectionState.Critical, ConnectionCheckReason.InternetNotReachable)]
    [InlineData(false, PublicIPAddressCheckStatus.NameResolutionFailed, InternetConnectivityLevel.None, ConnectionState.Critical, ConnectionCheckReason.NameResolutionFailed)]
    public void GetInternetResult_IPv4(bool isPingSuccessful, PublicIPAddressCheckStatus? apiStatus,
        InternetConnectivityLevel level, ConnectionState state, ConnectionCheckReason reason)
    {
        Assert.Equal(new ConnectionCheckResult(state, reason),
            NetworkConnectionCheck.GetInternetResult(AddressFamily.InterNetwork, isPingSuccessful, Api(apiStatus), level));
    }

    [Theory]
    [InlineData(true, ConnectionState.OK, ConnectionCheckReason.None)]
    [InlineData(false, ConnectionState.Critical, ConnectionCheckReason.InternetNotReachable)]
    public void GetInternetResult_IPv6_CaptivePortalOfWindowsIsNotUsed(bool isPingSuccessful, ConnectionState state,
        ConnectionCheckReason reason)
    {
        Assert.Equal(new ConnectionCheckResult(state, reason),
            NetworkConnectionCheck.GetInternetResult(AddressFamily.InterNetworkV6, isPingSuccessful, null,
                InternetConnectivityLevel.ConstrainedInternetAccess));
    }

    [Fact]
    public void GetInternetResult_IPv6_WindowsInternetAccessIsNotUsed()
    {
        // Windows doesn't report the connectivity per address family (internet access may be IPv4 only)
        Assert.Equal(new ConnectionCheckResult(ConnectionState.Critical, ConnectionCheckReason.InternetNotReachable),
            NetworkConnectionCheck.GetInternetResult(AddressFamily.InterNetworkV6, false,
                Api(PublicIPAddressCheckStatus.RequestFailed), InternetConnectivityLevel.InternetAccess));
    }
}
