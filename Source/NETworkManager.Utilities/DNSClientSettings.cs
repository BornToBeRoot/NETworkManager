using System.Collections.Generic;

namespace NETworkManager.Utilities;

/// <summary>
///     Class is used to store settings for the <see cref="DNSClient" />.
///     <see cref="DNSClient" /> retains the instance passed to <see cref="DNSClient.Configure" /> and reads
///     it from concurrent resolve calls without further synchronization - do not mutate an instance after
///     passing it to <see cref="DNSClient.Configure" />; construct a new one instead.
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

    /// <summary>
    ///     Add the DNS suffix to a hostname without a dot before resolving it (A/AAAA queries only).
    /// </summary>
    public bool AddDNSSuffix { get; set; }

    /// <summary>
    ///     DNS suffix to append. Either the custom DNS suffix from the settings or the Windows DNS suffix.
    /// </summary>
    public string DNSSuffix { get; set; }
}