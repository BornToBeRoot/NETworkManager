using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Views.Converters
{
    public sealed class IntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long test;

            long.TryParse(value.ToString(), out test);

            if (test == 0)
                return string.Format("-/-");

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
