using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NETworkManager.Views.Converters
{
    public sealed class BooleanReverseToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool && (bool)value) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
