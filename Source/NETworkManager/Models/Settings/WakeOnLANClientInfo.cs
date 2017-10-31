namespace NETworkManager.Models.Settings
{
    public class WakeOnLANClientInfo
    {
        public string Name { get; set; }
        public string MACAddress { get; set; }
        public string Broadcast { get; set; }
        public int Port { get; set; }
        public string Group { get; set; }

        public WakeOnLANClientInfo()
        {

        }
       
        public WakeOnLANClientInfo(string name, string macAddress, string broadcast, int port, string group)
        {
            Name = name;
            MACAddress = macAddress;
            Broadcast = broadcast;
            Port = port;
            Group = group;
        }

        public override string ToString()
        {
            return MACAddress;
        }
    }
}
