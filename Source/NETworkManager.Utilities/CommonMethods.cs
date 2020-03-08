using System.Windows;

namespace NETworkManager.Utilities
{
    public static class CommonMethods
    {
        public static void SetClipboard(string text)
        {
            Clipboard.SetDataObject(text, true);
        }
    }
}
