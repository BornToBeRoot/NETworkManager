using NETworkManager.Models.Network;
using NETworkManager.Models.Profile;
using NETworkManager.Settings;
using System.Net;

namespace NETworkManager.Models.NetworkTMP
{
    public static class WakeOnLAN
    {
        #region Methods
      

        public static WakeOnLANInfo CreateWakeOnLANInfo(ProfileInfo profileInfo)
        {
            var info = new WakeOnLANInfo
            {
                MagicPacket = Network.WakeOnLAN.CreateMagicPacket(profileInfo.WakeOnLAN_MACAddress),
                Broadcast = IPAddress.Parse(profileInfo.WakeOnLAN_Broadcast),
                Port = profileInfo.WakeOnLAN_OverridePort ? profileInfo.WakeOnLAN_Port : SettingsManager.Current.WakeOnLAN_Port
            };

            return info;
        }
        #endregion
    }
}
