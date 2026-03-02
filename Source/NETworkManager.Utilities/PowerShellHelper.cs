using System;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;

namespace NETworkManager.Utilities;

public static class PowerShellHelper
{
    /// <summary>
    ///     Execute a PowerShell command. Writes a temporary script file if the command is longer than Windows limits.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    /// <param name="asAdmin">
    ///     Start PowerShell as an administrator. Error code 1223 is returned when the UAC dialog is canceled by
    ///     the user.
    /// </param>
    /// <param name="windowStyle">Window style of the PowerShell console (Default: Hidden)</param>
    public static void ExecuteCommand(string command, bool asAdmin = false,
        ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden)
    {
        string scriptPath = string.Empty;
        string powershell = "powershell.exe";
        string baseOpts = "-NoProfile -NoLogo";
        string commandOpts = $" -Command {command}";
        // Handle Windows command line length limit of 32 767 characters
        if (powershell.Length
            + baseOpts.Length
            + commandOpts.Length > 32767)
        {
            var tempDir = Path.GetTempPath();
            scriptPath = Path.Combine(tempDir, $"NwM_{Guid.NewGuid()}_Temp.ps1");
            File.WriteAllText(scriptPath, command);
            commandOpts = $" -ExecutionPolicy Bypass -File \"{scriptPath}\"";
        }

        try
        {
            var info = new ProcessStartInfo()
            {
                FileName = powershell,
                Arguments = $"{baseOpts} {commandOpts}",
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
        catch (Win32Exception e) when (asAdmin)
        {
            if (e.NativeErrorCode != 1223)
                throw;
            // Nothing to handle on UAC cancellation
        }
        finally
        {
            if (scriptPath != string.Empty)
            {
                try
                {
                    File.Delete(scriptPath);
                }
                catch
                {
                    // ignored
                }
            }
        }

    }
}