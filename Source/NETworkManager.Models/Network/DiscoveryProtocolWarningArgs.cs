namespace NETworkManager.Models.Network
{
    public class DiscoveryProtocolWarningArgs : System.EventArgs
    {
        public string Message { get; set; }

        public DiscoveryProtocolWarningArgs()
        {

        }

        public DiscoveryProtocolWarningArgs(string message)
        {
            Message = message;
        }
    }
}
