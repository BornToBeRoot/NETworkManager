namespace NETworkManager.Models.Network
{
    public class IPDNSApiInfo
    {
        /// <summary>
        /// DNS server information.
        /// </summary>
        public IPDNSApiDnsInfo Dns { get; set; }

        /// <summary>
        /// Edns server information.
        /// </summary>
        public IPDNSApiEdnsInfo Edns { get; set; }
    }
}
