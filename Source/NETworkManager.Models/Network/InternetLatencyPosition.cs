namespace NETworkManager.Models.Network;

/// <summary>
///     Position of the internet latency widget on the dashboard.
/// </summary>
public enum InternetLatencyPosition
{
    /// <summary>
    ///     Right next to the speed test (1/3 of the width, the targets one below the other). In a narrow window (or
    ///     without the speed test) it gets its own row below the speed test.
    /// </summary>
    NextToSpeedTest,

    /// <summary>
    ///     Own row below the network connection (the targets side by side).
    /// </summary>
    OwnRow
}
