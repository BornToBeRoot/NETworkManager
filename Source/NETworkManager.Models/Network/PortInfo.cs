using NETworkManager.Models.Lookup;

namespace NETworkManager.Models.Network;

public partial class PortInfo
{
    
    public int Port { get; set; }
    public PortLookupInfo LookupInfo { get; set; }
    public PortState State { get; set; }
        
    public PortInfo()
    {

    }

    public PortInfo(int port, PortLookupInfo lookupInfo, PortState status)
    {   
        Port = port;
        LookupInfo = lookupInfo;
        State = status;
    }
}
