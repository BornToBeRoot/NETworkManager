namespace NETworkManager.Models.Network
{
    public class SNMPReceivedInfo
    {
        public string OID { get; set; }
        public string Data { get; set; }
        
        public SNMPReceivedInfo()
        {

        }

        public SNMPReceivedInfo(string oid, string data)
        {
            OID = oid;
            Data = data;
        }

        public static SNMPReceivedInfo Parse(SNMPReceivedArgs e)
        {
            return new SNMPReceivedInfo(e.OID, e.Data);
        }
    }
}
