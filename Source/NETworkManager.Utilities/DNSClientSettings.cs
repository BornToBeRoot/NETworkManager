using System.Collections.Generic;

namespace NETworkManager.Utilities;

/// <summary>
///     Class is used to store settings for the <see cref="DNSClient" />.
/// </summary>
public class DNSClientSettings
{
    /// <summary>
    ///     Create an instance of <see cref="DNSClientSettings" />.
    /// </summary>
    public DNSClientSettings()
    {
    }

    /// <summary>
    ///     Use custom DNS servers.
    /// </summary>
    public bool UseCustomDNSServers { get; set; }

    /// <summary>
    ///     List of name servers as Tuple (string Server, int Port).
    /// </summary>
    public IEnumerable<(string Server, int Port)> DNSServers { get; set; }
}