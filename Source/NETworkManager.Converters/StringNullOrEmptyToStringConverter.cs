using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters;

public sealed class StringNullOrEmptyToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is string stringValue && !string.IsNullOrEmpty(stringValue) ? stringValue : "-/-";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}