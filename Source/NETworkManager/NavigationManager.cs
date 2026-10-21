using System;

namespace NETworkManager;

/// <summary>
///     Navigation between the applications (tools) and the settings. The main window handles the requests.
/// </summary>
public static class NavigationManager
{
    /// <summary>
    ///     Occurs when an application should be opened, see <see cref="Open" />.
    /// </summary>
    public static event EventHandler<ApplicationRequest> ApplicationRequested;

    /// <summary>
    ///     Occurs when the settings should be opened, see <see cref="OpenSettings" />.
    /// </summary>
    public static event EventHandler SettingsRequested;

    /// <summary>
    ///     Opens an application and passes the request (target, profile, options) to it.
    /// </summary>
    /// <param name="request">Request with the application to open.</param>
    public static void Open(ApplicationRequest request)
    {
        ApplicationRequested?.Invoke(null, request);
    }

    /// <summary>
    ///     Opens the settings of the current application.
    /// </summary>
    public static void OpenSettings()
    {
        SettingsRequested?.Invoke(null, EventArgs.Empty);
    }
}
