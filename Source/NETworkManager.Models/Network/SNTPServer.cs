using System.Collections.Generic;

namespace NETworkManager.Models.Network
{
    /// <summary>
    /// Class provides static informations about SNTP servers.
    /// </summary>
    public static class SNTPServer
    {
        /// <summary>
        /// Method will return a default list of common SNTP servers.
        /// </summary>
        /// <return
        public static List<ServerInfoProfile> GetDefaultList() => new()
        {
            new ServerInfoProfile("Cloudflare", new()
            {
                new ServerInfo("time.cloudflare.com", 123)
            }),
            new ServerInfoProfile("Google Public NTP", new()
            {
                new ServerInfo("time.google.com", 123),
                new ServerInfo("time1.google.com", 123),
                new ServerInfo("time2.google.com", 123),
                new ServerInfo ("time3.google.com", 123),
                new ServerInfo("time4.google.com", 123)
            }),
            new ServerInfoProfile("Microsoft", new()
            {
                new ServerInfo("time.windows.com", 123)
            }),
            new ServerInfoProfile("pool.ntp.org", new()
            {
                new ServerInfo("0.pool.ntp.org", 123),
                new ServerInfo("1.pool.ntp.org", 123),
                new ServerInfo("2.pool.ntp.org", 123),
                new ServerInfo("3.pool.ntp.org", 123)
            })
        };
    }
}
