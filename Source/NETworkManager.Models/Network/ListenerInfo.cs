using NETworkManager.Utilities;
using System.Net;
using static NETworkManager.Models.Network.Listener;

namespace NETworkManager.Models.Network
{
    public class ListenerInfo
    {
        public Listener.Protocol Protocol { get; set; }
        public IPAddress IPAddress { get; set; }
        public int Port { get; set; }

        public int IPAddressInt32 => IPAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? IPv4Address.ToInt32(IPAddress) : 0;

        public ListenerInfo()
        {

        }

        public ListenerInfo(Listener.Protocol protocol, IPAddress ipddress, int port)
        {
            Protocol = protocol;
            IPAddress = ipddress;
            Port = port;
        }
    }
}