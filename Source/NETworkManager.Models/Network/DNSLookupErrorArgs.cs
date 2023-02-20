namespace NETworkManager.Models.Network
{
    public class DNSLookupErrorArgs : System.EventArgs
    {
        public string Server { get; set; }

        public bool HasIPEndPoint { get; set; }

        public string IPEndPoint { get; set; }

        public string ErrorMessage { get; set; }
        
        public DNSLookupErrorArgs()
        {

        }

        public DNSLookupErrorArgs(string server, string errorMessage)
        {
            Server = server;
            ErrorMessage = errorMessage;
        }
        
        public DNSLookupErrorArgs(string server, string ipEndPoint, string errorMessage)
        {
            Server = server;
            HasIPEndPoint = true;
            IPEndPoint = ipEndPoint;
            ErrorMessage = errorMessage;
        }
    }
}
