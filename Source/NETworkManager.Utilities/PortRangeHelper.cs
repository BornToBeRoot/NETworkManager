using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NETworkManager.Utilities;

public static class PortRangeHelper
{
    /// <summary>
    ///     Checks if the port is valid (1 to 65535).
    /// </summary>
    /// <param name="port">Port.</param>
    /// <returns>True if the port is valid.</returns>
    public static bool IsValidPort(int port)
    {
        return port is > 0 and < 65536;
    }

    /// <summary>
    ///     Checks if the port range is valid: both ports are valid and the range goes from a lower to a higher port.
    /// </summary>
    /// <param name="startPort">First port.</param>
    /// <param name="endPort">Last port.</param>
    /// <returns>True if the range is valid.</returns>
    public static bool IsValidPortRange(int startPort, int endPort)
    {
        return IsValidPort(startPort) && IsValidPort(endPort) && startPort < endPort;
    }

    /// <summary>
    ///     Checks if the value is a port (e.g. "80") or a port range (e.g. "1-1024"), spaces are ignored.
    /// </summary>
    /// <param name="value">Value to check.</param>
    /// <returns>True if it is a valid port or port range.</returns>
    public static bool IsValidPortOrPortRange(string value)
    {
        var parts = value.Replace(" ", "").Split('-');

        return parts.Length switch
        {
            1 => int.TryParse(parts[0], out var port) && IsValidPort(port),
            2 => int.TryParse(parts[0], out var startPort) && int.TryParse(parts[1], out var endPort) &&
                 IsValidPortRange(startPort, endPort),
            _ => false
        };
    }

    public static Task<int[]> ConvertPortRangeToIntArrayAsync(string portRange)
    {
        return Task.Run(() => ConvertPortRangeToIntArray(portRange));
    }

    // Generate from a string like "9; 11-13; 15" an list with "9,11,12,13,15". Ports entered multiple times
    // (e.g. "80; 1-100") are only added once (first occurrence, order is kept)
    public static int[] ConvertPortRangeToIntArray(string portRange)
    {
        var list = new List<int>();

        foreach (var portOrRange in portRange.Replace(" ", "").Split(';'))
            if (portOrRange.Contains('-'))
            {
                var portRangeSplit = portOrRange.Split('-');

                for (var i = int.Parse(portRangeSplit[0]); i < int.Parse(portRangeSplit[1]) + 1; i++) list.Add(i);
            }
            else
            {
                list.Add(int.Parse(portOrRange));
            }

        return list.Distinct().ToArray();
    }
}