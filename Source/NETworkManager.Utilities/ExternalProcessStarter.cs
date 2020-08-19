using System.Diagnostics;

namespace NETworkManager.Utilities
{
   public static class ExternalProcessStarter
    {
        /// <summary>
        /// Open an url with the default browser.
        /// </summary>
        /// <param name="url">Url like: https://github.com/BornToBeRoot</param>
        public static void OpenUrl(string url)
        {
            // Escape the $ in the command promp
            url = url.Replace("&", "^&");

            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true } );
        }
    }
}
