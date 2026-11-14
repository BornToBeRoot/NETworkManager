using System;
using NETworkManager.Models;

namespace NETworkManager.Settings;

public static class CommandLineManager
{
    private const string ParameterIdentifier = "--";
    private const char ValueSplitIdentifier = ':';

    static CommandLineManager()
    {
        Current = new CommandLineInfo();

        // Detect start parameters
        var parameters = Environment.GetCommandLineArgs();

        for (var i = 1; i < parameters.Length; i++)
        {
            var param = parameters[i].Split(ValueSplitIdentifier);

            if (param[0].StartsWith(ParameterIdentifier))
            {
                if (param[0].Equals(ParameterHelp, StringComparison.InvariantCultureIgnoreCase))
                {
                    Current.Help = true;
                } // Autostart
                else if (param[0].Equals(ParameterAutostart, StringComparison.InvariantCultureIgnoreCase))
                {
                    Current.Autostart = true;
                } // Reset Settings                    
                else if (param[0].Equals(ParameterResetSettings, StringComparison.InvariantCultureIgnoreCase))
                {
                    Current.ResetSettings = true;
                } // Restart
                else if (param[0].Equals(ParameterRestartPid, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (int.TryParse(param[1], out var restartPid))
                    {
                        Current.RestartPid = restartPid;
                    }
                    else
                    {
                        WrongParameterDetected(parameters);
                        return;
                    }
                } // Application
                else if (param[0].Equals(ParameterApplication, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (Enum.TryParse(param[1], out ApplicationName application))
                    {
                        Current.Application = application;
                    }
                    else
                    {
                        WrongParameterDetected(parameters);
                        return;
                    }
                } // Screenshot mode (optional size, e.g. --screenshot:1280x800)
                else if (param[0].Equals(ParameterScreenshot, StringComparison.InvariantCultureIgnoreCase))
                {
                    Current.Screenshot = true;

                    if (param.Length > 1)
                    {
                        if (TryParseScreenshotSize(param[1], out var width, out var height))
                        {
                            Current.ScreenshotWidth = width;
                            Current.ScreenshotHeight = height;
                        }
                        else
                        {
                            WrongParameterDetected(parameters);
                            return;
                        }
                    }
                }
                else
                {
                    WrongParameterDetected(parameters);
                    return;
                }
            }
            else
            {
                // Ignore the first parameter because it's the path of the .exe
                if (i == 0)
                    continue;

                WrongParameterDetected(parameters);
                return;
            }
        }
    }

    // Public 
    public static string ParameterHelp => $"{ParameterIdentifier}help";
    public static string ParameterResetSettings => $"{ParameterIdentifier}reset-settings";
    public static string ParameterApplication => $"{ParameterIdentifier}application";

    // Internal use only
    public static string ParameterAutostart => $"{ParameterIdentifier}autostart";
    public static string ParameterRestartPid => $"{ParameterIdentifier}restart-pid";

    // Documentation (screenshots)
    public static string ParameterScreenshot => $"{ParameterIdentifier}screenshot";

    /// <summary>
    ///     Default window width in screenshot mode (16:10, readable when scaled to the width of the documentation).
    /// </summary>
    public const int ScreenshotDefaultWidth = 1280;

    /// <summary>
    ///     Default window height in screenshot mode.
    /// </summary>
    public const int ScreenshotDefaultHeight = 800;

    // Minimum size of the main window (MinWidth / MinHeight in MainWindow.xaml)
    private const int ScreenshotMinWidth = 800;
    private const int ScreenshotMinHeight = 600;
    private const int ScreenshotMaxSize = 7680;

    public static CommandLineInfo Current { get; set; }

    public static string GetParameterWithSplitIdentifier(string parameter)
    {
        return $"{parameter}{ValueSplitIdentifier}";
    }

    /// <summary>
    ///     Parses the window size of the screenshot mode (<c>WIDTHxHEIGHT</c>, e.g. <c>1280x800</c>).
    /// </summary>
    /// <param name="value">Size to parse.</param>
    /// <param name="width">Parsed width.</param>
    /// <param name="height">Parsed height.</param>
    /// <returns><c>true</c> if the size is valid and not smaller than the minimum size of the main window.</returns>
    public static bool TryParseScreenshotSize(string value, out int width, out int height)
    {
        width = 0;
        height = 0;

        var size = value.Split('x', 'X');

        if (size.Length != 2 ||
            !int.TryParse(size[0], out var parsedWidth) ||
            !int.TryParse(size[1], out var parsedHeight))
            return false;

        if (parsedWidth is < ScreenshotMinWidth or > ScreenshotMaxSize ||
            parsedHeight is < ScreenshotMinHeight or > ScreenshotMaxSize)
            return false;

        width = parsedWidth;
        height = parsedHeight;

        return true;
    }

    private static void WrongParameterDetected(string[] parameters)
    {
        Current.WrongParameter = string.Join(" ", parameters);
        Current.Help = true;
    }
}