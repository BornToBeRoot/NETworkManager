using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
///     Convert a round-trip time in milliseconds (<see cref="long" /> or <see cref="double" />, e.g. the statistics of
///     the latency widget) to a string: <c>&lt;1 ms</c> if it rounds to 0, <c>-/-</c> without a value. See
///     <see cref="LatencyStatistics.FormatTime" />.
/// </summary>
public sealed class LatencyTimeToStringConverter : IValueConverter
{
    /// <summary>
    ///     Convert a round-trip time to a string.
    /// </summary>
    /// <param name="value">Round-trip time in milliseconds (<see cref="long" /> or <see cref="double" />) or <c>null</c>.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Formatted time, e.g. <c>&lt;1 ms</c>, <c>9 ms</c>, <c>-/-</c>.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return LatencyStatistics.FormatTime(value switch
        {
            long time => time,
            double time => time,
            _ => null
        });
    }

    /// <summary>
    ///     !!! Method not implemented !!!
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
