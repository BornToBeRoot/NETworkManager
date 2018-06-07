using System;
using System.Globalization;
using System.Net;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class ValidateSubnetCalculatorSupernettingConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // if Validation.HasError is true...
            if ((bool)values[0] || (bool)values[1])
                return false;

            string subnet1 = values[2] as string;
            string subnet2 = values[3] as string;

            // Catch null exceptions...
            if (string.IsNullOrEmpty(subnet1) || string.IsNullOrEmpty(subnet2))
                return false;

            // Compare...
            string[] subnet1data = subnet1.Split('/');
            string[] subnet2data = subnet2.Split('/');

            IPAddress subnet1IP = IPAddress.Parse(subnet1data[0]);
            IPAddress subnet2IP = IPAddress.Parse(subnet2data[0]);

            if (subnet1IP.AddressFamily == subnet2IP.AddressFamily)
                return subnet1data[1] == subnet2data[1];

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
