namespace NETworkManager.Models.Network;

/// <summary>
///     Target of the latency widget of the dashboard (Settings > Dashboard > Latency). The default gateway is not a
///     target of the list, see the setting <c>Dashboard_LatencyPingGateway</c>.
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
    /// <param name="host">IP address or host name.</param>
    public LatencyTargetInfo(string name, string host)
    {
        Name = name;
        Host = host;
    }

    /// <summary>
    ///     Name shown in the widget, empty to show the host.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///     IP address or host name of the target.
    /// </summary>
    public string Host { get; set; }

    /// <summary>
    ///     Same name and host (the widget keeps the pings of an unchanged target when the list changes).
    /// </summary>
    /// <param name="other">Other target.</param>
    /// <returns><c>true</c> if all values are the same.</returns>
    public bool IsSameTarget(LatencyTargetInfo other)
    {
        return other != null && Name == other.Name && Host == other.Host;
    }
}
