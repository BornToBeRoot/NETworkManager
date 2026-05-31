using NETworkManager.Models.Network;

namespace NETworkManager.Controls;

/// <summary>
///     A single data point of a Wi-Fi channel chart series. The X coordinate is expressed in
///     channel-number space (which is linear with the channel frequency), the Y coordinate is the
///     signal strength in dBm. Each point carries a reference to the originating network so the
///     chart tooltip can display its details.
/// </summary>
/// <param name="channelAxis">X coordinate in channel-number space.</param>
/// <param name="dbm">Signal strength in dBm (Y coordinate).</param>
/// <param name="network">The Wi-Fi network this point belongs to.</param>
public class WiFiChannelPoint(double channelAxis, double dbm, WiFiNetworkInfo network)
{
    public double ChannelAxis { get; } = channelAxis;

    public double Dbm { get; } = dbm;

    public WiFiNetworkInfo Network { get; } = network;
}
