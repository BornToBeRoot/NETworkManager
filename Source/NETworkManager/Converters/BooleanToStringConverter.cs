using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class BooleanToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return Application.Current.Resources["String_Yes"] as string;

            return Application.Current.Resources["String_No"] as string;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
