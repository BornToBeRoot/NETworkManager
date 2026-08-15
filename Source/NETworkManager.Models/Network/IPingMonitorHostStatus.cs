namespace NETworkManager.Models.Network;

/// <summary>
///     Minimal reachability/running status of a Ping Monitor host, exposed so lower-level
///     projects (e.g. converters) can read a host's status without depending on the concrete
///     View/ViewModel types that implement it.
/// </summary>
public interface IPingMonitorHostStatus
{
    /// <summary>
    ///     Gets a value indicating whether the host is reachable (responds to ping).
    /// </summary>
    bool IsReachable { get; }

    /// <summary>
    ///     Gets a value indicating whether the ping monitoring is currently running.
    /// </summary>
    bool IsRunning { get; }
}
