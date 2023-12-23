using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows.Data;
using Ping = NETworkManager.Models.Network.Ping;

namespace NETworkManager.Converters;

public sealed class PingTimeToStringConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return Ping.TimeToString((IPStatus)values[0], (long)values[1]);
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}