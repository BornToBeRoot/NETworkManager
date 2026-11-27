using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
///     Convert <see cref="LatencyTargetInfo" /> to the host shown in the settings ("Gateway" for the default gateway).
/// </summary>
public sealed class LatencyTargetToHostConverter : IValueConverter
{
    /// <summary>
    ///     Convert <see cref="LatencyTargetInfo" /> to the host.
    /// </summary>
    /// <param name="value">Object from type <see cref="LatencyTargetInfo" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Host, "Gateway" for the default gateway.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not LatencyTargetInfo target
            ? "-/-"
            : target.IsGateway
                ? Strings.Gateway
                : target.Host;
    }

    /// <summary>
    ///     !!! Method not implemented !!!
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
