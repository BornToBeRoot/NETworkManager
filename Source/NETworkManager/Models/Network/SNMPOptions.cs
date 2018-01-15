using Lextm.SharpSnmpLib.Messaging;
namespace NETworkManager.Models.Network
{
    public class SNMPOptions
    {
        public int Port { get; set; }
        public bool Walk { get; set; }
        public WalkMode WalkMode { get; set; }
        public int Timeout { get; set; }

        public SNMPOptions()
        {

        }
    }
}
