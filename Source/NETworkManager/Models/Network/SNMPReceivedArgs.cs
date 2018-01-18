using Lextm.SharpSnmpLib;

namespace NETworkManager.Models.Network
{
    public class SNMPReceivedArgs : System.EventArgs
    {
        public string OID { get; set; }
        public string Data { get; set; }

        public SNMPReceivedArgs()
        {

        }

        public SNMPReceivedArgs(ObjectIdentifier oid, ISnmpData data)
        {
            OID = oid.ToString();
            Data = data.ToString();
        }
    }
}
