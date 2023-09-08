namespace NETworkManager.Models.Network;

/// <summary>
/// Contains the information of a DNS lookup record in a <see cref="DNSLookup"/>.
/// </summary>
public class DNSLookupRecordReceivedArgs : System.EventArgs
{        
    /// <summary>
    /// DNS Lookup record information.
    /// </summary>
   public DNSLookupRecordInfo Args { get; }

    public DNSLookupRecordReceivedArgs(DNSLookupRecordInfo args)
    {
        Args = args;
    }
}
