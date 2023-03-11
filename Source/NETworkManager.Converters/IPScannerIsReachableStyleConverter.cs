using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NETworkManager.Converters;

public sealed class IPScannerIsReachableStyleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not bool isReachable)
            return null;

        return isReachable ? Application.Current.FindResource("CheckRectangle") : Application.Current.FindResource("ErrorRectangle");   
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
