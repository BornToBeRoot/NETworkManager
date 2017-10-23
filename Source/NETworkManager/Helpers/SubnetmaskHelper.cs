using System;
using System.Net;

namespace NETworkManager.Helpers
{
    public static class SubnetmaskHelper
    {
        public static string ConvertCidrToSubnetmask(int cidr)
        {
            string bits = string.Empty;

            for (int i = 0; i < cidr; i++)
                bits += "1";

            return IPv4AddressHelper.BinaryStringToHumanString(bits.PadRight(32, '0'));
        }

        public static int ConvertSubnetmaskToCidr(IPAddress subnetmask)
        {
            return string.Join("", IPv4AddressHelper.HumanStringToBinaryString(subnetmask.ToString()).Replace(".", "").TrimEnd('0')).Length;
        }

        public static long GetNumberIPv4Addresses(int cidr)
        {
            return Convert.ToInt64(Math.Pow(2, 32 - cidr));
        }

        public static Int64 GetNumberIPv4Addresses(IPAddress subnetmask)
        {
            return GetNumberIPv4Addresses(ConvertSubnetmaskToCidr(subnetmask));
        }
    }
}