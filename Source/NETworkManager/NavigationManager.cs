using NETworkManager.Models;
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
    ///     Opens an application and passes the request (target, profile, options) to it. The target is prepared for
    ///     the application (see <see cref="ApplicationManager.GetHostForApplication" />), e.g. "gateway." from a PTR
    ///     record -> "gateway" for Remote Desktop.
    /// </summary>
    /// <param name="request">Request with the application to open.</param>
    public static void Open(ApplicationRequest request)
    {
        if (request.Target != null)
            request = request with
            {
                Target = ApplicationManager.GetHostForApplication(request.Application, request.Target)
            };

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
