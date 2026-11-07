using System;
using System.Windows.Input;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

/// <summary>
///     Windows credentials of an IP Scanner tab ("Set Windows credentials..." in the options, "Scan as..." of a profile),
///     see <see cref="Views.WindowsCredentialsChildWindow" />. Only kept in memory, never saved.
/// </summary>
public class WindowsCredentialsChildWindowViewModel : ViewModelBase
{
    /// <summary>
    ///     Creates the dialog with the current credentials.
    /// </summary>
    /// <param name="okCommand">Called with the dialog on OK.</param>
    /// <param name="cancelHandler">Called with the dialog on Cancel.</param>
    /// <param name="credentials">Current credentials (the password stays unless a new one is entered), null for the
    /// user running the application.</param>
    public WindowsCredentialsChildWindowViewModel(Action<WindowsCredentialsChildWindowViewModel> okCommand,
        Action<WindowsCredentialsChildWindowViewModel> cancelHandler, WindowsCredentials credentials)
    {
        _credentials = credentials;

        OKCommand = new RelayCommand(_ => okCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        // Not saved: no warning about a profile file that is not encrypted
        Credentials = new WindowsCredentialsViewModel(credentials != null, credentials?.Username,
            credentials?.Domain, credentials?.Password, true);
    }

    // Credentials the dialog was opened with
    private readonly WindowsCredentials _credentials;

    public ICommand OKCommand { get; }

    public ICommand CancelCommand { get; }

    /// <summary>
    ///     Username, domain and password ("Use credentials" off: the user running the application).
    /// </summary>
    public WindowsCredentialsViewModel Credentials { get; }

    /// <summary>
    ///     Gets the entered credentials (copy of the password), null for the user running the application - the
    ///     credentials the dialog was opened with if nothing was changed (e.g. the source of "Scan as..." is kept).
    /// </summary>
    public WindowsCredentials ToCredentials()
    {
        if (IsUnchanged())
            return _credentials;

        return Credentials.UseCredentials
            ? new WindowsCredentials(Credentials.Username?.Trim() ?? string.Empty,
                Credentials.Domain?.Trim() ?? string.Empty, Credentials.Password?.Copy())
            : null;
    }

    /// <summary>
    ///     Disposes the password entered in the dialog (after <see cref="ToCredentials" /> copied it, or on Cancel) -
    ///     not the password of the credentials the dialog was opened with (owned by the caller).
    /// </summary>
    public void ReleaseEnteredPassword()
    {
        if (!ReferenceEquals(Credentials.Password, _credentials?.Password))
            Credentials.Password?.Dispose();
    }

    /// <summary>
    ///     Nothing was changed: still used or not used, same username / domain and no new password.
    /// </summary>
    private bool IsUnchanged()
    {
        if (_credentials == null)
            return !Credentials.UseCredentials;

        return Credentials.UseCredentials &&
               (Credentials.Username?.Trim() ?? string.Empty) == _credentials.Username &&
               (Credentials.Domain?.Trim() ?? string.Empty) == _credentials.Domain &&
               ReferenceEquals(Credentials.Password, _credentials.Password);
    }
}
