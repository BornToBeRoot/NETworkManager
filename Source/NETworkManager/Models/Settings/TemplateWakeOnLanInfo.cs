namespace NETworkManager.Models.Settings
{
    public class TemplateWakeOnLANInfo
    {
        public string MACAddress { get; set; }
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string Broadcast { get; set; }

        public TemplateWakeOnLANInfo()
        {

        }
       
        public override string ToString()
        {
            return MACAddress;
        }
    }
}
