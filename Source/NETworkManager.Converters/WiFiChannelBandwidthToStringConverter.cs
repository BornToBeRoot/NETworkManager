using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
///     Channel width of a WiFi network (<see cref="WiFiNetworkInfo" />) as text, e.g. "80 MHz", or "~80 MHz" if it was
///     estimated (see <see cref="WiFiNetworkInfo.IsChannelBandwidthEstimated" />).
/// </summary>
public sealed class WiFiChannelBandwidthToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not WiFiNetworkInfo info ? "-/-" : Format(info);
    }

    /// <summary>
    ///     Gets the channel width of a network as text (e.g. "80 MHz", "~80 MHz" if estimated).
    /// </summary>
    public static string Format(WiFiNetworkInfo info)
    {
        return $"{(info.IsChannelBandwidthEstimated ? "~" : string.Empty)}{info.ChannelBandwidth} MHz";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}