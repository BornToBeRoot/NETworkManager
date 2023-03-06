namespace NETworkManager.Models.Network;

public class SNTPLookupResultInfo
{
    public string Server { get; set; }
    public string IPEndPoint { get; set; }
    public SNTPDateTime DateTime { get; set; }
            
    public SNTPLookupResultInfo()
    {

    }

    public SNTPLookupResultInfo(string server, string ipEndPoint, SNTPDateTime dateTime)
    {
        Server = server;
        IPEndPoint = ipEndPoint;
        DateTime = dateTime;            
    }

    public static SNTPLookupResultInfo Parse(SNTPLookupResultArgs e)
    {
        return new SNTPLookupResultInfo(e.Server, e.IPEndPoint, e.DateTime);
    }
}