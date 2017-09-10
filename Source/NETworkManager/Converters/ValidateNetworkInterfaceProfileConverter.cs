using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class ValidateNetworkInterfaceProfileConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // Validate configuration
            if (values.Length == 9)
                return ((bool)values[0] || (bool)values[1] && !(bool)values[2] && !(bool)values[3] && !(bool)values[4]) && ((bool)values[5] || (bool)values[6] && !(bool)values[7] && !(bool)values[8]);
            else // Validate configuration and if the interface is up
                return (((bool)values[0] || (bool)values[1] && !(bool)values[2] && !(bool)values[3] && !(bool)values[4]) && ((bool)values[5] || (bool)values[6] && !(bool)values[7] && !(bool)values[8]) && (bool)values[9]);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
