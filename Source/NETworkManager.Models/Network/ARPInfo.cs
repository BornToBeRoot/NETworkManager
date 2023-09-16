using NETworkManager.Utilities;
using System.Net;
using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network;

/// <summary>
/// Class to store information about an ARP entry.
/// </summary>
public class ARPInfo
{
    /// <summary>
    /// IP address of the ARP entry.
    /// </summary>
    public IPAddress IPAddress { get; set; }
    
    /// <summary>
    /// Physical address of the ARP entry.
    /// </summary>
    public PhysicalAddress MACAddress { get; set; }
    
    /// <summary>
    /// Indicates if the ARP entry is a multicast address.
    /// </summary>
    public bool IsMulticast { get; set; }

    /// <summary>
    /// IP address as Int32.
    /// </summary>
    public int IPAddressInt32 => IPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? IPv4Address.ToInt32(IPAddress) : 0;
    
    /// <summary>
    /// Physical address as string.
    /// </summary>
    public string MACAddressString => MACAddress.ToString().Replace("-","").Replace("-","");

    /// <summary>
    /// Creates a new instance of <see cref="ARPInfo"/>.
    /// </summary>
    public ARPInfo()
    {

    }

    /// <summary>
    /// Creates a new instance of <see cref="ARPInfo"/> with the given parameters.
    /// </summary>
    /// <param name="ipAddress">IP address of the ARP entry.</param>
    /// <param name="macAddress">Physical address of the ARP entry.</param>
    /// <param name="isMulticast">Indicates if the ARP entry is a multicast address.</param>
    public ARPInfo(IPAddress ipAddress, PhysicalAddress macAddress, bool isMulticast)
    {
        IPAddress = ipAddress;
        MACAddress = macAddress;
        IsMulticast = isMulticast;
    }
}