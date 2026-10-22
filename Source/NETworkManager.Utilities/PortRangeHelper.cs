using System.Collections.Generic;
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

    public static Task<int[]> ConvertPortRangeToIntArrayAsync(string portRange)
    {
        return Task.Run(() => ConvertPortRangeToIntArray(portRange));
    }

    // Generate from a string like "9; 11-13; 15" an list with "9,11,12,13,15"
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

        return list.ToArray();
    }
}