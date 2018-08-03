using System;
using System.Diagnostics.CodeAnalysis;
using static NETworkManager.Models.PuTTY.PuTTY;

namespace NETworkManager.Models.Settings
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ProfileInfo
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public Guid CredentialID { get; set; } = Guid.Empty;
        public string Group { get; set; }
        public string Tags { get; set; }
        
        public bool NetworkInterface_Enabled { get; set; }
        public bool NetworkInterface_EnableStaticIPAddress { get; set; }
        public string NetworkInterface_IPAddress { get; set; }
        public string NetworkInterface_SubnetmaskOrCidr { get; set; }
        public string NetworkInterface_Gateway { get; set; }
        public bool NetworkInterface_EnableStaticDNS { get; set; }
        public string NetworkInterface_PrimaryDNSServer { get; set; }
        public string NetworkInterface_SecondaryDNSServer { get; set; }

        public bool IPScanner_Enabled { get; set; }
        public bool IPScanner_InheritHost { get; set; } = true;
        public string IPScanner_IPRange { get; set; }

        public bool PortScanner_Enabled { get; set; }
        public bool PortScanner_InheritHost { get; set; } = true;
        public string PortScanner_Host { get; set; }
        public string PortScanner_Ports { get; set; }

        public bool Ping_Enabled { get; set; }
        public bool Ping_InheritHost { get; set; } = true;
        public string Ping_Host { get; set; }

        public bool Traceroute_Enabled { get; set; }
        public bool Traceroute_InheritHost { get; set; } = true;
        public string Traceroute_Host { get; set; }

        public bool RemoteDesktop_Enabled { get; set; }
        public bool RemoteDesktop_InheritHost { get; set; } = true;
        public string RemoteDesktop_Host { get; set; }

        public bool PuTTY_Enabled { get; set; }
        public ConnectionMode PuTTY_ConnectionMode { get; set; }
        public bool PuTTY_InheritHost { get; set; } = true;
        public string PuTTY_HostOrSerialLine { get; set; }
        public int PuTTY_PortOrBaud { get; set; }
        public string PuTTY_Username { get; set; }
        public string PuTTY_Profile { get; set; }
        public string PuTTY_AdditionalCommandLine { get; set; }

        public bool WakeOnLAN_Enabled { get; set; }
        public string WakeOnLAN_MACAddress { get; set; }
        public string WakeOnLAN_Broadcast { get; set; }
        public int WakeOnLAN_Port { get; set; }

        public ProfileInfo()
        {

        }
    }
}
