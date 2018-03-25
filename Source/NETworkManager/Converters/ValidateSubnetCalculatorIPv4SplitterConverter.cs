using NETworkManager.Models.Network;
using System;
using System.Globalization;
using System.Net;
using System.Windows.Data;

namespace NETworkManager.Converters
{
    public sealed class ValidateSubnetCalculatorIPv4SplitterConverter : IMultiValueConverter
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
            string subnetmaskOrCIDR = subnet.Split('/')[1];
            int cidr;

            if (subnetmaskOrCIDR.Length < 3)
                cidr = int.Parse(subnetmaskOrCIDR);
            else
                cidr = Subnetmask.ConvertSubnetmaskToCidr(IPAddress.Parse(subnetmaskOrCIDR));
            
            newSubnetmaskOrCIDR = newSubnetmaskOrCIDR.TrimStart('/');
            int newCidr;

            if (newSubnetmaskOrCIDR.Length < 3)
                newCidr = int.Parse(newSubnetmaskOrCIDR);
            else
                newCidr = Subnetmask.ConvertSubnetmaskToCidr(IPAddress.Parse(newSubnetmaskOrCIDR));

            // Compare
            return newCidr > cidr;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
