using System.Collections.Generic;
using System.Linq;
using System.Net;
using NETworkManager.Models.Lookup;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class PortScannerScanComparerTests
{
    private static readonly IPAddress A = IPAddress.Parse("10.0.0.1");
    private static readonly IPAddress B = IPAddress.Parse("10.0.0.2");

    private static PortScannerPortInfo Port(IPAddress ipAddress, int port, PortState state, string hostname = "host") =>
        new(ipAddress, hostname, port, new PortLookupInfo(port, TransportProtocol.Tcp), state);

    [Fact]
    public void BaselineHasOnlyOpenPorts()
    {
        var baseline = PortScannerScanComparer.CreateBaseline([
            Port(A, 22, PortState.Open, "server"), Port(A, 80, PortState.Closed), Port(B, 443, PortState.TimedOut)
        ]);

        Assert.Equal([(A, 22)], baseline.Keys);
        Assert.Equal("server", baseline[(A, 22)]);
    }

    [Fact]
    public void SameOpenPortsHaveNoChange()
    {
        var baseline = PortScannerScanComparer.CreateBaseline([Port(A, 22, PortState.Open)]);

        var result = PortScannerScanComparer.Compare(baseline, [Port(A, 22, PortState.Open), Port(A, 80, PortState.Closed)]);

        Assert.Empty(result.Opened);
        Assert.Empty(result.Closed);
        Assert.Empty(result.Missing);
    }

    [Fact]
    public void PortOpenNowIsOpened()
    {
        var baseline = PortScannerScanComparer.CreateBaseline([Port(A, 22, PortState.Open)]);

        var result = PortScannerScanComparer.Compare(baseline, [Port(A, 22, PortState.Open), Port(A, 8000, PortState.Open)]);

        Assert.Equal([(A, 8000)], result.Opened);
    }

    [Theory]
    [InlineData(PortState.Closed)]
    [InlineData(PortState.TimedOut)]
    public void PortNotOpenNowIsClosed(PortState state)
    {
        var baseline = PortScannerScanComparer.CreateBaseline([Port(A, 3389, PortState.Open)]);

        var result = PortScannerScanComparer.Compare(baseline, [Port(A, 3389, state)]);

        Assert.Equal([(A, 3389)], result.Closed);
        Assert.Empty(result.Missing);
    }

    [Fact]
    public void PortNotInTheResultIsMissing()
    {
        var baseline = PortScannerScanComparer.CreateBaseline([Port(B, 443, PortState.Open, "web")]);

        var result = PortScannerScanComparer.Compare(baseline, [Port(A, 22, PortState.Open)]);

        Assert.Equal([(B, 443, "web")], result.Missing);
        Assert.Equal([(A, 22)], result.Opened);
    }
}
