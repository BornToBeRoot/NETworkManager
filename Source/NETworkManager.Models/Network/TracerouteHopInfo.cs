using System.Net;
using System.Net.NetworkInformation;
using NETworkManager.Models.IPApi;

namespace NETworkManager.Models.Network;

/// <summary>
/// Class containing information about a hop in a traceroute.
/// </summary>
public class TracerouteHopInfo
{
    /// <summary>
    /// Hop (router).
    /// </summary>
    public int Hop { get; set; }

    /// <summary>
    /// Status of the first ping.
    /// </summary>
    public IPStatus Status1 { get; set; }

    /// <summary>
    /// Time (ms) of the first ping.
    /// </summary>
    public long Time1 { get; set; }

    /// <summary>
    /// Status of the second ping.
    /// </summary>
    public IPStatus Status2 { get; set; }

    /// <summary>
    /// Time (ms) of the second ping.
    /// </summary>
    public long Time2 { get; set; }

    /// <summary>
    /// Status of the third ping.
    /// </summary>
    public IPStatus Status3 { get; set; }

    /// <summary>
    /// Time (ms) of the third ping.
    /// </summary>
    public long Time3 { get; set; }

    /// <summary>
    /// IP address of the hop (router).
    /// </summary>
    public IPAddress IPAddress { get; set; }

    /// <summary>
    /// Hostname of the hop (router).
    /// </summary>
    public string Hostname { get; set; }

    /// <summary>
    /// IP geolocation result (and info) of the hop (router).
    /// </summary>
    public IPGeolocationResult IPGeolocationResult { get; set; }

    /// <summary>
    /// Creates a new instance of the <see cref="TracerouteHopInfo"/> class with the specified parameters.
    /// </summary>
    /// <param name="hop">Hop (router).</param>
    /// <param name="status1">Status of the first ping.</param>
    /// <param name="time1">Time (ms) of the first ping.</param>
    /// <param name="status2">Status of the second ping.</param>
    /// <param name="time2">Time (ms) of the second ping.</param>
    /// <param name="status3">Status of the third ping.</param>
    /// <param name="time3">Time (ms) of the third ping.</param>
    /// <param name="ipAddress">IP address of the hop (router).</param>
    /// <param name="hostname">Hostname of the hop (router).</param>
    /// <param name="ipGeolocationResult">IP geolocation result (and info) of the hop (router).</param>
    public TracerouteHopInfo(int hop, IPStatus status1, long time1, IPStatus status2, long time2, IPStatus status3,
        long time3, IPAddress ipAddress, string hostname, IPGeolocationResult ipGeolocationResult)
    {
        Hop = hop;
        Status1 = status1;
        Time1 = time1;
        Status2 = status2;
        Time2 = time2;
        Status3 = status3;
        Time3 = time3;
        IPAddress = ipAddress;
        Hostname = hostname;
        IPGeolocationResult = ipGeolocationResult;
    }
}