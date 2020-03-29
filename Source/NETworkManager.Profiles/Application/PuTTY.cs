using NETworkManager.Models.PuTTY;
using NETworkManager.Settings;
using System.IO;

namespace NETworkManager.Profiles.Application
{
    public static class PuTTY
    {
        public static PuTTYSessionInfo CreateSessionInfo(ProfileInfo profileInfo)
        {
            var info = new PuTTYSessionInfo
            {
                Mode = profileInfo.PuTTY_ConnectionMode,
                HostOrSerialLine = profileInfo.PuTTY_HostOrSerialLine,
                PortOrBaud = profileInfo.PuTTY_OverridePortOrBaud ? profileInfo.PuTTY_PortOrBaud : Settings.Application.PuTTY.GetPortOrBaudByConnectionMode(profileInfo.PuTTY_ConnectionMode),
                Username = profileInfo.PuTTY_OverrideUsername ? profileInfo.PuTTY_Username : SettingsManager.Current.PuTTY_Username,
                Profile = profileInfo.PuTTY_OverrideProfile ? profileInfo.PuTTY_Profile : SettingsManager.Current.PuTTY_Profile,
                EnableSessionLog = profileInfo.PuTTY_OverrideEnableSessionLog ? profileInfo.PuTTY_EnableSessionLog : SettingsManager.Current.PuTTY_EnableSessionLog,
                SessionLogFullName = Path.Combine(Settings.Application.PuTTY.LogPath, profileInfo.PuTTY_OverrideSessionLogFileName ? profileInfo.PuTTY_SessionLogFileName : SettingsManager.Current.PuTTY_SessionLogFileName),
                AdditionalCommandLine = profileInfo.PuTTY_OverrideAdditionalCommandLine ? profileInfo.PuTTY_AdditionalCommandLine : SettingsManager.Current.PuTTY_AdditionalCommandLine
            };

            return info;
        }
    }
}
