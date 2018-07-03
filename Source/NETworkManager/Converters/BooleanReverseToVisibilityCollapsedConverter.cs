using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class BooleanReverseToVisibilityCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool visible && visible ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
