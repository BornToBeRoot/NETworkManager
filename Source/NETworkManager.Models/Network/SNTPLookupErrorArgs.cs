namespace NETworkManager.Models.Network
{
    public class SNTPLookupErrorArgs : System.EventArgs
    {
        public string ErrorCode { get; set; }
        public (string Server, int Port) SNTPServer { get; set; }

        public SNTPLookupErrorArgs()
        {

        }

        public SNTPLookupErrorArgs(string errorCode, (string Server, int Port) sntpServer)
        {
            ErrorCode = errorCode;
            SNTPServer = sntpServer;
        }
    }
}
