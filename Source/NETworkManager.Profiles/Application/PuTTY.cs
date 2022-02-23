using NETworkManager.Models.PuTTY;
using NETworkManager.Settings;

namespace NETworkManager.Profiles.Application
{
    public static class PuTTY
    {
        public static PuTTYSessionInfo CreateSessionInfo(ProfileInfo profile)
        {
            // Get group info
            GroupInfo group = ProfileManager.GetGroup(profile.Group);

            return new PuTTYSessionInfo
            {
                Mode = profile.PuTTY_ConnectionMode,
                HostOrSerialLine = profile.PuTTY_HostOrSerialLine,

                PortOrBaud = profile.PuTTY_OverridePortOrBaud ? profile.PuTTY_PortOrBaud : Settings.Application.PuTTY.GetPortOrBaudByConnectionMode(profile.PuTTY_ConnectionMode),

                Username = profile.PuTTY_OverrideUsername ? profile.PuTTY_Username : (group.PuTTY_OverrideUsername ? group.PuTTY_Username : SettingsManager.Current.PuTTY_Username),
                PrivateKey = profile.PuTTY_OverridePrivateKeyFile ? profile.PuTTY_PrivateKeyFile : (group.PuTTY_OverridePrivateKeyFile ? group.PuTTY_PrivateKeyFile : SettingsManager.Current.PuTTY_PrivateKeyFile),
                Profile = profile.PuTTY_OverrideProfile ? profile.PuTTY_Profile : (group.PuTTY_OverrideProfile ? group.PuTTY_Profile : SettingsManager.Current.PuTTY_Profile),
                EnableLog = profile.PuTTY_OverrideEnableLog ? profile.PuTTY_EnableLog : (group.PuTTY_OverrideEnableLog ? group.PuTTY_EnableLog : SettingsManager.Current.PuTTY_EnableSessionLog),
                LogMode = profile.PuTTY_OverrideLogMode ? profile.PuTTY_LogMode : (group.PuTTY_OverrideLogMode ? group.PuTTY_LogMode : SettingsManager.Current.PuTTY_LogMode),
                LogPath = profile.PuTTY_OverrideLogPath ? profile.PuTTY_LogPath : (group.PuTTY_OverrideLogPath ? group.PuTTY_LogPath : Settings.Application.PuTTY.LogPath),
                LogFileName = profile.PuTTY_OverrideLogFileName ? profile.PuTTY_LogFileName : (group.PuTTY_OverrideLogFileName ? group.PuTTY_LogFileName : SettingsManager.Current.PuTTY_LogFileName),
                AdditionalCommandLine = profile.PuTTY_OverrideAdditionalCommandLine ? profile.PuTTY_AdditionalCommandLine : (group.PuTTY_OverrideAdditionalCommandLine ? group.PuTTY_AdditionalCommandLine : SettingsManager.Current.PuTTY_AdditionalCommandLine)
            };
        }
    }
}
