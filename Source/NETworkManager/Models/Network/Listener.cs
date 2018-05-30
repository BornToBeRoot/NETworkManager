using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public class Listener
    {
        #region Methods
        public static Task<List<ListenerInfo>> GetAllActiveListenersAsync()
        {
            return Task.Run(() => GetAllActiveListeners());

        }

        public static List<ListenerInfo> GetAllActiveListeners()
        {
            List<ListenerInfo> list = new List<ListenerInfo>();

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
            List<ListenerInfo> list = new List<ListenerInfo>();

            foreach (IPEndPoint ipEndPoint in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners())
                list.Add(new ListenerInfo(Protocol.TCP, ipEndPoint.Address, ipEndPoint.Port));

            return list;
        }

        public static Task<List<ListenerInfo>> GetActiveUdpListenersAsync()
        {
            return Task.Run(() => GetActiveUdpListeners());
        }

        public static List<ListenerInfo> GetActiveUdpListeners()
        {
            List<ListenerInfo> list = new List<ListenerInfo>();

            foreach (IPEndPoint ipEndPoint in IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners())
                list.Add(new ListenerInfo(Protocol.UDP, ipEndPoint.Address, ipEndPoint.Port));

            return list;
        }
        #endregion

        #region Enum
        public enum Protocol
        {
            TCP,
            UDP
        }

        #endregion
    }
}