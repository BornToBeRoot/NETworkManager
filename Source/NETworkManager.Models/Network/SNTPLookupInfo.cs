namespace NETworkManager.Models.Network;

public class SNTPLookupInfo
{
    /// <summary>
    /// SNTP server used for the lookup.
    /// </summary>
    public string Server { get; set; }
    
    /// <summary>
    /// IP endpoint (IP address:port) of the SNTP server.
    /// </summary>
    public string IPEndPoint { get; set; }
    
    /// <summary>
    /// Date and time received from the SNTP server.
    /// </summary>
    public SNTPDateTime DateTime { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="SNTPLookupInfo"/> with the specified parameters.
    /// </summary>
    /// <param name="server">SNTP server used for the lookup.</param>
    /// <param name="ipEndPoint">IP endpoint (IP address:port) of the SNTP server.</param>
    /// <param name="dateTime">Date and time received from the SNTP server.</param>
    public SNTPLookupInfo(string server, string ipEndPoint, SNTPDateTime dateTime)
    {
        Server = server;
        IPEndPoint = ipEndPoint;
        DateTime = dateTime;            
    }
}