using System.Collections.Generic;

namespace NETworkManager.Models.Network;

/// <summary>
///     Class provides static informations about SNTP servers.
/// </summary>
public static class SNTPServer
{
    /// <summary>
    ///     Method will return a default list of common SNTP servers.
    /// </summary>
    /// <return
    public static List<ServerConnectionInfoProfile> GetDefaultList()
    {
        return new List<ServerConnectionInfoProfile>
        {
            new("Cloudflare", new List<ServerConnectionInfo>
            {
                new("time.cloudflare.com", 123)
            }),
            new("Google Public NTP", new List<ServerConnectionInfo>
            {
                new("time.google.com", 123),
                new("time1.google.com", 123),
                new("time2.google.com", 123),
                new("time3.google.com", 123),
                new("time4.google.com", 123)
            }),
            new("Microsoft", new List<ServerConnectionInfo>
            {
                new("time.windows.com", 123)
            }),
            new("pool.ntp.org", new List<ServerConnectionInfo>
            {
                new("0.pool.ntp.org", 123),
                new("1.pool.ntp.org", 123),
                new("2.pool.ntp.org", 123),
                new("3.pool.ntp.org", 123)
            })
        };
    }
}