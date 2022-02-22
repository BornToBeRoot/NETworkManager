using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using System;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class ValidateSubnetCalculatorSubnettingConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            // if Validation.HasError is true...
            if ((bool)values[0] || (bool)values[1])
                return false;

            var subnet = values[2] as string;
            var newSubnetmaskOrCidr = values[3] as string;

            // Catch null exceptions...
            if (string.IsNullOrEmpty(subnet) || string.IsNullOrEmpty(newSubnetmaskOrCidr))
                return false;

            // Get the cidr to compare...
            var subnetData = subnet.Split('/');

            var ipAddress = IPAddress.Parse(subnetData[0]);
            var subnetmaskOrCidr = subnetData[1];
            int cidr;

            switch (ipAddress.AddressFamily)
            {
                case System.Net.Sockets.AddressFamily.InterNetwork when subnetmaskOrCidr.Length < 3:
                    cidr = int.Parse(subnetmaskOrCidr);
                    break;
                case System.Net.Sockets.AddressFamily.InterNetwork:
                    cidr = Subnetmask.ConvertSubnetmaskToCidr(IPAddress.Parse(subnetmaskOrCidr));
                    break;
                default:
                    cidr = int.Parse(subnetmaskOrCidr);
                    break;
            }

            int newCidr;

            // Support subnetmask like 255.255.255.0
            if (Regex.IsMatch(newSubnetmaskOrCidr, RegexHelper.SubnetmaskRegex))
                newCidr = System.Convert.ToByte(Subnetmask.ConvertSubnetmaskToCidr(IPAddress.Parse(newSubnetmaskOrCidr)));
            else
                newCidr = System.Convert.ToByte(newSubnetmaskOrCidr.TrimStart('/'));

            // Compare
            return newCidr > cidr;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
