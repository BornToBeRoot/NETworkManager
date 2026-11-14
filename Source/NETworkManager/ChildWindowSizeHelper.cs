using System.Windows;
using MahApps.Metro.SimpleChildWindow;

namespace NETworkManager;

/// <summary>
///     Size of a dialog relative to its parent window (85 %, limited by ChildWindowMaxWidth / -Height of the dialog),
///     updated when the parent window is resized.
/// </summary>
public static class ChildWindowSizeHelper
{
    private const double Factor = 0.85;

    /// <summary>
    ///     Sets the size of the dialog and follows the size of the parent window while the dialog is shown: subscribed
    ///     in Loaded and unsubscribed in Unloaded, so the parent window doesn't keep closed (or never shown) dialogs in
    ///     memory.
    /// </summary>
    /// <param name="dialog">Dialog.</param>
    /// <param name="parentWindow">Window that shows the dialog.</param>
    /// <param name="height">Also the height (otherwise only the width; the height follows the content).</param>
    public static void FollowParentSize(ChildWindow dialog, Window parentWindow, bool height)
    {
        Update();

        dialog.Loaded += (_, _) =>
        {
            parentWindow.SizeChanged -= ParentWindow_SizeChanged;
            parentWindow.SizeChanged += ParentWindow_SizeChanged;

            Update();
        };

        dialog.Unloaded += (_, _) => parentWindow.SizeChanged -= ParentWindow_SizeChanged;

        return;

        void ParentWindow_SizeChanged(object sender, SizeChangedEventArgs e) => Update();

        void Update()
        {
            dialog.ChildWindowWidth = parentWindow.ActualWidth * Factor;

            if (height)
                dialog.ChildWindowHeight = parentWindow.ActualHeight * Factor;
        }
    }
}
