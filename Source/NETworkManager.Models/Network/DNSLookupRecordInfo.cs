namespace NETworkManager.Models.Network;

/// <summary>
/// Class containing information about a DNS lookup record.
/// </summary>
public class DNSLookupRecordInfo
{
    /// <summary>
    /// Domain name of the record.
    /// </summary>
    public string DomainName { get; set; }
    
    /// <summary>
    /// Time to live (TTL) of the record.
    /// </summary>
    public int TTL { get; set; }
    
    /// <summary>
    /// Class of the record.
    /// </summary>
    public string RecordClass { get; set; }

    /// <summary>
    /// Type of the record.
    /// </summary>
    public string RecordType { get; set; }

    /// <summary>
    /// Result of the record. (IP address, hostname, text, etc.)
    /// </summary>
    public string Result { get; set; }

    /// <summary>
    /// Name server that provided the result. 
    /// </summary>
    public string Server { get; set; }

    /// <summary>
    /// IP endpoint (IP address:port) of the name server that provided the result.
    /// </summary>
    public string IPEndPoint { get; set; }

    /// <summary>
    /// Creates a new instance of <see cref="DNSLookupRecordInfo"/> with the specified parameters.
    /// </summary>
    /// <param name="domainName">Domain name of the record.</param>
    /// <param name="ttl">Time to live (TTL) of the record.</param>
    /// <param name="recordClass">Class of the record.</param>
    /// <param name="recordType">Type of the record.</param>
    /// <param name="result">Result of the record. (IP address, hostname, text, etc.)</param>
    /// <param name="server">Name server that provided the result.</param>
    /// <param name="ipEndPoint">IP endpoint (IP address:port) of the name server that provided the result.</param>
    public DNSLookupRecordInfo(string domainName, int ttl, string recordClass, string recordType, string result, string server, string ipEndPoint)
    {
        DomainName = domainName;
        TTL = ttl;
        RecordClass = recordClass;
        RecordType = recordType;
        Result = result;
        Server = server;
        IPEndPoint = ipEndPoint;
    }
}
