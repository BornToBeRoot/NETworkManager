namespace NETworkManager.Models.Network
{
    public class DNSLookupErrorArgs : System.EventArgs
    {
        public string Error { get; set; }
        
        public DNSLookupErrorArgs()
        {

        }

        public DNSLookupErrorArgs(string error)
        {
            Error = error;
        }
    }
}
