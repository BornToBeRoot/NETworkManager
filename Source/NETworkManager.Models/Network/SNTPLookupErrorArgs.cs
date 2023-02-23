namespace NETworkManager.Models.Network
{
    public class SNTPLookupErrorArgs : System.EventArgs
    {
        public string Server { get; set; }

        public string IPEndPoint { get; set; }
        
        public string ErrorMessage { get; set; }

        public bool IsDNSError { get; set; }

        public SNTPLookupErrorArgs()
        {

        }

        public SNTPLookupErrorArgs(string errorMessage, bool isDNSError)
        {            
            ErrorMessage = errorMessage;
            IsDNSError = isDNSError;
        }

        public SNTPLookupErrorArgs(string server, string ipEndPoint, string errorMessage)
        {
            Server = server;     
            IPEndPoint = ipEndPoint;
            ErrorMessage = errorMessage;
        }
    }
}
