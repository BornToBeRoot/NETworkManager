namespace NETworkManager.Models
{
    /// <summary>
    /// Represents all available applications.
    /// </summary>
    public enum ApplicationName
    {
        /// <summary>
        /// No application.
        /// </summary>
        None,

        /// <summary>
        /// Dashboard application.
        /// </summary>            
        Dashboard,

        /// <summary>
        ///  Network interface application.
        /// </summary>
        NetworkInterface,

        /// <summary>
        ///  WiFi application.
        /// </summary>
        WiFi,

        /// <summary>
        /// IP scanner application.
        /// </summary>
        IPScanner,

        /// <summary>
        /// Port scanner application.
        /// </summary>
        PortScanner,

        /// <summary>
        /// Ping monitor application.
        /// </summary>
        PingMonitor,

        /// <summary>
        /// Traceroute application.
        /// </summary>
        Traceroute,

        /// <summary>
        /// DNS lookup application.
        /// </summary>
        DNSLookup,

        /// <summary>
        /// Remote Desktop application.
        /// </summary>
        RemoteDesktop,

        /// <summary>
        /// PowerShell application.
        /// </summary>
        PowerShell,

        /// <summary>
        /// PuTTY application.
        /// </summary>
        PuTTY,

        /// <summary>
        /// AWS Systems Manager Session Manager.
        /// </summary>
        AWSSessionManager,

        /// <summary>
        /// TigerVNC application.
        /// </summary>
        TigerVNC,

        /// <summary>
        /// WebConsole application.
        /// </summary>
        WebConsole,
                      
        /// <summary>
        /// SNMP application.
        /// </summary>
        SNMP,
        
        /// <summary>
        /// NTP lookup application.
        /// </summary>
        NTPLookup,
        
        /// <summary>
        /// Discovery protocol application.
        /// </summary>
        DiscoveryProtocol,

        /// <summary>
        /// Wake on LAN application.
        /// </summary>
        WakeOnLAN,

        /// <summary>
        /// Whois application.
        /// </summary>
        Whois,

        /// <summary>
        /// Subnet calculator application.
        /// </summary>
        SubnetCalculator,

        /// <summary>
        /// Bit calculator application.
        /// </summary>
        BitCalculator,

        /// <summary>
        /// Lookup application.
        /// </summary>
        Lookup,

        /// <summary>
        /// Connections application.
        /// </summary>
        Connections,

        /// <summary>
        /// Listeners application.
        /// </summary>
        Listeners,

        /// <summary>
        /// ARP table application.
        /// </summary>
        ARPTable
    }
}