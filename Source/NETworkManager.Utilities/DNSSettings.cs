using System.Collections.Generic;

namespace NETworkManager.Utilities
{
    /// <summary>
    /// Class is used to store informations about DNS lookup.
    /// </summary>
    public class DNSSettings
    {
        /// <summary>
        /// List of DNS servers.
        /// </summary>
        public IEnumerable<string> DNSServers { get; set; }
    }
}
