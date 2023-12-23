using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network;

public class Listener
{
    #region Methods

    public static Task<List<ListenerInfo>> GetAllActiveListenersAsync()
    {
        return Task.Run(() => GetAllActiveListeners());
    }

    public static List<ListenerInfo> GetAllActiveListeners()
    {
        var list = new List<ListenerInfo>();

        list.AddRange(GetActiveTcpListeners());
        list.AddRange(GetActiveUdpListeners());

        return list;
    }

    public static Task<List<ListenerInfo>> GetActiveTcpListenersAsync()
    {
        return Task.Run(() => GetActiveTcpListeners());
    }

    public static List<ListenerInfo> GetActiveTcpListeners()
    {
        var list = new List<ListenerInfo>();

        foreach (var ipEndPoint in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners())
            list.Add(new ListenerInfo(TransportProtocol.Tcp, ipEndPoint.Address, ipEndPoint.Port));

        return list;
    }

    public static Task<List<ListenerInfo>> GetActiveUdpListenersAsync()
    {
        return Task.Run(() => GetActiveUdpListeners());
    }

    public static List<ListenerInfo> GetActiveUdpListeners()
    {
        var list = new List<ListenerInfo>();

        foreach (var ipEndPoint in IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners())
            list.Add(new ListenerInfo(TransportProtocol.Udp, ipEndPoint.Address, ipEndPoint.Port));

        return list;
    }

    #endregion
}