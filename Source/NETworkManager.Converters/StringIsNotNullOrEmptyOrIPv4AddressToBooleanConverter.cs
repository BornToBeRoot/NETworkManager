using NETworkManager.Utilities;
using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters;

public sealed class StringIsNotNullOrEmptyOrIPv4AddressToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return !string.IsNullOrEmpty(value as string) && !RegexHelper.IPv4AddressRegex().IsMatch((string)value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}