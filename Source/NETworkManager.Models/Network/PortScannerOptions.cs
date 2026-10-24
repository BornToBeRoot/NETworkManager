namespace NETworkManager.Models.Network;

/// <summary>
///     Class contains the options for the port scanner.
/// </summary>
public class PortScannerOptions
{
    /// <summary>
    ///     Max concurrent threads for the host scan.
    /// </summary>
    public required int MaxHostThreads { get; init; }

    /// <summary>
    ///     Max concurrent threads for the port scan of each host.
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
    ///     Resolve the hostname for an IP address.
    /// </summary>
    public required bool ResolveHostname { get; init; }

    /// <summary>
    ///     Include closed ports in the result.
    /// </summary>
    public required bool ShowAllResults { get; init; }
}
