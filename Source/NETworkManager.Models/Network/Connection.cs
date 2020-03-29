using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace NETworkManager.Models.Network
{
    public partial class Connection
    {
        #region Methods
        public static Task<List<ConnectionInfo>> GetActiveTcpConnectionsAsync()
        {
            return Task.Run(() => GetActiveTcpConnections());
        }

        public static List<ConnectionInfo> GetActiveTcpConnections()
        {
            return IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections().Select(tcpInfo => new ConnectionInfo(Protocol.TCP, tcpInfo.LocalEndPoint.Address, tcpInfo.LocalEndPoint.Port, tcpInfo.RemoteEndPoint.Address, tcpInfo.RemoteEndPoint.Port, tcpInfo.State)).ToList();
        }
        #endregion
    }
}