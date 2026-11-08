using System.Linq;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class WiFiScanComparerTests
{
    private static WiFiNetworkInfo Network(string bssid, int channel = 36, int width = 80,
        string security = "WPA2 Personal (PSK)", bool isEstimated = false)
    {
        return new WiFiNetworkInfo
        {
            Bssid = bssid,
            Channel = channel,
            ChannelBandwidth = width,
            IsChannelBandwidthEstimated = isEstimated,
            NetworkAuthenticationType = security
        };
    }

    [Fact]
    public void SameNetworks_HaveNoChange()
    {
        var result = WiFiScanComparer.Compare([Network("AA"), Network("BB")], [Network("BB"), Network("AA")]);

        Assert.Empty(result.Changes);
        Assert.Empty(result.Gone);
    }

    [Fact]
    public void SignalStrength_IsNoChange()
    {
        // Only the BSSID, channel, width and security are compared - another object with the same values
        var result = WiFiScanComparer.Compare([Network("AA")], [Network("AA")]);

        Assert.Empty(result.Changes);
    }

    [Fact]
    public void NetworkVisibleNow_IsNew()
    {
        var current = Network("BB");

        var result = WiFiScanComparer.Compare([Network("AA")], [Network("AA"), current]);

        var change = Assert.Single(result.Changes);
        Assert.Same(current, change.Network);
        Assert.Equal(WiFiNetworkChange.New, change.Change);
        Assert.Null(change.Previous);
    }

    [Fact]
    public void NetworkNotVisibleNow_IsGone()
    {
        var previous = Network("BB");

        var result = WiFiScanComparer.Compare([Network("AA"), previous], [Network("AA")]);

        Assert.Same(previous, Assert.Single(result.Gone));
        Assert.Empty(result.Changes);
    }

    [Theory]
    [InlineData(40, 80, "WPA2 Personal (PSK)")] // Channel
    [InlineData(36, 160, "WPA2 Personal (PSK)")] // Channel width
    [InlineData(36, 80, "WPA3 Personal (SAE)")] // Security
    public void ChannelWidthOrSecurity_IsChanged(int channel, int width, string security)
    {
        var previous = Network("AA");

        var result = WiFiScanComparer.Compare([previous], [Network("AA", channel, width, security)]);

        var change = Assert.Single(result.Changes);
        Assert.Equal(WiFiNetworkChange.Changed, change.Change);
        Assert.Same(previous, change.Previous);
    }

    [Fact]
    public void EstimatedWidth_IsNotCompared()
    {
        var result = WiFiScanComparer.Compare([Network("AA", width: 80)],
            [Network("AA", width: 20, isEstimated: true)]);

        Assert.Empty(result.Changes);
    }

    [Fact]
    public void Bssid_IsComparedCaseInsensitive()
    {
        var result = WiFiScanComparer.Compare([Network("aa:bb")], [Network("AA:BB")]);

        Assert.Empty(result.Changes);
        Assert.Empty(result.Gone);
    }

    [Fact]
    public void DuplicateBssid_UsesTheFirst()
    {
        var result = WiFiScanComparer.Compare([Network("AA"), Network("AA", 40)], [Network("AA")]);

        Assert.Empty(result.Changes);
    }

    [Fact]
    public void Gone_KeepsTheOrderOfThePreviousScan()
    {
        var result = WiFiScanComparer.Compare([Network("CC"), Network("AA"), Network("BB")], []);

        Assert.Equal(["CC", "AA", "BB"], result.Gone.Select(x => x.Bssid));
    }
}
