using NETworkManager.Models.Profile;

namespace NETworkManager.Models.WebConsole
{
    public class WebConsole
    {        
        public static WebConsoleSessionInfo CreateSessionInfo(ProfileInfo profileInfo)
        {
            var info = new WebConsoleSessionInfo
            {
                Url = profileInfo.WebConsole_Url
            };

            return info;
        }
    }
}
