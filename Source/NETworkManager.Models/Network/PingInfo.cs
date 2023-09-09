using System;
using System.Net;
using System.Net.NetworkInformation;

namespace NETworkManager.Models.Network;

/// <summary>
/// Class containing the information of a ping.
/// </summary>
public class PingInfo
{
    /// <summary>
    /// Timestamp when the ping was sent.
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// IP address of the host.
    /// </summary>
    public IPAddress IPAddress { get; set; }
    
    /// <summary>
    /// Hostname of the host.
    /// </summary>
    public string Hostname { get; set; }
    
    /// <summary>
    /// Bytes used to send the icmp packet.
    /// </summary>
    public int Bytes { get; set; }
    
    /// <summary>
    /// Time in milliseconds how long it took to receive a response.
    /// </summary>
    public long Time { get; set; }
    
    /// <summary>
    /// Time to live of the icmp packet.
    /// </summary>
    public int TTL { get; set; }
    
    /// <summary>
    /// IP status of the ping.
    /// </summary>
    public IPStatus Status { get; set; }

    /// <summary>
    /// IP address as an integer.
    /// </summary>
    public int IPAddressInt32 => IPAddress is { AddressFamily: System.Net.Sockets.AddressFamily.InterNetwork } ? IPv4Address.ToInt32(IPAddress) : 0;

    /// <summary>
    /// Creates a new instance of <see cref="PingInfo"/>.
    /// </summary>
    public PingInfo()
    {

    }

    /// <summary>
    /// Creates a new instance of <see cref="PingInfo"/> with the specified parameters.
    /// </summary>
    /// <param name="timestamp">Timestamp when the ping was sent.</param>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="status">IP status of the ping.</param>
    public PingInfo(DateTime timestamp, IPAddress ipAddress, IPStatus status)
    {
        Timestamp = timestamp;
        IPAddress = ipAddress;
        Status = status;
    }

    /// <summary>
    /// Creates a new instance of <see cref="PingInfo"/> with the specified parameters.
    /// </summary>
    /// <param name="timestamp">Timestamp when the ping was sent.</param>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="hostname">Hostname of the host.</param>
    /// <param name="status">IP status of the ping.</param>
    public PingInfo(DateTime timestamp, IPAddress ipAddress, string hostname, IPStatus status)
    {
        Timestamp = timestamp;
        IPAddress = ipAddress;
        Hostname = hostname;
        Status = status;
    }
    
    /// <summary>
    /// Creates a new instance of <see cref="PingInfo"/> with the specified parameters.
    /// </summary>
    /// <param name="timestamp">Timestamp when the ping was sent.</param>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="bytes">Bytes used to send the icmp packet.</param>
    /// <param name="time">Time in milliseconds how long it took to receive a response.</param>
    /// <param name="status">IP status of the ping.</param>
    public PingInfo(DateTime timestamp, IPAddress ipAddress, int bytes, long time, IPStatus status)
    {
        Timestamp = timestamp;
        IPAddress = ipAddress;
        Bytes = bytes;
        Time = time;
        Status = status;
    }

    /// <summary>
    /// Creates a new instance of <see cref="PingInfo"/> with the specified parameters.
    /// </summary>
    /// <param name="timestamp">Timestamp when the ping was sent.</param>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="hostname">Hostname of the host.</param>
    /// <param name="bytes">Bytes used to send the icmp packet.</param>
    /// <param name="time">Time in milliseconds how long it took to receive a response.</param>
    /// <param name="status">IP status of the ping.</param>
    public PingInfo(DateTime timestamp, IPAddress ipAddress, string hostname, int bytes, long time, IPStatus status)
    {
        Timestamp = timestamp;
        IPAddress = ipAddress;
        Hostname = hostname;
        Bytes = bytes;
        Time = time;
        Status = status;
    }

    /// <summary>
    /// Creates a new instance of <see cref="PingInfo"/> with the specified parameters.
    /// </summary>
    /// <param name="timestamp">Timestamp when the ping was sent.</param>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="bytes">Bytes used to send the icmp packet.</param>
    /// <param name="time">Time in milliseconds how long it took to receive a response.</param>
    /// <param name="ttl">Time to live of the icmp packet.</param>
    /// <param name="status">IP status of the ping.</param>
    public PingInfo(DateTime timestamp, IPAddress ipAddress, int bytes, long time, int ttl, IPStatus status)
    {
        Timestamp = timestamp;
        IPAddress = ipAddress;
        Bytes = bytes;
        Time = time;
        TTL = ttl;
        Status = status;
    }

    /// <summary>
    /// Creates a new instance of <see cref="PingInfo"/> with the specified parameters.
    /// </summary>
    /// <param name="timestamp">Timestamp when the ping was sent.</param>
    /// <param name="ipAddress">IP address of the host.</param>
    /// <param name="hostname">Hostname of the host.</param>
    /// <param name="bytes">Bytes used to send the icmp packet.</param>
    /// <param name="time">Time in milliseconds how long it took to receive a response.</param>
    /// <param name="ttl">Time to live of the icmp packet.</param>
    /// <param name="status">IP status of the ping.</param>
    public PingInfo(DateTime timestamp, IPAddress ipAddress, string hostname, int bytes, long time, int ttl, IPStatus status)
    {
        Timestamp = timestamp;
        IPAddress = ipAddress;
        Hostname = hostname;
        Bytes = bytes;
        Time = time;
        TTL = ttl;
        Status = status;
    }
}