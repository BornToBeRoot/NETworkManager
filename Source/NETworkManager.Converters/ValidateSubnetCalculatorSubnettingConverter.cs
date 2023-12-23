using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows.Data;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;

namespace NETworkManager.Converters;

public sealed class ValidateSubnetCalculatorSubnettingConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        // if Validation.HasError is true...
        if ((bool)values[0] || (bool)values[1])
            return false;

        var subnet = (values[2] as string)?.Trim();
        var newSubnetmaskOrCidr = (values[3] as string)?.Trim();

        // Catch null exceptions...
        if (string.IsNullOrEmpty(subnet) || string.IsNullOrEmpty(newSubnetmaskOrCidr))
            return false;

        // Get the cidr to compare...
        var subnetData = subnet.Split('/');

        var ipAddress = IPAddress.Parse(subnetData[0]);
        var subnetmaskOrCidr = subnetData[1];

        var cidr = ipAddress.AddressFamily switch
        {
            AddressFamily.InterNetwork when subnetmaskOrCidr.Length < 3 => int.Parse(
                subnetmaskOrCidr),
            AddressFamily.InterNetwork => Subnetmask.ConvertSubnetmaskToCidr(
                IPAddress.Parse(subnetmaskOrCidr)),
            _ => int.Parse(subnetmaskOrCidr)
        };

        // Support subnetmask like 255.255.255.0
        int newCidr = Regex.IsMatch(newSubnetmaskOrCidr, RegexHelper.SubnetmaskRegex)
            ? System.Convert.ToByte(Subnetmask.ConvertSubnetmaskToCidr(IPAddress.Parse(newSubnetmaskOrCidr)))
            : System.Convert.ToByte(newSubnetmaskOrCidr.TrimStart('/'));

        // Compare
        return newCidr > cidr;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}