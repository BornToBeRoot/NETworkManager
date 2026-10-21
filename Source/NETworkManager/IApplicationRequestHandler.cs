namespace NETworkManager;

/// <summary>
///     Application view that can handle an <see cref="ApplicationRequest" /> with a target or a profile (e.g. open a new
///     tab and start a scan). Views without this interface are only opened.
/// </summary>
public interface IApplicationRequestHandler
{
    /// <summary>
    ///     Handles the request. Called after the application view is shown.
    /// </summary>
    /// <param name="request">Request with a target or a profile.</param>
    void Open(ApplicationRequest request);
}
