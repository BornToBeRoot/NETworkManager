namespace NETworkManager.Models.Network
{
    public class DiscoveryProtocolErrorArgs : System.EventArgs
    {
        public string Message { get; set; }
        

        public DiscoveryProtocolErrorArgs()
        {

        }

        public DiscoveryProtocolErrorArgs(string message)
        {
            Message = message;
        }
    }
}
