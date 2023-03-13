using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NETworkManager.Converters;

public sealed class HostUpStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool isReachable)
            return null;

        return isReachable ? Application.Current.FindResource("HostUpRectangle") : Application.Current.FindResource("HostDownRectangle");   
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
