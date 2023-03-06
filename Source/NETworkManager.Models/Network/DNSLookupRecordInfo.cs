namespace NETworkManager.Models.Network;

public class DNSLookupRecordInfo
{

    public string DomainName { get; set; }
    
    public int TTL { get; set; }
    
    public string Class { get; set; }

    public string Type { get; set; }

    public string Result { get; set; }

    public string Server { get; set; }

    public string IPEndPoint { get; set; }

    public DNSLookupRecordInfo()
    {

    }

    public DNSLookupRecordInfo(string domainName, int ttl, string xclass, string type, string result, string server, string ipEndPoint)
    {
        DomainName = domainName;
        TTL = ttl;
        Class = xclass;
        Type = type;
        Result = result;
        Server = server;
        IPEndPoint = ipEndPoint;
    }

    public static DNSLookupRecordInfo Parse(DNSLookupRecordArgs e)
    {
        return new DNSLookupRecordInfo(e.DomainName, e.TTL, e.Class.ToString(), e.Type.ToString(), e.Result, e.Server, e.IPEndPoint);
    }
}
