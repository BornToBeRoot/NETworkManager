using Heijden.DNS;

namespace NETworkManager.Models.Network
{
    public class DNSLookupRecordInfo
    {
        public string Name { get; set; }
        public uint TTL { get; set; }
        public string Class { get; set; }
        public string Type { get; set; }
        public string Result { get; set; }

        public DNSLookupRecordInfo()
        {

        }

        public DNSLookupRecordInfo(RR resourceRecord, string result)
        {
            Name = resourceRecord.NAME;
            TTL = resourceRecord.TTL;
            Class = resourceRecord.Class.ToString();
            Type = resourceRecord.Type.ToString();
            Result = result;
        }

        public static DNSLookupRecordInfo Parse(DNSLookupRecordArgs e)
        {
            return new DNSLookupRecordInfo(e.ResourceRecord, e.Result);
        }
    }
}
