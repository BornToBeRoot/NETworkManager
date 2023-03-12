namespace NETworkManager.Models.Network;

/// <summary>
/// Class contains the options for the port scanner.
/// </summary>
public class PortScannerOptions
{
    /// <summary>
    /// Max cuncurrent threads for the host scan.
    /// </summary>
    public int MaxHostThreads { get; set; }

    /// <summary>
    /// Max cuncurrent threads for the port scan of each host.
    /// </summary>
    public int MaxPortThreads { get; set; }

    /// <summary>
    /// Timeout in ms after which the port is considered closed.
    /// </summary>
    public int Timeout { get; set; }

    /// <summary>
    /// Resolve the hostname for an IP address.
    /// </summary>
    public bool ResolveHostname { get; set; }

    /// <summary>
    /// Include closed ports in the result.
    /// </summary>
    public bool ShowAllResults { get; set; }

    /// <summary>
    /// Create an instance of <see cref="PortScannerOptions"/> with parameters.
    /// </summary>
    /// <param name="maxHostThreads">Max cuncurrent threads for the host scan.</param>
    /// <param name="maxPortThreads">Max cuncurrent threads for the port scan of each host.</param>
    /// <param name="timeout">Timeout in ms after which the port is considered closed.</param>
    /// <param name="resolveHostname">Resolve the hostname for an IP address.</param>
    /// <param name="showAllResults">Include closed ports in the result.</param>
    public PortScannerOptions(int maxHostThreads, int maxPortThreads, int timeout, bool resolveHostname, bool showAllResults)
    {
        MaxHostThreads = maxHostThreads;
        MaxPortThreads = maxPortThreads;
        Timeout = timeout;
        ResolveHostname = resolveHostname;
        ShowAllResults = showAllResults;
    }
}
