using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
///     Convert a <see cref="WiFiNetworkInfo" /> to the SSID as it is shown ("Hidden Network" for a hidden network).
/// </summary>
public sealed class WiFiNetworkToSsidStringConverter : IValueConverter
{
    /// <summary>
    ///     Convert a network to its shown SSID.
    /// </summary>
    /// <param name="value">Network as <see cref="WiFiNetworkInfo" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Shown SSID, empty without a network.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is WiFiNetworkInfo network ? GetSsid(network) : string.Empty;
    }

    /// <summary>
    ///     Gets the SSID of a network as it is shown in the grid, the charts and the messages ("Hidden Network" for a
    ///     hidden network).
    /// </summary>
    /// <param name="network">Network.</param>
    /// <returns>Shown SSID.</returns>
    public static string GetSsid(WiFiNetworkInfo network)
    {
        return network.IsHidden ? Strings.HiddenNetwork : network.AvailableNetwork.Ssid;
    }

    /// <summary>
    ///     !!! Method not implemented !!!
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
