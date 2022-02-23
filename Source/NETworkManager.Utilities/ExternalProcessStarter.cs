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

            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }

        public static void RunProcess(string filename)
        {
            RunProcess(filename, false);
        }

        public static void RunProcess(string filename, bool asAdmin)
        {
            RunProcess(filename, "", asAdmin);
        }

        public static void RunProcess(string filename, string arguments = "", bool asAdmin = false)
        {
            ProcessStartInfo info = new()
            {
                FileName = filename,
                UseShellExecute = true
            };

            if (!string.IsNullOrEmpty(arguments))
                info.Arguments = arguments;

            if (asAdmin)
                info.Verb = "runas";

            Process.Start(info);
        }
    }
}
