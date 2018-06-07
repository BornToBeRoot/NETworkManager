using NETworkManager.Models.Network;
using System;
using System.Globalization;
using System.Net;
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

            string subnet = values[2] as string;
            string newSubnetmaskOrCIDR = values[3] as string;

            // Catch null exceptions...
            if (string.IsNullOrEmpty(subnet) || string.IsNullOrEmpty(newSubnetmaskOrCIDR))
                return false;

            // Get the cidr to compare...
            string[] subnetData = subnet.Split('/');

            IPAddress ipAddress = IPAddress.Parse(subnetData[0]);
            string subnetmaskOrCIDR = subnetData[1];
            int cidr;

            if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                if (subnetmaskOrCIDR.Length < 3)
                    cidr = int.Parse(subnetmaskOrCIDR);
                else
                    cidr = Subnetmask.ConvertSubnetmaskToCidr(IPAddress.Parse(subnetmaskOrCIDR));
            }
            else
            {
                cidr = int.Parse(subnetmaskOrCIDR);
            }

            newSubnetmaskOrCIDR = newSubnetmaskOrCIDR.TrimStart('/');
            int newCidr;

            if (ipAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                if (newSubnetmaskOrCIDR.Length < 3)
                    newCidr = int.Parse(newSubnetmaskOrCIDR);
                else
                    newCidr = Subnetmask.ConvertSubnetmaskToCidr(IPAddress.Parse(newSubnetmaskOrCIDR));
            }
            else
            {
                newCidr = int.Parse(newSubnetmaskOrCIDR);
            }

            // Compare
            return newCidr > cidr;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
