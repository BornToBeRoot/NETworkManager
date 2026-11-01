using System;

namespace NETworkManager.Models.Network;

/// <summary>
///     Information a host reveals in the SMB negotiation without credentials, see <see cref="SMBInfoResolver" />.
/// </summary>
/// <param name="Dialect">Negotiated SMB dialect, e.g. "3.1.1".</param>
/// <param name="SigningRequired">The server requires signed messages.</param>
/// <param name="ServerGuid">GUID of the SMB server (stable per host, e.g. for known devices).</param>
/// <param name="SystemTime">
///     Time of the host in UTC (of the negotiate response, otherwise of the NTLM challenge), null if the host didn't
///     send it.
/// </param>
/// <param name="TimeOffset">
///     Time of the host minus the local time (corrected by half the round trip time), null without
///     <paramref name="SystemTime" />.
/// </param>
/// <param name="NTLM">Values of the NTLM challenge, null if the host didn't send one (e.g. NTLM disabled).</param>
public sealed record SMBInfo(
    string Dialect,
    bool SigningRequired,
    Guid ServerGuid,
    DateTime? SystemTime,
    TimeSpan? TimeOffset,
    NTLMChallengeInfo NTLM);

/// <summary>
///     Values of an NTLM challenge (sent by the host before any credentials are checked), see
///     <see cref="SMBInfoResolver.ParseNTLMChallenge" />. Strings are empty if the host didn't send them.
/// </summary>
/// <param name="Version">Version of the operating system (major.minor.build), null if not sent.</param>
/// <param name="NetBIOSComputerName">NetBIOS name of the computer, e.g. "CLIENT01".</param>
/// <param name="NetBIOSDomainName">
///     NetBIOS name of the domain, e.g. "CORP" (the computer name if it is not in a domain).
/// </param>
/// <param name="DNSComputerName">DNS name of the computer, e.g. "client01.corp.example.com".</param>
/// <param name="DNSDomainName">DNS name of the domain, e.g. "corp.example.com" (the computer name if not in a domain).</param>
/// <param name="DNSTreeName">DNS name of the forest, e.g. "example.com".</param>
/// <param name="Timestamp">Time of the host in UTC, null if not sent.</param>
public sealed record NTLMChallengeInfo(
    Version Version,
    string NetBIOSComputerName,
    string NetBIOSDomainName,
    string DNSComputerName,
    string DNSDomainName,
    string DNSTreeName,
    DateTime? Timestamp)
{
    /// <summary>
    ///     The computer is in a domain: the NetBIOS domain name is not its own computer name (a computer in a
    ///     workgroup sends its own name as domain).
    /// </summary>
    public bool IsDomainMember => !string.IsNullOrEmpty(NetBIOSDomainName) &&
                                  !string.Equals(NetBIOSDomainName, NetBIOSComputerName,
                                      StringComparison.OrdinalIgnoreCase);
}
