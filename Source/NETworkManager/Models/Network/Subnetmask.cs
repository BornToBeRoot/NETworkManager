using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.Net;

namespace NETworkManager.Models.Network
{
    public static class Subnetmask
    {
        public static List<SubnetmaskInfo> List
        {
            get { return GenerateList(); }
        }

        private static List<SubnetmaskInfo> GenerateList()
        {
            List<SubnetmaskInfo> list = new List<SubnetmaskInfo>();

            for (int i = 2; i < 31; i++)
                list.Add(GetFromCidr(i));

            return list;
        }

        public static SubnetmaskInfo GetFromCidr(int cidr)
        {
            return new SubnetmaskInfo
            {
                CIDR = cidr,
                Subnetmask = ConvertCidrToSubnetmask(cidr)
            };
        }

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

        public static int ConvertSubnetmaskToCidr(string subnetmask)
        {
            return ConvertSubnetmaskToCidr(IPAddress.Parse(subnetmask));
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
