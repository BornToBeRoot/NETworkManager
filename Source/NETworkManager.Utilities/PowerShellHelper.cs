using System;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;

namespace NETworkManager.Utilities;

public static class PowerShellHelper
{
    /// <summary>
    ///   Path to the PowerShell executable. Using "powershell.exe" allows the system to resolve it from the PATH,
    ///   ensuring compatibility across different Windows versions and configurations.
    /// </summary>
    private const string Powershell = "powershell.exe";

    /// <summary>
    ///   Base options for PowerShell execution:
    ///   -NoProfile: Prevents loading the user's PowerShell profile, ensuring a clean environment.
    ///   -NoLogo: Suppresses the PowerShell logo, providing a cleaner output.
    /// </summary>
    private const string BaseOpts = "-NoProfile -NoLogo";

    /// <summary>
    ///     Execute a PowerShell command. Writes a temporary script file if the command is longer than Windows limits.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    /// <param name="asAdmin">
    ///     Start PowerShell as an administrator. Error code 1223 is returned when the UAC dialog is canceled by
    ///     the user.
    /// </param>
    /// <param name="windowStyle">Window style of the PowerShell console (Default: Hidden)</param>
    public static void ExecuteCommand(string command, bool asAdmin = false, ProcessWindowStyle windowStyle = ProcessWindowStyle.Hidden)
    {
        string scriptPath = null;
        var commandOpts = $" -Command {command}";

        // Handle Windows command line length limit of 32,767 characters.
        if (Powershell.Length + BaseOpts.Length + commandOpts.Length > 32767)
        {
            scriptPath = Path.Combine(Path.GetTempPath(), $"NETworkManager_{Guid.NewGuid()}.ps1");

            File.WriteAllText(scriptPath, command);

            commandOpts = $" -ExecutionPolicy Bypass -File \"{scriptPath}\"";
        }

        try
        {
            var info = new ProcessStartInfo
            {
                FileName = Powershell,
                Arguments = $"{BaseOpts}{commandOpts}",
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
            if (scriptPath != null)
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

    /// <summary>
    ///     Escapes a string for safe embedding inside a PowerShell single-quoted string
    ///     by doubling any single-quote characters.
    /// </summary>
    public static string EscapeSingleQuotes(string value) => value.Replace("'", "''");
}
