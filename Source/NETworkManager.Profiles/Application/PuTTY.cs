using NETworkManager.Models.PuTTY;
using NETworkManager.Settings;

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
                PrivateKey = profileInfo.PuTTY_OverridePrivateKeyFile ? profileInfo.PuTTY_PrivateKeyFile : SettingsManager.Current.PuTTY_PrivateKeyFile,
                Profile = profileInfo.PuTTY_OverrideProfile ? profileInfo.PuTTY_Profile : SettingsManager.Current.PuTTY_Profile,
                EnableLog = profileInfo.PuTTY_OverrideEnableLog ? profileInfo.PuTTY_EnableLog : SettingsManager.Current.PuTTY_EnableSessionLog,
                LogMode = profileInfo.PuTTY_OverrideLogMode ? profileInfo.PuTTY_LogMode : SettingsManager.Current.PuTTY_LogMode,
                LogPath = profileInfo.PuTTY_OverrideLogPath ? profileInfo.PuTTY_LogPath : Settings.Application.PuTTY.LogPath,
                LogFileName = profileInfo.PuTTY_OverrideLogFileName ? profileInfo.PuTTY_LogFileName : SettingsManager.Current.PuTTY_LogFileName,
                AdditionalCommandLine = profileInfo.PuTTY_OverrideAdditionalCommandLine ? profileInfo.PuTTY_AdditionalCommandLine : SettingsManager.Current.PuTTY_AdditionalCommandLine
            };

            return info;
        }
    }
}
