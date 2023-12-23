namespace NETworkManager.Models.Network;

/// <summary>
///     Class containing information about a DNS lookup record.
/// </summary>
public class DNSLookupRecordInfo
{
    /// <summary>
    ///     Creates a new instance of <see cref="DNSLookupRecordInfo" /> with the specified parameters.
    /// </summary>
    /// <param name="domainName">Domain name of the record.</param>
    /// <param name="ttl">Time to live (TTL) of the record.</param>
    /// <param name="recordClass">Class of the record.</param>
    /// <param name="recordType">Type of the record.</param>
    /// <param name="result">Result of the record. (IP address, hostname, text, etc.)</param>
    /// <param name="nameServerIPAddress">IP address of the name server that provided the result.</param>
    /// <param name="nameServerHostName">Hostname of the name server that provided the result.</param>
    /// <param name="nameServerPort">Port of the name server that provided the result.</param>
    public DNSLookupRecordInfo(string domainName, int ttl, string recordClass, string recordType, string result,
        string nameServerIPAddress, string nameServerHostName, int nameServerPort)
    {
        DomainName = domainName;
        TTL = ttl;
        RecordClass = recordClass;
        RecordType = recordType;
        Result = result;
        NameServerIPAddress = nameServerIPAddress;
        NameServerPort = nameServerPort;
        NameServerHostName = nameServerHostName;
    }

    /// <summary>
    ///     Domain name of the record.
    /// </summary>
    public string DomainName { get; set; }

    /// <summary>
    ///     Time to live (TTL) of the record.
    /// </summary>
    public int TTL { get; set; }

    /// <summary>
    ///     Class of the record.
    /// </summary>
    public string RecordClass { get; set; }

    /// <summary>
    ///     Type of the record.
    /// </summary>
    public string RecordType { get; set; }

    /// <summary>
    ///     Result of the record. (IP address, hostname, text, etc.)
    /// </summary>
    public string Result { get; set; }

    /// <summary>
    ///     IP address of the name server that provided the result.
    /// </summary>
    public string NameServerIPAddress { get; set; }

    /// <summary>
    ///     Port of the name server that provided the result.
    /// </summary>
    public int NameServerPort { get; set; }

    /// <summary>
    ///     Hostname of the name server that provided the result.
    /// </summary>
    public string NameServerHostName { get; set; }

    /// <summary>
    ///     Hostname (if available) or/and IP address with port of the name server that provided the result.
    /// </summary>
    public string NameServerAsString => string.IsNullOrEmpty(NameServerHostName)
        ? $"{NameServerIPAddress}:{NameServerPort}"
        : $"{NameServerHostName.TrimEnd('.')} # {NameServerIPAddress}:{NameServerPort}";
}