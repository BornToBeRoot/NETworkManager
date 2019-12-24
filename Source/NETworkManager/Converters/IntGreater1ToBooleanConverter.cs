using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class IntGreater1ToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var x = value is int count && count > 1;
            Debug.WriteLine(x);
            return x;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
