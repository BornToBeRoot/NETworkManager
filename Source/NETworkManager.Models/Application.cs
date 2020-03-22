using System;

namespace NETworkManager.Models
{
    public static class Application
    {
        public static Name[] GetNames() => (Name[])Enum.GetValues(typeof(Name));

        public enum Name
        {
            None,
            Dashboard,
            NetworkInterface,
            WiFi,
            IPScanner,
            PortScanner,
            Ping,
            PingMonitor,
            Traceroute,
            DNSLookup,
            RemoteDesktop,
            PowerShell,
            PuTTY,
            TigerVNC,
            WebConsole,
            SNMP,
            DiscoveryProtocol,
            WakeOnLAN,
            HTTPHeaders,
            Whois,
            SubnetCalculator,
            Lookup,
            //Routing,
            Connections,
            Listeners,
            ARPTable
        }
    }
}
