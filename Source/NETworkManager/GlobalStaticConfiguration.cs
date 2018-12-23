using System;
using System.IO;

namespace NETworkManager
{
    public class GlobalStaticConfiguration
    {
        // Type to search (verage type speed --> 187 chars/min)
        public static TimeSpan SearchDispatcherTimerTimeSpan => new TimeSpan(0, 0, 0, 0, 750);
        public static TimeSpan CredentialsUILockTime => new TimeSpan(0, 0, 120);

        // Profile
        public static double ProfileWidthCollapsed => 40;
        public static double ProfileDefaultWidthExpanded => 250;
        public static double ProfileMaxWidthExpanded => 350;

        // Application: PuTTY
        public static int PuTTYDefaultSSHPort => 22;
        public static string PuTTYDefaultSerialLine => "COM1";
        public static int PuTTYDefaultTelnetPort => 23;
        public static int PuTTYDefaultRloginPort => 513;
        public static int PuTTYDefaultBaudRate => 9600;
        public static int PuTTYDefaultRaw => 0;

        // Application: TightVNC
        public static int TightVNCDefaultVNCPort => 5900;

        // Application: PowerShell
        public static string DefaultApplicationFileLocationPowerShell => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), @"System32\WindowsPowerShell\v1.0\powershell.exe");

        // Filter
        public static string ApplicationFileExtensionFilter => "Application (*.exe)|*.exe";

    }
}