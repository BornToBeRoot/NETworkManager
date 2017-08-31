namespace NETworkManager.Models.Settings
{
    public class WakeOnLANClientInfo
    {
        public string MACAddress { get; set; }
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string Broadcast { get; set; }

        public WakeOnLANClientInfo()
        {

        }
       
        public override string ToString()
        {
            return MACAddress;
        }
    }
}
