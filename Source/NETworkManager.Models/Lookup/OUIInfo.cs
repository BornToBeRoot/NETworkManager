namespace NETworkManager.Models.Lookup
{
    public class OUIInfo
    {
        public string MACAddress { get; set; }
        public string Vendor { get; set; }

        public string MACAddressString => MACAddress.Replace("-", "").Replace(":", "");

        public OUIInfo(string macAddress, string vendor)
        {
            MACAddress = macAddress;
            Vendor = vendor;
        }
    }
}

