using System.Net;

namespace NETworkManager.Models.Network;

public class SNTPLookupResultArgs : System.EventArgs
{        
    public string Server { get; set; }
    public string IPEndPoint { get; set; }
    public SNTPDateTime DateTime { get; set; }        

    public SNTPLookupResultArgs()
    {

    }

    public SNTPLookupResultArgs(string server, string ipEndPoint, SNTPDateTime dateTime)
    {
        Server = server;
        IPEndPoint = ipEndPoint;
        DateTime = dateTime;
    }
}
