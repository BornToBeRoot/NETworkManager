namespace NETworkManager.Models.Network
{
    public partial class DiscoveryProtocol
    {
        /// <summary>
        /// Represents all discovery protocols.
        /// </summary>
        public enum Protocol
        {
            /// <summary>
            /// Link layer and Cisco discovery protocol.
            /// </summary>
            LLDP_CDP,

            /// <summary>
            /// Link layer discovery protocol.
            /// </summary>
            LLDP,

            /// <summary>
            /// Cisco discovery protocol.
            /// </summary>
            CDP
        }
    }

}