using System;
using System.Net;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

/// <summary>
///     Name the NetApi requests of the Windows host information connect to: the DNS hostname if it belongs to the host
///     (Kerberos), otherwise the IP address.
/// </summary>
public class IPScannerWindowsTargetTests
{
    private static readonly IPAddress IPAddress = IPAddress.Parse("10.0.0.10");

    private static readonly NTLMChallengeInfo Challenge = new(new Version(10, 0, 26200), "CLIENT01", "CORP",
        "client01.corp.example.com", "corp.example.com", "example.com", null);

    [Theory]
    [InlineData("client01.corp.example.com", "client01.corp.example.com")]
    [InlineData("CLIENT01.corp.example.com.", "CLIENT01.corp.example.com")] // case, trailing dot
    [InlineData("client01.other.example.com", "client01.other.example.com")] // other suffix, same computer name
    [InlineData("printer.corp.example.com", "10.0.0.10")] // stale PTR record of another computer
    [InlineData("", "10.0.0.10")]
    public void GetWindowsTarget(string dnsHostname, string expected)
    {
        Assert.Equal(expected, IPScanner.GetWindowsTarget(IPAddress, dnsHostname, Challenge));
    }

    [Fact]
    public void GetWindowsTarget_WithoutChallenge_IPAddress()
    {
        // The computer name is not known (e.g. NTLM disabled): the hostname can't be checked
        Assert.Equal("10.0.0.10", IPScanner.GetWindowsTarget(IPAddress, "client01.corp.example.com", null));
    }
}
