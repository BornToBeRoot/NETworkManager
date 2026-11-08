using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
///     Convert the <see cref="WiFiNetworkChange" /> of a network (since the previous scan) to a translated
///     <see cref="string" /> (e.g. "New"), or a <see cref="WiFiNetworkInfo" /> to the tooltip of a changed network.
/// </summary>
public sealed class WiFiNetworkChangeToStringConverter : IValueConverter
{
    /// <summary>
    ///     Convert the change of a network to a translated <see cref="string" />.
    /// </summary>
    /// <param name="value">
    ///     Change of the network as <see cref="WiFiNetworkChange" />, or the network as <see cref="WiFiNetworkInfo" /> with
    ///     the parameter "ToolTip".
    /// </param>
    /// <param name="targetType"></param>
    /// <param name="parameter">"ToolTip" for the values of the previous scan of a changed network.</param>
    /// <param name="culture"></param>
    /// <returns>Translated change (empty if there is none), or the tooltip (null without previous values).</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is "ToolTip")
            return value is WiFiNetworkInfo { Previous: { } previous }
                ? string.Format(Strings.PreviouslyX,
                    $"{Strings.Channel} {previous.Channel} · {WiFiChannelBandwidthToStringConverter.Format(previous)} · {previous.NetworkAuthenticationType}")
                : null;

        return value is WiFiNetworkChange change ? ToLocalizedString(change) : string.Empty;
    }

    /// <summary>
    ///     Gets the translated change of a network.
    /// </summary>
    /// <param name="change">Change of the network.</param>
    /// <returns>Translated change, empty for <see cref="WiFiNetworkChange.None" />.</returns>
    public static string ToLocalizedString(WiFiNetworkChange change)
    {
        return change switch
        {
            WiFiNetworkChange.New => Strings.HostChange_New,
            WiFiNetworkChange.Gone => Strings.HostChange_Gone,
            WiFiNetworkChange.Changed => Strings.WiFiNetworkChange_Changed,
            _ => string.Empty
        };
    }

    /// <summary>
    ///     !!! Method not implemented !!!
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
