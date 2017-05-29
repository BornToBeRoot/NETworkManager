using NETworkManager.Helpers;
using System.Collections.Generic;

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
                Subnetmask = SubnetmaskHelper.ConvertCidrToSubnetmask(cidr)
            };
        }
    }
}
