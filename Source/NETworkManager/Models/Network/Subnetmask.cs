using NETworkManager.Utilities;
using System.Collections.Generic;
using System.Net;

namespace NETworkManager.Models.Network
{
    public static class Subnetmask
    {
        public static List<SubnetmaskInfo> List => GenerateList();

        private static List<SubnetmaskInfo> GenerateList()
        {
            var list = new List<SubnetmaskInfo>();

            for (var i = 2; i < 31; i++)
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
            var bits = string.Empty;

            for (var i = 0; i < cidr; i++)
                bits += "1";

            return IPv4AddressConverter.ToHumanString(bits.PadRight(32, '0'));
        }

        public static int ConvertSubnetmaskToCidr(IPAddress subnetmask)
        {
            return string.Join("", IPv4AddressConverter.ToBinaryString(subnetmask.ToString()).Replace(".", "").TrimEnd('0')).Length;
        }
    }
}
