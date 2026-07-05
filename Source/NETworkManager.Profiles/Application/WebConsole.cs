using NETworkManager.Models.WebConsole;
using NETworkManager.Utilities;

namespace NETworkManager.Profiles.Application;

public class WebConsole
{
    public static WebConsoleSessionInfo CreateSessionInfo(ProfileInfo profileInfo)
    {
        var info = new WebConsoleSessionInfo
        {
            Url = PlaceholderHelper.Resolve(profileInfo.WebConsole_Url, (PlaceholderHelper.Host, profileInfo.Host))
        };

        return info;
    }
}