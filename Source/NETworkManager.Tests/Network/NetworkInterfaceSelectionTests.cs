using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class NetworkInterfaceSelectionTests
{
    private static readonly NetworkInterfaceInfo Ethernet = new() { Id = "{A}", Name = "Ethernet" };
    private static readonly NetworkInterfaceInfo WiFi = new() { Id = "{B}", Name = "Wi-Fi" };
    private static readonly NetworkInterfaceInfo Vpn = new() { Id = "{C}", Name = "VPN" };

    private static readonly NetworkInterfaceInfo[] All = [Ethernet, WiFi, Vpn];

    [Theory]
    [InlineData("{B}", "Wi-Fi")]
    [InlineData("{X}", null)]
    [InlineData("", null)]
    [InlineData(null, null)]
    public void FindById(string id, string expectedName)
    {
        Assert.Equal(expectedName, NetworkInterface.FindById(All, id)?.Name);
    }

    [Fact]
    public void GetNetworkInterfaceToSelect_RememberedFirst()
    {
        Assert.Same(Vpn, NetworkInterface.GetNetworkInterfaceToSelect(All, "{C}", "{B}"));
    }

    [Fact]
    public void GetNetworkInterfaceToSelect_RememberedMissing_PreviousSelection()
    {
        // e.g. the remembered VPN adapter is disconnected: the fallback selected before stays selected
        Assert.Same(WiFi, NetworkInterface.GetNetworkInterfaceToSelect([Ethernet, WiFi], "{C}", "{B}"));
    }

    [Fact]
    public void GetNetworkInterfaceToSelect_RememberedIsBack()
    {
        // The remembered adapter replaces the fallback when it is back (the fallback is not remembered)
        Assert.Same(Vpn, NetworkInterface.GetNetworkInterfaceToSelect(All, "{C}", "{A}"));
    }

    [Theory]
    [InlineData("{X}", "{Y}")]
    [InlineData(null, null)]
    public void GetNetworkInterfaceToSelect_NothingKnown_First(string rememberedId, string previousId)
    {
        Assert.Same(Ethernet, NetworkInterface.GetNetworkInterfaceToSelect(All, rememberedId, previousId));
    }

    [Fact]
    public void GetNetworkInterfaceToSelect_Empty()
    {
        Assert.Null(NetworkInterface.GetNetworkInterfaceToSelect([], "{A}", "{A}"));
    }
}
