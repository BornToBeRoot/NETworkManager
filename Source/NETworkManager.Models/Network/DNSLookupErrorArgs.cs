using System;

namespace NETworkManager.Models.Network;

public class DNSLookupErrorArgs : EventArgs
{
    public DNSLookupErrorArgs()
    {
    }

    public DNSLookupErrorArgs(string query, string server, string ipEndPoint, string errorMessage)
    {
        Query = query;
        Server = server;
        ErrorMessage = errorMessage;
        IPEndPoint = ipEndPoint;
    }

    public string Query { get; set; }

    public string Server { get; set; }

    public bool HasIPEndPoint { get; set; }

    public string IPEndPoint { get; set; }

    public string ErrorMessage { get; set; }
}