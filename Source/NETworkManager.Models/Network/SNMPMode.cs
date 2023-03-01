namespace NETworkManager.Models.Network
{
    public partial class SNMP
    {
        #endregion

        // Trap and Inform not implemented
        public enum SNMPMode
        {
            Get,
            Walk,
            Set,
            Trap,
            Inform
        }
    }
}
