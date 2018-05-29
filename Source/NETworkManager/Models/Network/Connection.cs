using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public class Connection
    {
        #region Variables
        public static Task<List<ConnectionInfo>> GetActiveTCPConnectionsAsync()
        {
            return Task.Run(() => GetActiveTCPConnections());
        }

        public static List<ConnectionInfo> GetActiveTCPConnections()
        {
            List<ConnectionInfo> list = new List<ConnectionInfo>();

            foreach (TcpConnectionInformation tcpInfo in IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections())
                list.Add(new ConnectionInfo(tcpInfo.LocalEndPoint.Address, tcpInfo.LocalEndPoint.Port, tcpInfo.RemoteEndPoint.Address, tcpInfo.RemoteEndPoint.Port, tcpInfo.State));

            return list;
        }
        #endregion
    }
}