using NETworkManager.Models.Network;
using Windows.Networking.Connectivity;
using Xunit;

namespace NETworkManager.Tests.Network;

public class WiFiConnectModeTests
{
    [Theory]
    [InlineData(NetworkAuthenticationType.Open80211, NetworkEncryptionType.None, WiFiConnectMode.Open)]
    [InlineData(NetworkAuthenticationType.Owe, NetworkEncryptionType.Ccmp, WiFiConnectMode.Open)]
    [InlineData(NetworkAuthenticationType.RsnaPsk, NetworkEncryptionType.None, WiFiConnectMode.Open)]
    [InlineData(NetworkAuthenticationType.SharedKey80211, NetworkEncryptionType.Wep, WiFiConnectMode.Psk)]
    [InlineData(NetworkAuthenticationType.WpaPsk, NetworkEncryptionType.Tkip, WiFiConnectMode.Psk)]
    [InlineData(NetworkAuthenticationType.RsnaPsk, NetworkEncryptionType.Ccmp, WiFiConnectMode.Psk)]
    [InlineData(NetworkAuthenticationType.Wpa3Sae, NetworkEncryptionType.Ccmp, WiFiConnectMode.Psk)]
    [InlineData(NetworkAuthenticationType.Wpa, NetworkEncryptionType.Tkip, WiFiConnectMode.Eap)]
    [InlineData(NetworkAuthenticationType.Rsna, NetworkEncryptionType.Ccmp, WiFiConnectMode.Eap)]
    [InlineData(NetworkAuthenticationType.Wpa3Enterprise, NetworkEncryptionType.Ccmp, WiFiConnectMode.Eap)]
    [InlineData(NetworkAuthenticationType.Wpa3Enterprise192Bits, NetworkEncryptionType.Ccmp, WiFiConnectMode.Eap)]
    public void GetConnectMode_ReturnsTheDialogMode(NetworkAuthenticationType authenticationType,
        NetworkEncryptionType encryptionType, WiFiConnectMode expected)
    {
        Assert.Equal(expected, WiFi.GetConnectMode(authenticationType, encryptionType));
    }
}
