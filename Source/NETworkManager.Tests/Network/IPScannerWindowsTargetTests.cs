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
    [InlineData("client01.other.example.com", "10.0.0.10")] // same computer name in another domain
    [InlineData("client01", "10.0.0.10")] // short name
    [InlineData("printer.corp.example.com", "10.0.0.10")] // stale PTR record of another computer
    [InlineData("", "10.0.0.10")]
    public void GetWindowsTarget(string dnsHostname, string expected)
    {
        Assert.Equal(expected, IPScanner.GetWindowsTarget(IPAddress, dnsHostname, Challenge, true));
    }

    [Fact]
    public void GetWindowsTarget_Workgroup_IPAddress()
    {
        // A computer in a workgroup has no DNS domain: its DNS name is the computer name only
        var workgroup = new NTLMChallengeInfo(new Version(10, 0, 26200), "PC01", "PC01", "PC01", "PC01", string.Empty,
            null);

        Assert.Equal("10.0.0.10", IPScanner.GetWindowsTarget(IPAddress, "pc01.fritz.box", workgroup, false));
    }

    [Fact]
    public void GetWindowsTarget_WithoutChallenge_IPAddress()
    {
        // The computer name is not known (e.g. NTLM disabled): the hostname can't be checked
        Assert.Equal("10.0.0.10", IPScanner.GetWindowsTarget(IPAddress, "client01.corp.example.com", null, true));
    }

    [Fact]
    public void GetWindowsTarget_NotGated_IPAddress()
    {
        // E.g. a NAS in a workgroup that sends its full DNS name: with the hostname, Kerberos would ask the domain
        // controller of the credentials for a ticket (a wrong password counts there) - not gated, so in parallel
        var nas = new NTLMChallengeInfo(null, "NAS", "NAS", "nas.home.lan", "home.lan", string.Empty, null);

        Assert.Equal("10.0.0.10", IPScanner.GetWindowsTarget(IPAddress, "nas.home.lan", nas, false));
        Assert.Equal("nas.home.lan", IPScanner.GetWindowsTarget(IPAddress, "nas.home.lan", nas, true));
    }
}
