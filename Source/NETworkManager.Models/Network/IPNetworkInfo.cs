using System.Net;
using System.Net.Sockets;
using System.Numerics;
using IPAddress = System.Net.IPAddress;

namespace NETworkManager.Models.Network;

public class IPNetworkInfo
{
    public IPNetworkInfo()
    {
    }

    public IPNetworkInfo(IPNetwork2 ipNetwork)
    {
        Network = ipNetwork.Network;
        Broadcast = ipNetwork.Broadcast;
        Total = ipNetwork.Total;
        Netmask = ipNetwork.Netmask;
        Cidr = ipNetwork.Cidr;
        FirstUsable = ipNetwork.FirstUsable;
        LastUsable = ipNetwork.LastUsable;
        Usable = ipNetwork.Usable;
    }

    public IPAddress Network { get; set; }
    public IPAddress Broadcast { get; set; }
    public BigInteger Total { get; set; }
    public IPAddress Netmask { get; set; }
    public int Cidr { get; set; }
    public IPAddress FirstUsable { get; set; }
    public IPAddress LastUsable { get; set; }
    public BigInteger Usable { get; set; }

    public int NetworkInt32 => Network.AddressFamily == AddressFamily.InterNetwork
        ? IPv4Address.ToInt32(Network)
        : 0;

    public int BroadcastInt32 => Broadcast.AddressFamily == AddressFamily.InterNetwork
        ? IPv4Address.ToInt32(Broadcast)
        : 0;

    public int NetmaskInt32 => Netmask.AddressFamily == AddressFamily.InterNetwork
        ? IPv4Address.ToInt32(Netmask)
        : 0;

    public int FirstUsableInt32 => FirstUsable.AddressFamily == AddressFamily.InterNetwork
        ? IPv4Address.ToInt32(FirstUsable)
        : 0;

    public int LastUsableInt32 => LastUsable.AddressFamily == AddressFamily.InterNetwork
        ? IPv4Address.ToInt32(LastUsable)
        : 0;
}