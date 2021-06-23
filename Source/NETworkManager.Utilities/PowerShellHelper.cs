using System.Diagnostics;

namespace NETworkManager.Utilities
{
    public static class PowerShellHelper
    {
        public static void ExecuteCommand(string command, bool asAdmin = false, ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden)
        {
            var info = new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -NoLogo -Command {command}",
                UseShellExecute = true,
                WindowStyle = windowStyle
            };

            if (asAdmin)
                info.Verb = "runas";

            using var process = new Process();

            process.StartInfo = info;

            process.Start();
            process.WaitForExit();
        }
    }
}
