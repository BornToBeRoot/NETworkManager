using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using NETworkManager.Utilities;

namespace NETworkManager.Models.PuTTY;

/// <summary>
///     Class control PuTTY.
/// </summary>
public class PuTTY
{
    /// <summary>
    ///     PuTTY file name.
    /// </summary>
    public static readonly string FileName = "putty.exe";

    /// <summary>
    ///     Default SZ registry keys for PuTTY profile NETworkManager.
    /// </summary>
    private static readonly List<Tuple<string, string>> DefaultProfileRegkeysSzBase =
    [
        new("Colour1", "255,255,255"),
        new("Colour3", "85,85,85"),
        new("Colour4", "0,0,0"),
        new("Colour5", "0,255,0"),
        new("Colour6", "0,0,0"),
        new("Colour7", "85,85,85"),
        new("Colour8", "187,0,0"),
        new("Colour9", "255,85,85"),
        new("Colour10", "0,187,0"),
        new("Colour11", "85,255,85"),
        new("Colour12", "187,187,0"),
        new("Colour13", "255,255,85"),
        new("Colour14", "0,0,187"),
        new("Colour15", "85,85,255"),
        new("Colour16", "187,0,187"),
        new("Colour17", "255,85,255"),
        new("Colour18", "0,187,187"),
        new("Colour19", "85,255,255"),
        new("Colour20", "187,187,187"),
        new("Colour21", "255,255,255"),
        new("LineCodePage", "UTF-8"),
        new("Font", "Consolas")
    ];

    /// <summary>
    ///     Default DWORD registry keys for PuTTY profile NETworkManager.
    /// </summary>
    private static readonly List<Tuple<string, int>> DefaultProfileRegkeysDwordBase =
    [
        new("CurType", 2),
        new("FontHeight", 12),
        new("BlinkCur", 1),
        new("ScrollBar", 0)
    ];

    /// <summary>
    ///     SZ registry keys for PuTTY profile NETworkManager if app theme is dark.
    /// </summary>
    /// <returns>List with SZ registry keys.</returns>
    private static List<Tuple<string, string>> GetProfileRegkeysSzDark()
    {
        return DefaultProfileRegkeysSzBase.Concat(
        [
            // new Tuple<string, string>("Colour0", "255,255,255"),
                new Tuple<string, string>("Colour0", "187,187,187"), // Foreground
                new Tuple<string, string>("Colour2", "37,37,37") // Background
        ]).ToList();
    }

    /// <summary>
    ///     SZ registry keys for PuTTY profile NETworkManager if app theme is white.
    /// </summary>
    /// <returns>List with DWORD registry keys.</returns>
    private static List<Tuple<string, string>> GetProfileRegkeysSzWhite()
    {
        return DefaultProfileRegkeysSzBase.Concat(
        [
            // new Tuple<string, string>("Colour0", "68,68,68"),
                new Tuple<string, string>("Colour0", "0,0,0"), // Foreground
                new Tuple<string, string>("Colour2", "255,255,255") // Background
        ]).ToList();
    }

    /// <summary>
    ///     Write the default PuTTY profile NETworkManager to the registry.
    ///     HKCU\Software\SimonTatham\PuTTY\Sessions\NETworkManager
    /// </summary>
    /// <param name="theme">Current application theme to adjust the PuTTY colors</param>
    public static void WriteDefaultProfileToRegistry(string theme)
    {
        var profilePath = @"Software\SimonTatham\PuTTY\Sessions\NETworkManager";

        var registryKey = Registry.CurrentUser.OpenSubKey(profilePath, true);

        registryKey ??= Registry.CurrentUser.CreateSubKey(profilePath);

        if (registryKey != null)
        {
            foreach (var item in theme == "Dark" ? GetProfileRegkeysSzDark() : GetProfileRegkeysSzWhite())
                registryKey.SetValue(item.Item1, item.Item2);

            foreach (var item in DefaultProfileRegkeysDwordBase)
                registryKey.SetValue(item.Item1, item.Item2);
        }

        registryKey.Close();
    }

    /// <summary>
    ///     Build command line arguments based on a <see cref="PuTTYSessionInfo" />.
    /// </summary>
    /// <param name="sessionInfo">Instance of <see cref="PuTTYSessionInfo" />.</param>
    /// <returns>Command line arguments like "-ssh -l root -i C:\data\key.ppk".</returns>
    public static string BuildCommandLine(PuTTYSessionInfo sessionInfo)
    {
        var command = string.Empty;

        // PuTTY Profile
        if (!string.IsNullOrEmpty(sessionInfo.Profile))
            command += $"-load \"{sessionInfo.Profile}\"";

        // Protocol
        switch (sessionInfo.Mode)
        {
            case ConnectionMode.SSH:
                command += " -ssh";
                break;
            case ConnectionMode.Telnet:
                command += " -telnet";
                break;
            case ConnectionMode.Serial:
                command += " -serial";
                break;
            case ConnectionMode.Rlogin:
                command += " -rlogin";
                break;
            case ConnectionMode.RAW:
                command += " -raw";
                break;
        }

        // Username
        if (new[] { ConnectionMode.SSH, ConnectionMode.Telnet, ConnectionMode.Rlogin }.Contains(sessionInfo.Mode) &&
            !string.IsNullOrEmpty(sessionInfo.Username))
            command += $" -l {sessionInfo.Username}";

        // SSH specific settings
        if (sessionInfo.Mode == ConnectionMode.SSH)
        {
            // Private key
            if (!string.IsNullOrEmpty(sessionInfo.PrivateKey))
                command += $" -i \"{sessionInfo.PrivateKey}\"";

            // Hostkey(s)
            if (sessionInfo.Mode == ConnectionMode.SSH && !string.IsNullOrEmpty(sessionInfo.Hostkey))
            {
                var hostkeys = StringHelper.RemoveWhitespace(sessionInfo.Hostkey)
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var hostkey in hostkeys)
                    command += $" -hostkey \"{hostkey}\"";
            }
        }

        // Log
        if (sessionInfo.EnableLog)
        {
            switch (sessionInfo.LogMode)
            {
                case LogMode.SessionLog:
                    command += " -sessionlog";
                    break;
                case LogMode.SSHLog:
                    command += " -sshlog";
                    break;
                case LogMode.SSHRawLog:
                    command += " -sshrawlog";
                    break;
            }

            command +=
                $" \"{Environment.ExpandEnvironmentVariables(Path.Combine(sessionInfo.LogPath, sessionInfo.LogFileName))}\"";
        }

        // Additional command line
        if (!string.IsNullOrEmpty(sessionInfo.AdditionalCommandLine))
            command += $" {sessionInfo.AdditionalCommandLine}";

        // SerialLine, Baud
        if (sessionInfo.Mode == ConnectionMode.Serial)
            command += $" {sessionInfo.HostOrSerialLine} -sercfg {sessionInfo.PortOrBaud}";

        // Port, Host
        if (sessionInfo.Mode != ConnectionMode.Serial)
            command += $" -P {sessionInfo.PortOrBaud} {sessionInfo.HostOrSerialLine}";

        return command;
    }
}