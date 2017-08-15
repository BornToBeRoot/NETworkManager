namespace NETworkManager.Models.Network
{
    public class DNSLookupCompleteArgs : System.EventArgs
    {
        public int ResourceRecordsCount { get; set; }

        public DNSLookupCompleteArgs()
        {

        }

        public DNSLookupCompleteArgs(int resourceRecordsCount)
        {
            ResourceRecordsCount = resourceRecordsCount;
        }
    }
}
