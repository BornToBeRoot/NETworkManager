namespace NETworkManager.Models.Network;

/// <summary>
///     Class contains the options for the port scanner.
/// </summary>
public class PortScannerOptions
{
    /// <summary>
    ///     Adjust the number of hosts and ports scanned at the same time to the scan (see
    ///     <see cref="PortScanner" />) instead of using <see cref="MaxHostThreads" /> and <see cref="MaxPortThreads" />.
    /// </summary>
    public required bool AutoAdjustThreads { get; init; }

    /// <summary>
    ///     Max concurrent threads for the host scan (if the threads are not adjusted automatically).
    /// </summary>
    public required int MaxHostThreads { get; init; }

    /// <summary>
    ///     Max concurrent threads for the port scan of each host (if the threads are not adjusted automatically).
    /// </summary>
    public required int MaxPortThreads { get; init; }

    /// <summary>
    ///     Timeout in ms after which the port is considered closed.
    /// </summary>
    public required int Timeout { get; init; }

    /// <summary>
    ///     Number of attempts to connect to a port if there was no answer (timed out).
    /// </summary>
    public required int Attempts { get; init; }

    /// <summary>
    ///     Ping every host before its ports are scanned (with <see cref="Timeout" /> and <see cref="Attempts" />), a host
    ///     that doesn't answer is not scanned.
    /// </summary>
    public required bool PingHostsBeforeScan { get; init; }

    /// <summary>
    ///     Resolve the hostname for an IP address.
    /// </summary>
    public required bool ResolveHostname { get; init; }

    /// <summary>
    ///     Include closed ports in the result.
    /// </summary>
    public required bool ShowAllResults { get; init; }
}
