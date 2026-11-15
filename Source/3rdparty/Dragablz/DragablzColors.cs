using System;
using System.Windows.Media;
using Dragablz.Core;

namespace Dragablz
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// In supporting .Net 4.0 we don't have access to SystemParameters.WindowGlassBrush, and even then
    /// the opacity is not provided, so this class wraps up a few issues around here.
    /// </remarks>
    public static class DragablzColors
    {
        //TODO listen to changes from the OS to provide updates
        public static Brush WindowGlassBrush = GetWindowGlassBrush();

        private static Brush GetWindowGlassBrush()
        {
            var colorizationParams = new Native.DWMCOLORIZATIONPARAMS();
            Native.DwmGetColorizationParameters(ref colorizationParams);
            var frameColor = ToColor(colorizationParams.ColorizationColor);

            return new SolidColorBrush(frameColor);
        }

        private static Color ToColor(UInt32 value)
        {
            return Color.FromArgb(255,
                (byte)(value >> 16),
                (byte)(value >> 8),
                (byte)value
                );
        }

    }
}