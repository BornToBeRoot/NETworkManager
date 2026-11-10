using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Microsoft.Win32;

namespace NETworkManager.Models.PowerShell;

/// <summary>
///     Class with static methods for PowerShell.
/// </summary>
public static class PowerShell
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(PowerShell));

    /// <summary>
    /// Windows PowerShell file name.
    /// </summary>
    public const string WindowsPowerShellFileName = "powershell.exe";

    /// <summary>
    /// PowerShell Core file name.
    /// </summary>
    public const string PwshFileName = "pwsh.exe";

    /// <summary>
    ///     Default SZ registry keys for the global PowerShell profile.
    /// </summary>
    private static readonly List<Tuple<string, string>> DefaultProfileRegkeysSzBase =
        [
            new("FaceName", "Consolas")
        ];

    /// <summary>
    ///     Default DWORD registry keys for the global PowerShell profile.
    /// </summary>
    private static readonly List<Tuple<string, int>> DefaultProfileRegkeysDwordBase =
    [
        new("CursorType", 1),
        new("FontFamily", 54),    // 36
        new("FontSize", 1179648), // 120000
        new("FontWeight", 400)    // 190
    ];

    /// <summary>
    ///     Default DWORD registry keys for the global PowerShell profile to delete.
    /// </summary>
    private static readonly List<string> DefaultProfileRegkeysDwordDelete = [
        "ScreenColors"
    ];

    /// <summary>
    ///     Write default (global) PowerShell profile to registry.
    /// </summary>
    /// <param name="powerShellPath">Path to the PowerShell executable.</param>
    /// <param name="background">Background of the console (DefaultBackground, ColorTable00).</param>
    /// <param name="foreground">Text of the console (ColorTable07).</param>
    public static void WriteDefaultProfileToRegistry((byte R, byte G, byte B) background,
        (byte R, byte G, byte B) foreground, string powerShellPath)
    {
        var registryPath = @"Console\";

        var systemRoot = Environment.ExpandEnvironmentVariables("%SystemRoot%");

        // Windows PowerShell --> HKCU:\Console\%SystemRoot%_System32_WindowsPowerShell_v1.0_powershell.exe
        if (powerShellPath.StartsWith(systemRoot))
            registryPath += "%SystemRoot%" + powerShellPath.Substring(systemRoot.Length, powerShellPath.Length - systemRoot.Length).Replace(@"\", "_");
        // PWSH -->  HKCU:\Console\C:_Program Files_PowerShell_7_pwsh.exe
        else
            registryPath += powerShellPath.Replace(@"\", "_");

        Log.Info($"Registry path for PowerShell profile: \"{registryPath}\"");

        var registryKey = Registry.CurrentUser.OpenSubKey(registryPath, true);

        registryKey ??= Registry.CurrentUser.CreateSubKey(registryPath);

        if (registryKey != null)
        {
            foreach (var item in DefaultProfileRegkeysDwordBase)
                registryKey.SetValue(item.Item1, item.Item2);

            // Colors of the application theme (COLORREF: 0x00BBGGRR), the console blends into the application
            registryKey.SetValue("DefaultBackground", ToColorRef(background));
            registryKey.SetValue("ColorTable00", ToColorRef(background));
            registryKey.SetValue("ColorTable07", ToColorRef(foreground));

            foreach (var item in DefaultProfileRegkeysSzBase)
                registryKey.SetValue(item.Item1, item.Item2);

            foreach (var item in DefaultProfileRegkeysDwordDelete) registryKey.DeleteValue(item, false);
        }

        registryKey?.Close();
    }

    private static int ToColorRef((byte R, byte G, byte B) color) => color.R | (color.G << 8) | (color.B << 16);

    /// <summary>
    ///     Build command line arguments based on a <see cref="PowerShellSessionInfo" />.
    /// </summary>
    /// <param name="sessionInfo">Instance of <see cref="PowerShellSessionInfo" />.</param>
    /// <returns>Command line arguments like "-NoExit -ExecutionPolicy RemoteSigned ...".</returns>
    public static string BuildCommandLine(PowerShellSessionInfo sessionInfo)
    {
        var command = $"-NoExit -ExecutionPolicy {sessionInfo.ExecutionPolicy} {sessionInfo.AdditionalCommandLine}";

        // Connect to remote host or execute local command if configured
        if (sessionInfo.EnableRemoteConsole)
            command += $" -Command \"& {{Enter-PSSession -ComputerName {sessionInfo.Host}}}\"";
        else if (!string.IsNullOrEmpty(sessionInfo.Command))
            command += $" -Command \"& {{{sessionInfo.Command}}}\"";

        return command;
    }
}