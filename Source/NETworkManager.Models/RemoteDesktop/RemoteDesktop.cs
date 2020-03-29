using System.Collections.Generic;

namespace NETworkManager.Models.RemoteDesktop
{
    public static class RemoteDesktop
    {
        public static List<string> ScreenResolutions => new List<string>
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

        public static List<int> ColorDepths => new List<int>
        {
            15,
            16,
            24,
            32
        };

        public static RemoteDesktopKeystrokeInfo GetKeystroke(Keystroke keystroke)
        {
            RemoteDesktopKeystrokeInfo info = new RemoteDesktopKeystrokeInfo();

            switch (keystroke)
            {
                case Keystroke.CtrlAltDel:
                    info.ArrayKeyUp = new bool[] { false, false, false, true, true, true, };
                    info.KeyData = new int[] { 0x1d, 0x38, 0x53, 0x53, 0x38, 0x1d };
                    break;
            }

            return info;
        }
    }
}
