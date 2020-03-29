using NETworkManager.Profiles;
using NETworkManager.Models.WebConsole;

namespace NETworkManager.Models.WebConsoleTMP
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
