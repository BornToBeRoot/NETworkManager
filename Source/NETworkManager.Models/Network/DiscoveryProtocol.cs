namespace NETworkManager.Models.Network
{
    /// <summary>
    /// Enum indicates all discovery protocols.
    /// </summary>
    public enum DiscoveryProtocol
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
