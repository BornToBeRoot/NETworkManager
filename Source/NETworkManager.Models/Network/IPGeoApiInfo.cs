namespace NETworkManager.Models.Network
{
    public class IPGeoApiInfo
    {
        /// <summary>
        /// The status of the IP information retrieval.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The continent where the IP address is located.
        /// </summary>
        public string Continent { get; set; }

        /// <summary>
        /// The continent code where the IP address is located.
        /// </summary>
        public string ContinentCode { get; set; }

        /// <summary>
        /// The country where the IP address is located.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// The country code where the IP address is located.
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// The region where the IP address is located.
        /// </summary>
        public string Region { get; set; }

        /// <summary>
        /// The region name where the IP address is located.
        /// </summary>
        public string RegionName { get; set; }

        /// <summary>
        /// The city where the IP address is located.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// The district where the IP address is located.
        /// </summary>
        public string District { get; set; }

        /// <summary>
        /// The zip code of the location where the IP address is located.
        /// </summary>
        public string Zip { get; set; }

        /// <summary>
        /// The latitude of the location where the IP address is located.
        /// </summary>
        public double Lat { get; set; }

        /// <summary>
        /// The longitude of the location where the IP address is located.
        /// </summary>
        public double Lon { get; set; }

        /// <summary>
        /// The timezone of the location where the IP address is located.
        /// </summary>
        public string Timezone { get; set; }

        /// <summary>
        /// The offset from UTC in seconds for the location where the IP address is located.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// The currency used in the country where the IP address is located.
        /// </summary>
        public string Currency { get; set; }

        /// <summary>
        /// The Internet Service Provider (ISP) of the IP address.
        /// </summary>
        public string Isp { get; set; }

        /// <summary>
        /// The organization associated with the IP address.
        /// </summary>
        public string Org { get; set; }

        /// <summary>
        /// The Autonomous System (AS) number and name associated with the IP address.
        /// </summary>
        public string As { get; set; }

        /// <summary>
        /// The name of the Autonomous System (AS) associated with the IP address.
        /// </summary>
        public string Asname { get; set; }

        /// <summary>
        /// The reverse DNS hostname associated with the IP address.
        /// </summary>
        public string Reverse { get; set; }

        /// <summary>
        /// Indicates whether the IP address is associated with a mobile network.
        /// </summary>
        public bool Mobile { get; set; }

        /// <summary>
        /// Indicates whether the IP address is a proxy server.
        /// </summary>
        public bool Proxy { get; set; }

        /// <summary>
        /// Indicates whether the IP address is associated with hosting services.
        /// </summary>
        public bool Hosting { get; set; }

        /// <summary>
        /// The IP address used for the query.
        /// </summary>
        public string Query { get; set; }
    }
}