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

            var subnet1 = values[2] as string;
            var subnet2 = values[3] as string;

            // Catch null exceptions...
            if (string.IsNullOrEmpty(subnet1) || string.IsNullOrEmpty(subnet2))
                return false;

            // Compare...
            var subnet1Data = subnet1.Split('/');
            var subnet2Data = subnet2.Split('/');

            var subnet1Ip = IPAddress.Parse(subnet1Data[0]);
            var subnet2Ip = IPAddress.Parse(subnet2Data[0]);

            if (subnet1Ip.AddressFamily == subnet2Ip.AddressFamily)
                return subnet1Data[1] == subnet2Data[1];

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
