using System.Windows;

namespace NETworkManager.Utilities
{
    /// <summary>
    /// Class for static common methods.
    /// </summary>
    public static class ClipboardHelper
    {
        /// <summary>
        /// Methods to set a text to the clipboard.
        /// </summary>
        /// <param name="text">Some text...</param>
        public static void SetClipboard(string text)
        {
            Clipboard.SetDataObject(text, true);
        }
    }
}
