using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows.Data;
using Ping = NETworkManager.Models.Network.Ping;

namespace NETworkManager.Converters;

/// <summary>
///     Converts the status and the time of a ping to a string (e.g. "4 ms", "-/-" if it failed).
///     Values: [0] status (<see cref="IPStatus" />), [1] time (long), [2] optional: pending (bool) - "..." while the
///     ping is still running.
/// </summary>
public sealed class PingTimeToStringConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length > 2 && values[2] is true)
            return "...";

        return Ping.TimeToString((IPStatus)values[0], (long)values[1]);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}