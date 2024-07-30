using System;
using System.Windows;
using log4net;

namespace NETworkManager.Utilities;

/// <summary>
///     Class provides static methods to interact with the clipboard.
/// </summary>
public static class ClipboardHelper
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(ClipboardHelper));

    /// <summary>
    ///     Methods to set a text to the clipboard.
    /// </summary>
    /// <param name="text">Some text...</param>
    public static void SetClipboard(string text)
    {
        try
        {
            Clipboard.SetDataObject(text, true);
        }
        catch (Exception e)
        {
            Log.Error($"Failed to set clipboard: {e.Message}");
        }
    }
}