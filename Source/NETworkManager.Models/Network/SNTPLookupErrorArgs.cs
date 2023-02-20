namespace NETworkManager.Models.Network
{
    public class SNTPLookupErrorArgs : System.EventArgs
    {
        public string Server { get; set; }

        public bool HasIPEndPoint { get; set; }
        
        public string IPEndPoint { get; set; }
        
        public string ErrorMessage { get; set; }

        public SNTPLookupErrorArgs()
        {

        }

        public SNTPLookupErrorArgs(string server, string errorMessage)
        {
            Server = server;            
            ErrorMessage = errorMessage;
        }

        public SNTPLookupErrorArgs(string server, string ipEndPoint, string errorMessage)
        {
            Server = server;
            HasIPEndPoint = true;
            IPEndPoint = ipEndPoint;
            ErrorMessage = errorMessage;
        }
    }
}
