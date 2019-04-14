namespace NETworkManager.Models.RemoteDesktop
{
    public static class RemoteDesktopPerformanceConstants
    {
        // No features are disabled.
        public const int TS_PERF_DISABLE_NOTHING = 0x00000000;

        // Wallpaper on the desktop is not displayed.
        public const int TS_PERF_DISABLE_WALLPAPER = 0x00000001;

        // Full-window drag is disabled; only the window outline is displayed when the window is moved. 
        public const int TS_PERF_DISABLE_FULLWINDOWDRAG = 0x00000002;

        // Menu animations are disabled.
        public const int TS_PERF_DISABLE_MENUANIMATIONS = 0x00000004;

        // Themes are disabled.
        public const int TS_PERF_DISABLE_THEMING = 0x00000008;

        // Enable enhanced graphics.
        public const int TS_PERF_ENABLE_ENHANCED_GRAPHICS = 0x00000010;

        // No shadow is displayed for the cursor.
        public const int TS_PERF_DISABLE_CURSOR_SHADOW = 0x00000020;

        // Cursor blinking is disabled.
        public const int TS_PERF_DISABLE_CURSORSETTINGS = 0x00000040;

        // Font smoothing
        public const int TS_PERF_ENABLE_FONT_SMOOTHING = 0x00000080;

        // Desktop composition
        public const int TS_PERF_ENABLE_DESKTOP_COMPOSITION = 0x00000100;

    }
}
