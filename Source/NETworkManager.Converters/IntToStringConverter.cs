using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters;

public sealed class IntToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is int intValue ? intValue == 0 ? "-/-" : intValue.ToString() : "-/-";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}