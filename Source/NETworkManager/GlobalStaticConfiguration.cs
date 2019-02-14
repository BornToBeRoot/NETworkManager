using System;
using System.Collections.Generic;
using System.IO;
// ReSharper disable InconsistentNaming

namespace NETworkManager
{
    public class GlobalStaticConfiguration
    {
        // General
        public static double FloatPointFix => 1.0;

        // Type to search (verage type speed --> 187 chars/min)
        public static TimeSpan SearchDispatcherTimerTimeSpan => new TimeSpan(0, 0, 0, 0, 750);
        public static TimeSpan CredentialsUILockTime => new TimeSpan(0, 0, 120);

        // Filter
        public static string ApplicationFileExtensionFilter => "Application (*.exe)|*.exe";

        // Profile
        public static double Profile_WidthCollapsed => 40;
        public static double Profile_DefaultWidthExpanded => 250;
        public static double Profile_MaxWidthExpanded => 350;

        // Application: RemoteDesktop
        public static List<string> RemoteDesktop_ScreenResolutions => new List<string>
        {
            "640x480",
            "800x600",
            "1024x768",
            "1280x720",
            "1280x768",
            "1280x800",
            "1280x1024",
            "1366x768",
            "1440x900",
            "1400x1050",
            "1680x1050",
            "1920x1080"
        };
        public static int RemoteDesktop_ScreenWidth => 1280;
        public static int RemoteDesktop_ScreenHeight => 768;
        public static List<int> RemoteDesktop_ColorDepths => new List<int>
        {
            15,
            16,
            24,
            32
        };
        public static int RemoteDesktop_ColorDepth = 32;
        public static int RemoteDesktop_Port => 3389;
        public static List<Tuple<int, string>> RemoteDesktop_KeyboardHookModes => new List<Tuple<int, string>>
        {
            Tuple.Create(0, Resources.Localization.Strings.OnThisComputer),
            Tuple.Create(1, Resources.Localization.Strings.OnTheRemoteComputer)/*,
            Tuple.Create(2, Resources.Localization.Strings.OnlyWhenUsingTheFullScreen),*/
        };

        // Application: PuTTY
        public static int PuTTY_DefaultSSHPort => 22;
        public static string PuTTY_DefaultSerialLine => "COM1";
        public static int PuTTY_DefaultTelnetPort => 23;
        public static int PuTTY_DefaultRloginPort => 513;
        public static int PuTTY_DefaultBaudRate => 9600;
        public static int PuTTY_DefaultRaw => 0;

        // Application: TightVNC
        public static int TightVNC_DefaultVNCPort => 5900;

        // Application: PowerShell
        public static string PowerShell_ApplicationFileLocationPowerShell => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"System32\WindowsPowerShell\v1.0\powershell.exe");
    }
}