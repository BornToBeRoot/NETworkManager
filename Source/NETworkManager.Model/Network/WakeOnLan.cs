using System;
using System.Net.Sockets;

namespace NETworkManager.Model.Network
{
    public class WakeOnLan
    {
        #region Events
        public event EventHandler Completed;
        
        protected virtual void OnCompleted()
        {
            Completed?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Methods
        public void Send(WakeOnLanInfo info)
        {
            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.Connect(info.Broadcast, info.Port);

                udpClient.Send(info.MagicPacket, info.MagicPacket.Length);

                OnCompleted();
            }
        }
        #endregion
    }
}
