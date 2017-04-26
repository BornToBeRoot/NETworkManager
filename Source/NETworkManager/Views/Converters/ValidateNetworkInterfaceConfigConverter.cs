using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Views.Converters
{
    public sealed class ValidateNetworkInterfaceConfigConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)values[0] || (bool)values[1] && !(bool)values[2] && !(bool)values[3] && !(bool)values[4]) && ((bool)values[5] || (bool)values[6] && !(bool)values[7] && !(bool)values[8]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
