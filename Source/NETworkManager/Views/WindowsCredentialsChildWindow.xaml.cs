using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.ViewModels;

namespace NETworkManager.Views;

/// <summary>
///     Windows credentials of an IP Scanner tab, the DataContext is a <see cref="WindowsCredentialsChildWindowViewModel" />.
/// </summary>
public partial class WindowsCredentialsChildWindow
{
    public WindowsCredentialsChildWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    ///     Shows the dialog (options of an IP Scanner tab, "Scan as..." of a profile).
    /// </summary>
    /// <param name="window">Window to show the dialog in.</param>
    /// <param name="title">Title, e.g. "Windows credentials" or "Scan as".</param>
    /// <param name="credentials">Current credentials, null for the user running the application.</param>
    /// <returns>
    ///     OK: true with the entered credentials (null for the user running the application); Cancel: false.
    /// </returns>
    public static async Task<(bool isOK, WindowsCredentials credentials)> ShowAsync(Window window, string title,
        WindowsCredentials credentials)
    {
        var childWindow = new WindowsCredentialsChildWindow { Title = title };

        var result = new TaskCompletionSource<(bool, WindowsCredentials)>();

        childWindow.DataContext = new WindowsCredentialsChildWindowViewModel(instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            // The credentials have a copy of an entered password
            var entered = instance.ToCredentials();
            instance.ReleaseEnteredPassword();

            result.TrySetResult((true, entered));
        }, instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            instance.ReleaseEnteredPassword();

            result.TrySetResult((false, null));
        }, credentials);

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await window.ShowChildWindowAsync(childWindow);

        return await result.Task;
    }
}
