using System.Diagnostics;

namespace NETworkManager.Utilities;

public static class PowerShellHelper
{
    /// <summary>
    /// Execute a PowerShell command.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    /// <param name="asAdmin">Start PowerShell as administrator. Error code 1223 is returned when UAC dialog is canceled by user.</param>
    /// <param name="windowStyle">Window style of the PowerShell console (Default: Hidden)</param>
    public static void ExecuteCommand(string command, bool asAdmin = false, ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden)
    {
        var info = new ProcessStartInfo()
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -NoLogo -Command {command}",
            UseShellExecute = true,
            WindowStyle = ProcessWindowStyle.Normal //windowStyle
        };

        if (asAdmin)
            info.Verb = "runas";

        using var process = new Process();

        process.StartInfo = info;

        process.Start();
        process.WaitForExit();
    }
}
