namespace NETworkManager.Models.Network
{
    /// <summary>
    /// Class for (E)DNS server data used for deserialization.
    /// </summary>
    public class IPDNSApiDeserializationInfo
    {
        /// <summary>
        /// DNS server data.
        /// </summary>
        public IPDNSApiDeserializationBaseInfo Dns { get; set; }

        /// <summary>
        /// EDNS server data.
        /// </summary>
        public IPDNSApiDeserializationBaseInfo Edns { get; set; }
    }
}
