namespace NETworkManager.Models.Lookup
{
    public class OuiInfo
    {
        public string MacAddress { get; set; }
        public string Vendor { get; set; }

        public OuiInfo(string macAddress, string vendor)
        {
            MacAddress = macAddress;
            Vendor = vendor;
        }
    }
}
