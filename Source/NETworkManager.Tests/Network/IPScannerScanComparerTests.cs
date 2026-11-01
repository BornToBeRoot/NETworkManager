using System.Collections.Generic;
using System.Linq;
using System.Net;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class IPScannerScanComparerTests
{
    private static readonly IPAddress A = IPAddress.Parse("10.0.0.1");
    private static readonly IPAddress B = IPAddress.Parse("10.0.0.2");
    private static readonly IPAddress C = IPAddress.Parse("10.0.0.3");
    private static readonly IPAddress V6 = IPAddress.Parse("fe80::1");

    private static IPScannerBaselineHost Up(string mac = "") => new(true, "host", mac, "vendor");
    private static IPScannerBaselineHost Down() => new(false, string.Empty, string.Empty, string.Empty);

    private static Dictionary<IPAddress, IPScannerBaselineHost> Scan(params (IPAddress, IPScannerBaselineHost)[] hosts) =>
        hosts.ToDictionary(x => x.Item1, x => x.Item2);

    [Fact]
    public void SameHostsHaveNoChange()
    {
        var result = IPScannerScanComparer.Compare(Scan((A, Up("00:11")), (B, Up())),
            Scan((A, Up("00:11")), (B, Up())));

        Assert.Empty(result.Changes);
        Assert.Empty(result.Gone);
    }

    [Fact]
    public void HostUpNowIsNew()
    {
        var result = IPScannerScanComparer.Compare(Scan((A, Up()), (B, Down())), Scan((A, Up()), (B, Up()), (C, Up())));

        Assert.Equal(IPScannerHostChange.New, result.Changes[B].Change);
        Assert.Equal(IPScannerHostChange.New, result.Changes[C].Change);
        Assert.False(result.Changes.ContainsKey(A));
    }

    [Fact]
    public void HostNotUpNowIsGone()
    {
        var previous = Up("00:11");
        var result = IPScannerScanComparer.Compare(Scan((A, previous), (B, Up()), (V6, Up())),
            Scan((B, Down())));

        Assert.Equal([A, B, V6], result.Gone.Select(x => x.IPAddress));
        Assert.Same(previous, result.Gone[0].Previous);
    }

    [Fact]
    public void HostDownBeforeAndNowIsNoChange()
    {
        var result = IPScannerScanComparer.Compare(Scan((A, Down())), Scan((A, Down())));

        Assert.Empty(result.Changes);
        Assert.Empty(result.Gone);
    }

    [Fact]
    public void OtherMACAddressIsMACChanged()
    {
        var result = IPScannerScanComparer.Compare(Scan((A, Up("00:11:22:33:44:55"))),
            Scan((A, Up("66:77:88:99:AA:BB"))));

        Assert.Equal(IPScannerHostChange.MACChanged, result.Changes[A].Change);
        Assert.Equal("00:11:22:33:44:55", result.Changes[A].PreviousMACAddress);
    }

    [Theory]
    [InlineData("", "00:11:22:33:44:55")]
    [InlineData("00:11:22:33:44:55", "")]
    [InlineData("00:11:22:33:44:aa", "00:11:22:33:44:AA")]
    public void UnknownOrSameMACAddressIsNoChange(string previousMAC, string currentMAC)
    {
        var result = IPScannerScanComparer.Compare(Scan((A, Up(previousMAC))), Scan((A, Up(currentMAC))));

        Assert.Empty(result.Changes);
    }

    [Fact]
    public void EmptyBaselineMarksAllUpHostsAsNew()
    {
        var result = IPScannerScanComparer.Compare(Scan(), Scan((A, Up()), (B, Down())));

        Assert.Single(result.Changes);
        Assert.Equal(IPScannerHostChange.New, result.Changes[A].Change);
    }

    [Fact]
    public void BaselineSkipsGoneRowsOfTheLastComparison()
    {
        var gone = IPScannerHostInfo.CreateGone(A, Up("00:11"));
        var up = new IPScannerHostInfo(B, true, new PingInfo(), false, [], new NetBIOSInfo(B));

        var baseline = IPScannerScanComparer.CreateBaseline([gone, up]);

        Assert.Equal([B], baseline.Keys);
        Assert.True(baseline[B].IsUp);
    }

    [Fact]
    public void GoneRowHasThePreviousValues()
    {
        var gone = IPScannerHostInfo.CreateGone(A, new IPScannerBaselineHost(true, "nas.local", "00:11", "Synology"));

        Assert.False(gone.IsReachable);
        Assert.Equal(IPScannerHostChange.Gone, gone.Change);
        Assert.Equal("nas.local", gone.Hostname);
        Assert.Equal("00:11", gone.MACAddress);
        Assert.False(gone.IsHostnamePending);
        Assert.False(gone.IsMACAddressPending);
    }
}
