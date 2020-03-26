namespace NETworkManager.Models.TigerVNC
{
    public static class TigerVNC
    {
        public static string BuildCommandLine(TigerVNCSessionInfo sessionInfo)
        {
            return $"{sessionInfo.Host}::{sessionInfo.Port}";
        }
    }
}
