using NETworkManager.Models.Lookup;
using System;
using System.Net;

namespace NETworkManager.Models.Network;

public class PortScannerPortScannedArgs : EventArgs
{
    public IPAddress IPAddress { get; set; }
    public string Hostname { get; set; }
    public int Port { get; set; }
    public PortLookupInfo LookupInfo { get; set; }
    public PortState State { get; set; }

    public PortScannerPortScannedArgs()
    {

    }

    public PortScannerPortScannedArgs(IPAddress ipAddres, string hostname, int port, PortLookupInfo lookupInfo, PortState state)
    {
        IPAddress = ipAddres;
        Hostname = hostname;
        Port = port;
        LookupInfo = lookupInfo;
        State = state;
    }
}
