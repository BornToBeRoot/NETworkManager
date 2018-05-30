using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public class Connection
    {
        #region Methods
        public static Task<List<ConnectionInfo>> GetActiveTcpConnectionsAsync()
        {
            return Task.Run(() => GetActiveTcpConnections());
        }

        public static List<ConnectionInfo> GetActiveTcpConnections()
        {
            List<ConnectionInfo> list = new List<ConnectionInfo>();

            foreach (TcpConnectionInformation tcpInfo in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections())
                list.Add(new ConnectionInfo(Protocol.TCP, tcpInfo.LocalEndPoint.Address, tcpInfo.LocalEndPoint.Port, tcpInfo.RemoteEndPoint.Address, tcpInfo.RemoteEndPoint.Port, tcpInfo.State));

            return list;
        }
        #endregion

        #region Enum
        public enum Protocol
        {
            TCP
        }

        #endregion
    }
}