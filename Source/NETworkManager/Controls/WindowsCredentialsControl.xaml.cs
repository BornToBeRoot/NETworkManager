namespace NETworkManager.Controls;

/// <summary>
///     Windows credentials of a profile or group in the profile and group dialog (Remote Desktop, IP Scanner), the
///     DataContext is a <see cref="ViewModels.WindowsCredentialsViewModel" />.
/// </summary>
public partial class WindowsCredentialsControl
{
    public WindowsCredentialsControl()
    {
        InitializeComponent();

        // Invalid input of the text boxes: the dialog can't be saved
        InputErrorTracker.Track(this);
    }
}
