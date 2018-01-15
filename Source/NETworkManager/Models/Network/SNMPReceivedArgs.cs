namespace NETworkManager.Models.Network
{
    public class SNMPReceivedArgs : System.EventArgs
    {
        public string OID { get; set; }
        public string Data { get; set; }

        public SNMPReceivedArgs()
        {

        }

        public SNMPReceivedArgs(string oid, string data)
        {
            OID = oid;
            Data = data;
        }
    }
}
