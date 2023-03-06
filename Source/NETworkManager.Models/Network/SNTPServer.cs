using System.Collections.Generic;

namespace NETworkManager.Models.Network;

/// <summary>
/// Class provides static informations about SNTP servers.
/// </summary>
public static class SNTPServer
{
    /// <summary>
    /// Method will return a default list of common SNTP servers.
    /// </summary>
    /// <return
    public static List<ServerConnectionInfoProfile> GetDefaultList() => new()
    {
        new ServerConnectionInfoProfile("Cloudflare", new()
        {
            new ServerConnectionInfo("time.cloudflare.com", 123)
        }),
        new ServerConnectionInfoProfile("Google Public NTP", new()
        {
            new ServerConnectionInfo("time.google.com", 123),
            new ServerConnectionInfo("time1.google.com", 123),
            new ServerConnectionInfo("time2.google.com", 123),
            new ServerConnectionInfo ("time3.google.com", 123),
            new ServerConnectionInfo("time4.google.com", 123)
        }),
        new ServerConnectionInfoProfile("Microsoft", new()
        {
            new ServerConnectionInfo("time.windows.com", 123)
        }),
        new ServerConnectionInfoProfile("pool.ntp.org", new()
        {
            new ServerConnectionInfo("0.pool.ntp.org", 123),
            new ServerConnectionInfo("1.pool.ntp.org", 123),
            new ServerConnectionInfo("2.pool.ntp.org", 123),
            new ServerConnectionInfo("3.pool.ntp.org", 123)
        })
    };
}
