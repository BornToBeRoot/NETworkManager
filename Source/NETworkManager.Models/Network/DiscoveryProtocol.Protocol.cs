namespace NETworkManager.Models.Network;

    /// <summary>
    /// Represents all discovery protocols.
    /// </summary>
    public enum DiscoveryProtocol
    {
        /// <summary>
        /// Link layer and Cisco discovery protocol.
        /// </summary>
        LldpCdp,

        /// <summary>
        /// Link layer discovery protocol.
        /// </summary>
        Lldp,

        /// <summary>
        /// Cisco discovery protocol.
        /// </summary>
        Cdp,
    }
