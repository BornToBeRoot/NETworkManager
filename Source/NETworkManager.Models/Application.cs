using System;

namespace NETworkManager.Models
{
    public static class Application
    {
        /// <summary>
        /// Method to return all available applications in.
        /// </summary>
        /// <returns>All names as array.</returns>
        public static Name[] GetNames() => (Name[])Enum.GetValues(typeof(Name));

        /// <summary>
        /// Enum represents all available applications.
        /// </summary>
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
