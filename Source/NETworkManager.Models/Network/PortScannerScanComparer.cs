using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace NETworkManager.Models.Network;

/// <summary>
///     Result of <see cref="PortScannerScanComparer.Compare" />.
/// </summary>
/// <param name="Opened">Ports that are open now and were not open in the previous scan.</param>
/// <param name="Closed">Ports of the result that were open in the previous scan and are not open now.</param>
/// <param name="Missing">
///     Ports that were open in the previous scan and are not in the result (e.g. the host didn't answer the ping
///     before the scan), with the hostname of the previous scan.
/// </param>
public sealed record PortScannerComparison(
    IReadOnlySet<(IPAddress IPAddress, int Port)> Opened,
    IReadOnlySet<(IPAddress IPAddress, int Port)> Closed,
    IReadOnlyList<(IPAddress IPAddress, int Port, string Hostname)> Missing);

/// <summary>
///     Compares a port scan with the previous scan of the same hosts and ports: which ports are open now or no longer
///     open. Only the open ports of a scan are kept (closed ports may not be in the result).
/// </summary>
public static class PortScannerScanComparer
{
    /// <summary>
    ///     Creates the baseline of a completed scan: its open ports with the hostname.
    /// </summary>
    /// <param name="ports">Ports of the result.</param>
    /// <returns>Open ports and their hostname.</returns>
    public static Dictionary<(IPAddress IPAddress, int Port), string> CreateBaseline(
        IEnumerable<PortScannerPortInfo> ports)
    {
        return ports.Where(x => x.State == PortState.Open)
            .GroupBy(x => (x.IPAddress, x.Port))
            .ToDictionary(x => x.Key, x => x.First().Hostname ?? string.Empty);
    }

    /// <summary>
    ///     Compares the result of a scan with the baseline of the previous one.
    /// </summary>
    /// <param name="baseline">Open ports of the previous scan (see <see cref="CreateBaseline" />).</param>
    /// <param name="ports">Ports of the current result.</param>
    /// <returns>Opened, closed and missing ports.</returns>
    public static PortScannerComparison Compare(IReadOnlyDictionary<(IPAddress IPAddress, int Port), string> baseline,
        IEnumerable<PortScannerPortInfo> ports)
    {
        var opened = new HashSet<(IPAddress, int)>();
        var closed = new HashSet<(IPAddress, int)>();
        var seen = new HashSet<(IPAddress, int)>();

        foreach (var port in ports)
        {
            var key = (port.IPAddress, port.Port);
            seen.Add(key);

            var wasOpen = baseline.ContainsKey(key);

            if (port.State == PortState.Open && !wasOpen)
                opened.Add(key);
            else if (port.State != PortState.Open && wasOpen)
                closed.Add(key);
        }

        var missing = baseline.Where(x => !seen.Contains(x.Key))
            .Select(x => (x.Key.IPAddress, x.Key.Port, x.Value))
            .ToList();

        return new PortScannerComparison(opened, closed, missing);
    }
}
