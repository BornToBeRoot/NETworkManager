using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows.Data;
using Ping = NETworkManager.Models.Network.Ping;

namespace NETworkManager.Converters;

/// <summary>
///     Converts the status and the time of a ping to a string (e.g. "4 ms", "-/-" if it failed).
///     Values: [0] status (<see cref="IPStatus" />), [1] time (long), [2] optional: pending (bool) - "..." while the
///     ping is still running. Other values (e.g. <c>{DisconnectedItem}</c> of a recycled row, or
///     <see cref="System.Windows.DependencyProperty.UnsetValue" />) -> empty string.
/// </summary>
public sealed class PingTimeToStringConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length > 2 && values[2] is true)
            return "...";

        return values.Length > 1 && values[0] is IPStatus status && values[1] is long time
            ? Ping.TimeToString(status, time)
            : string.Empty;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}