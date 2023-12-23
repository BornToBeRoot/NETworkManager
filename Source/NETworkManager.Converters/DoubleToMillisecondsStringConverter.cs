using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters;

public sealed class DoubleToMillisecondsStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is double milliseconds ? $"{milliseconds} ms" : "-/-";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}