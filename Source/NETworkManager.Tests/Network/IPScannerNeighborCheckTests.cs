using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class IPScannerNeighborCheckTests
{
    private const string HostMAC = "001122AABBCC";
    private const string GatewayMAC = "00AABBCCDDEE";

    private static readonly string[] GatewayMACs = [GatewayMAC];

    private static NeighborInfo CreateEntry(NeighborState state, string macAddress = HostMAC)
    {
        return new NeighborInfo(IPAddress.Parse("192.168.1.10"), PhysicalAddress.Parse(macAddress), false, 12,
            "Ethernet", state, AddressFamily.InterNetwork);
    }

    [Fact]
    public void Check_NoEntry_IsUnreachable()
    {
        Assert.Equal(IPScannerNeighborCheckResult.Unreachable, IPScannerNeighborCheck.Check(null, false, GatewayMACs));
    }

    [Theory]
    [InlineData(NeighborState.Reachable, IPScannerNeighborCheckResult.Reachable)]
    [InlineData(NeighborState.Stale, IPScannerNeighborCheckResult.Verify)]
    [InlineData(NeighborState.Delay, IPScannerNeighborCheckResult.Verify)]
    [InlineData(NeighborState.Probe, IPScannerNeighborCheckResult.Verify)]
    [InlineData(NeighborState.Permanent, IPScannerNeighborCheckResult.Unreachable)]
    [InlineData(NeighborState.Incomplete, IPScannerNeighborCheckResult.Unreachable)]
    [InlineData(NeighborState.Unreachable, IPScannerNeighborCheckResult.Unreachable)]
    public void Check_State(NeighborState state, IPScannerNeighborCheckResult expected)
    {
        Assert.Equal(expected, IPScannerNeighborCheck.Check(CreateEntry(state), false, GatewayMACs));
    }

    [Theory]
    [InlineData(NeighborState.Reachable)]
    [InlineData(NeighborState.Stale)]
    public void Check_MACOfGateway_IsProxyARP(NeighborState state)
    {
        Assert.Equal(IPScannerNeighborCheckResult.Unreachable,
            IPScannerNeighborCheck.Check(CreateEntry(state, GatewayMAC), false, GatewayMACs));
    }

    [Fact]
    public void Check_GatewayItself_IsReachable()
    {
        Assert.Equal(IPScannerNeighborCheckResult.Reachable,
            IPScannerNeighborCheck.Check(CreateEntry(NeighborState.Reachable, GatewayMAC), true, GatewayMACs));
    }

    [Fact]
    public void Check_NoGateway_IsReachable()
    {
        Assert.Equal(IPScannerNeighborCheckResult.Reachable,
            IPScannerNeighborCheck.Check(CreateEntry(NeighborState.Reachable), false, []));
    }

    [Theory]
    [InlineData("00AABBCCDDEE", true)]
    [InlineData("00aabbccddee", true)]
    [InlineData("001122AABBCC", false)]
    public void IsProxyARPAnswer(string macAddress, bool expected)
    {
        Assert.Equal(expected, IPScannerNeighborCheck.IsProxyARPAnswer(macAddress, GatewayMACs));
    }

    [Theory]
    [InlineData(0, NeighborState.Unreachable)]
    [InlineData(4, NeighborState.Stale)]
    [InlineData(5, NeighborState.Reachable)]
    [InlineData(6, NeighborState.Permanent)]
    public void NeighborState_MatchesWindowsValues(uint value, NeighborState expected)
    {
        // NL_NEIGHBOR_STATE (MIB_IPNET_ROW2.State) starts at 0 (NlnsUnreachable)
        Assert.Equal(expected, (NeighborState)value);
    }
}
