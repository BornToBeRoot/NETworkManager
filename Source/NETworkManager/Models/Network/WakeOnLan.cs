using System;
using System.Net.Sockets;

namespace NETworkManager.Models.Network
{
    public class WakeOnLAN
    {
        #region Events
        public event EventHandler Completed;
        
        protected virtual void OnCompleted()
        {
            Completed?.Invoke(this, System.EventArgs.Empty);
        }
        #endregion

        #region Methods
        public void Send(WakeOnLANInfo info)
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
