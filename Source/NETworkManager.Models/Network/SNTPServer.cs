using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace NETworkManager.Models.Network
{
    public static class SNTPServer
    {
        public static List<SNTPServerInfo> GetDefaultList() => new()
        {
            new SNTPServerInfo("Cloudflare", new()
            {
                ("time.cloudflare.com", 123)
            }),
            new SNTPServerInfo("Google Public NTP", new()
            {
                ("time.google.com", 123),
                ("time1.google.com", 123),
                ("time2.google.com", 123),
                ("time3.google.com", 123),
                ("time4.google.com", 123)
            }),            
            new SNTPServerInfo("Microsoft", new()
            {
                ("time.windows.com", 123)
            }),
            new SNTPServerInfo("NIST", new ()
            {
                ("time.nist.gov", 123)
            }),
            new SNTPServerInfo("pool.ntp.org", new()
            {
                ("0.pool.ntp.org", 123),
                ("1.pool.ntp.org", 123),
                ("2.pool.ntp.org", 123),
                ("3.pool.ntp.org", 123)
            })            
        };
    }
}
