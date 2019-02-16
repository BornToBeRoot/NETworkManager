using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows.Data;
using NETworkManager.Utilities;

namespace NETworkManager.Converters
{
    public sealed class StringIsNotNullOrEmptyOrIPv4AddressToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrEmpty(value as string) && !Regex.IsMatch((string) value, RegexHelper.IPv4AddressRegex);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
