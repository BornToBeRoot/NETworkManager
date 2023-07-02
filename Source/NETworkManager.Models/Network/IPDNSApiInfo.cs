namespace NETworkManager.Models.Network
{
    /// <summary>
    /// Class contains the IP information from the IP DNS API.
    /// </summary>
    public class IPDNSApiInfo
    {
        /// <summary>
        /// IP address of the DNS server.
        /// </summary>
        public string DnsIp { get; set; }

        /// <summary>
        /// Geographic location of the DNS server.
        /// </summary>
        public string DnsGeo { get; set; }


        /// <summary>
        /// IP address of the Edns server.
        /// </summary>
        public string EdnsIp { get; set; }

        /// <summary>
        /// Geographic location of the Edns server.
        /// </summary>
        public string EdnsGeo { get; set; }

        /// <summary>
        /// Create an instance of <see cref="IPDNSApiInfo"/> with parameters.
        /// </summary>
        /// <param name="dnsIp">IP address of the DNS server.</param>
        /// <param name="dnsGeo">Geographic location of the DNS server.</param>
        /// <param name="ednsIp">IP address of the EDNS server.</param>
        /// <param name="ednsGeo">Geographic location of the EDNS server.</param>
        public IPDNSApiInfo(string dnsIp, string dnsGeo, string ednsIp, string ednsGeo)
        {
            DnsIp = dnsIp;
            DnsGeo = dnsGeo;
            EdnsIp = ednsIp;
            EdnsGeo = ednsGeo;
        }

        /// <summary>
        /// Parses the IP information from the deserialization info.
        /// </summary>
        /// <param name="info"><see cref="IPDNSApiDeserializationInfo"/> to parse to <see cref="IPDNSApiInfo"/>.</param>
        /// <returns>New instance of <see cref="IPDNSApiInfo"/> from <see cref="IPDNSApiDeserializationInfo"/> data.</returns>
        public static IPDNSApiInfo Parse(IPDNSApiDeserializationInfo info)
        {
            // Strings can be null if no data is available.
            return new IPDNSApiInfo(info.Dns?.Ip, info.Dns?.Geo, info.Edns?.Ip, info.Edns?.Geo);
        }
    }
}
