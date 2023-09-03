using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Windows.Data;

namespace NETworkManager.Converters;

public sealed class OperationalStatusToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not OperationalStatus operationalStatus ? "-/-" : operationalStatus.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
