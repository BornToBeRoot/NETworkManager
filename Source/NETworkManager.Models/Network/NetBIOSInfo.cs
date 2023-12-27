using System.Net;

namespace NETworkManager.Models.Network;

/// <summary>
///     Class containing information about a result of a <see cref="NetBIOSResolver" />.
/// </summary>
public class NetBIOSInfo
{
    /// <summary>
    ///     Constructor for an unreachable host.
    /// </summary>
    public NetBIOSInfo()
    {
        IsReachable = false;
    }

    /// <summary>
    ///     Constructor for a reachable host.
    /// </summary>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="computerName">Computer name of the host.</param>
    /// <param name="userName">User name of the host.</param>
    /// <param name="groupName">Group name or domain of the host.</param>
    /// <param name="macAddress">MAC address of the host.</param>
    /// <param name="vendor">Vendor of the host based on the MAC address.</param>
    public NetBIOSInfo(IPAddress ipAddress, string computerName, string userName, string groupName, string macAddress,
        string vendor)
    {
        IsReachable = true;
        IPAddress = ipAddress;
        ComputerName = computerName;
        UserName = userName;
        GroupName = groupName;
        MACAddress = macAddress;
        Vendor = vendor;
    }

    /// <summary>
    ///     Whether the host is reachable via NetBIOS.
    /// </summary>
    public bool IsReachable { get; set; }

    /// <summary>
    ///     IP address of the host.
    /// </summary>
    public IPAddress IPAddress { get; set; }

    /// <summary>
    ///     Computer name of the host.
    /// </summary>
    public string ComputerName { get; set; }

    /// <summary>
    ///     User name of the host.
    /// </summary>
    public string UserName { get; set; }

    /// <summary>
    ///     Group name or domain of the host.
    /// </summary>
    public string GroupName { get; set; }

    /// <summary>
    ///     MAC address of the host.
    /// </summary>
    public string MACAddress { get; set; }

    /// <summary>
    ///     Vendor of the host based on the MAC address.
    /// </summary>
    public string Vendor { get; set; }
}