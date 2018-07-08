namespace NETworkManager.Models.Lookup
{
    public class OUIInfo
    {
        public string MacAddress { get; set; }
        public string Vendor { get; set; }

        public OUIInfo(string macAddress, string vendor)
        {
            MacAddress = macAddress;
            Vendor = vendor;
        }
    }
}
