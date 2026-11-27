namespace NETworkManager.Models.Network;

/// <summary>
///     Target of the latency widget of the dashboard (Settings > Dashboard > Latency).
/// </summary>
public class LatencyTargetInfo
{
    /// <summary>
    ///     Creates an empty target (JSON deserialization of the settings).
    /// </summary>
    public LatencyTargetInfo()
    {
    }

    /// <summary>
    ///     Creates a target.
    /// </summary>
    /// <param name="name">Name shown in the widget, empty to show the host.</param>
    /// <param name="host">IP address or host name (not used for the gateway).</param>
    /// <param name="isGateway">Ping the default gateway of the network instead of a host.</param>
    public LatencyTargetInfo(string name, string host, bool isGateway = false)
    {
        Name = name;
        Host = host;
        IsGateway = isGateway;
    }

    /// <summary>
    ///     Name shown in the widget, empty to show the host.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     IP address or host name of the target (not used for the gateway).
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    ///     Ping the current default gateway (detected when the pings start, e.g. after a network change) instead of
    ///     <see cref="Host" />, like the "[Windows DNS]" entry of the DNS servers.
    /// </summary>
    public bool IsGateway { get; set; }

    /// <summary>
    ///     Same name, host and gateway option (the widget keeps the pings of an unchanged target when the list changes).
    /// </summary>
    /// <param name="other">Other target.</param>
    /// <returns><c>true</c> if all values are the same.</returns>
    public bool IsSameTarget(LatencyTargetInfo other)
    {
        return other != null && Name == other.Name && Host == other.Host && IsGateway == other.IsGateway;
    }
}
