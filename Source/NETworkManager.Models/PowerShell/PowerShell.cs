using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
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
    ///     Real paths of App Execution Aliases (Microsoft Store), resolved once per start of the application (the real
    ///     path contains the version and changes with every update). Lazy, so parallel calls start the alias only once;
    ///     a failed resolve is removed and tried again next time.
    /// </summary>
    private static readonly ConcurrentDictionary<string, Lazy<string>> ResolvedAliases = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    ///     Write default (global) PowerShell profile to registry: the console settings of Windows (conhost) in
    ///     HKCU\Console\&lt;title&gt;, the title is the path of the started executable (\ replaced by _, the Windows
    ///     directory as %SystemRoot%).
    ///
    ///     PowerShell from the Microsoft Store is started via an App Execution Alias (e.g.
    ///     %LocalAppData%\Microsoft\WindowsApps\Microsoft.PowerShell_8wekyb3d8bbwe\pwsh.exe), which starts the real
    ///     executable in C:\Program Files\WindowsApps\Microsoft.PowerShell_7.x.x.0_x64__8wekyb3d8bbwe\pwsh.exe - the
    ///     profile is written for the alias and for the real path, so it applies whichever the console uses as title.
    ///     Resolving the alias starts it once (can take a moment, call it in the background).
    /// </summary>
    /// <param name="background">Background of the console (DefaultBackground, ColorTable00).</param>
    /// <param name="foreground">Text of the console (ColorTable07).</param>
    /// <param name="powerShellPath">Path to the PowerShell executable.</param>
    public static void WriteDefaultProfileToRegistry((byte R, byte G, byte B) background,
        (byte R, byte G, byte B) foreground, string powerShellPath)
    {
        WriteProfile(background, foreground, powerShellPath);

        if (!IsAppExecutionAlias(powerShellPath))
            return;

        var resolved = ResolvedAliases.GetOrAdd(powerShellPath, x => new Lazy<string>(() => ResolveAppExecutionAlias(x)));
        var realPath = resolved.Value;

        // Not resolved (e.g. timeout): try again with the next write
        if (string.IsNullOrEmpty(realPath))
            ResolvedAliases.TryRemove(new KeyValuePair<string, Lazy<string>>(powerShellPath, resolved));

        if (!string.IsNullOrEmpty(realPath) && !string.Equals(realPath, powerShellPath, StringComparison.OrdinalIgnoreCase))
            WriteProfile(background, foreground, realPath);
    }

    /// <summary>
    ///     Whether the path is an App Execution Alias of an app from the Microsoft Store
    ///     (%LocalAppData%\Microsoft\WindowsApps\...).
    /// </summary>
    public static bool IsAppExecutionAlias(string path)
    {
        var aliasFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Microsoft", "WindowsApps") + Path.DirectorySeparatorChar;

        return !string.IsNullOrEmpty(path) && path.StartsWith(aliasFolder, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Resolves the real executable of an App Execution Alias: starts it and asks for the path of its own process
    ///     (works with Windows PowerShell and PowerShell 7).
    /// </summary>
    /// <returns>Real path or <see langword="null" /> if it could not be resolved.</returns>
    private static string ResolveAppExecutionAlias(string aliasPath)
    {
        try
        {
            ProcessStartInfo info = new()
            {
                FileName = aliasPath,
                Arguments = "-NoLogo -NoProfile -NonInteractive -Command \"(Get-Process -Id $PID).Path\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(info);

            if (process == null)
                return null;

            // Read asynchronously, so the timeout also applies if the process doesn't end (ReadToEnd would block)
            var outputTask = process.StandardOutput.ReadToEndAsync();

            if (!process.WaitForExit(10000))
            {
                process.Kill(true);
                Log.Warn($"Timeout while resolving the App Execution Alias \"{aliasPath}\".");

                return null;
            }

            if (!outputTask.Wait(2000))
                return null;

            var output = outputTask.Result.Trim();

            Log.Info($"App Execution Alias \"{aliasPath}\" resolved to \"{output}\".");

            return File.Exists(output) ? output : null;
        }
        catch (Exception ex)
        {
            Log.Error($"Could not resolve the App Execution Alias \"{aliasPath}\".", ex);

            return null;
        }
    }

    /// <summary>
    ///     Writes the profile for the console title of the executable path.
    /// </summary>
    private static void WriteProfile((byte R, byte G, byte B) background, (byte R, byte G, byte B) foreground,
        string powerShellPath)
    {
        var registryPath = @"Console\";

        var systemRoot = Environment.ExpandEnvironmentVariables("%SystemRoot%");

        // Windows PowerShell --> HKCU:\Console\%SystemRoot%_System32_WindowsPowerShell_v1.0_powershell.exe
        if (powerShellPath.StartsWith(systemRoot, StringComparison.OrdinalIgnoreCase))
            registryPath += "%SystemRoot%" + powerShellPath.Substring(systemRoot.Length).Replace(@"\", "_");
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