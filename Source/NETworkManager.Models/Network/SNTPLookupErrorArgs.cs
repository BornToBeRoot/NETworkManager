namespace NETworkManager.Models.Network
{
    public class SNTPLookupErrorArgs : System.EventArgs
    {
        public string Server { get; set; }

        public bool HasIPEndPoint { get; set; }
        public string IPEndPoint { get; set; }
        
        public string ErrorCode { get; set; }

        public SNTPLookupErrorArgs()
        {

        }

        public SNTPLookupErrorArgs(string server, string errorCode)
        {
            Server = server;            
            ErrorCode = errorCode;
        }

        public SNTPLookupErrorArgs(string server, string ipEndPoint, string errorCode)
        {
            Server = server;
            HasIPEndPoint = true;
            IPEndPoint = ipEndPoint;
            ErrorCode = errorCode;
        }
    }
}
