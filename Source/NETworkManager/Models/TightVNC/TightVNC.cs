namespace NETworkManager.Models.TightVNC
{
    public class TightVNC
    {
        public static string BuildCommandLine(TightVNCSessionInfo sessionInfo)
        {
            return $"-host={sessionInfo.Host} -port={sessionInfo.Port}";
        }
    }
}
