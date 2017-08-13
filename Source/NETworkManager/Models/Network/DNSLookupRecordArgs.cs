using Heijden.DNS;

namespace NETworkManager.Models.Network
{
    public class DNSLookupRecordArgs : System.EventArgs
    {        
        public RR ResourceRecord { get; set; }
        public string Result { get; set; }

        public DNSLookupRecordArgs()
        {

        }

        public DNSLookupRecordArgs(RR resourceRecord, object result)
        {
            ResourceRecord = resourceRecord;
            Result = result.ToString();
        }
    }
}
