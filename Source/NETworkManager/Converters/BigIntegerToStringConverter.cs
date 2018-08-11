using System;
using System.Globalization;
using System.Numerics;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class BigIntegerToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? ((BigInteger) value).ToString() : "-/-";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
