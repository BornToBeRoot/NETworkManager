using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

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
        var bits = new string('1', cidr);

        return IPv4Address.ToHumanString(bits.PadRight(32, '0'));
    }

    public static int ConvertSubnetmaskToCidr(IPAddress subnetmask)
    {
        return string.Join("", IPv4Address.ToBinaryString(subnetmask.ToString()).Replace(".", "").TrimEnd('0')).Length;
    }

    /// <summary>
    ///     Gets the CIDRs a subnet can be split into (Subnetting): from the CIDR of the subnet to the maximum of the
    ///     address family (32 for IPv4, 128 for IPv6).
    /// </summary>
    /// <param name="network">Subnet to split.</param>
    /// <returns>Minimum and maximum CIDR.</returns>
    public static (int Min, int Max) GetSubnettingCidrRange(IPNetwork2 network)
    {
        return (network.Cidr, network.AddressFamily == AddressFamily.InterNetwork ? 32 : 128);
    }

    /// <summary>
    ///     Converts a subnet mask (e.g. 255.255.255.0) or a CIDR (e.g. /24 or 24) to the CIDR. The value must be valid
    ///     (see IPv4IPv6SubnetmaskOrCIDRValidator).
    /// </summary>
    /// <param name="subnetmaskOrCidr">Subnet mask or CIDR.</param>
    /// <returns>CIDR.</returns>
    public static int ConvertSubnetmaskOrCidrToCidr(string subnetmaskOrCidr)
    {
        subnetmaskOrCidr = subnetmaskOrCidr.Trim();

        return RegexHelper.SubnetmaskRegex().IsMatch(subnetmaskOrCidr)
            ? ConvertSubnetmaskToCidr(IPAddress.Parse(subnetmaskOrCidr))
            : int.Parse(subnetmaskOrCidr.TrimStart('/'));
    }
}