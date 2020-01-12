using NETworkManager.Models.Profile;
using NETworkManager.Models.Settings;

namespace NETworkManager.Models.WebConsole
{
    public class WebConsole
    {        
        public static WebConsoleSessionInfo CreateSessionInfo(ProfileInfo profileInfo)
        {
            var info = new WebConsoleSessionInfo
            {
                Url = profileInfo.WebConsole_Url,
                IgnoreCertificateErrors = profileInfo.WebConsole_OverrideIgnoreCertificateErrors ? profileInfo.WebConsole_IgnoreCertificateErrors : SettingsManager.Current.WebConsole_IgnoreCertificateErrors
            };

            return info;
        }
    }
}
