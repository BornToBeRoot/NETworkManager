using System.Text;

namespace NETworkManager.Models.Network;

/// <summary>
///     Version and credentials of an SNMP request of the IP Scanner (see <see cref="SNMPSystemInfoResolver" />): the
///     community for v1 / v2c, the user (security level, authentication and privacy) for v3. Plain text only for the
///     duration of a scan; <see cref="ToString" /> doesn't contain the secrets.
/// </summary>
/// <param name="Version">SNMP version.</param>
public sealed record SNMPCredentials(SNMPVersion Version)
{
    /// <summary>
    ///     Community (v1 / v2c), e.g. "public".
    /// </summary>
    public string Community { get; init; } = string.Empty;

    /// <summary>
    ///     Security level (v3).
    /// </summary>
    public SNMPV3Security Security { get; init; } = SNMPV3Security.AuthPriv;

    /// <summary>
    ///     Username (v3).
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    ///     Authentication provider (v3, AuthNoPriv and AuthPriv).
    /// </summary>
    public SNMPV3AuthenticationProvider AuthenticationProvider { get; init; } = SNMPV3AuthenticationProvider.SHA1;

    /// <summary>
    ///     Authentication password (v3, AuthNoPriv and AuthPriv).
    /// </summary>
    public string Auth { get; init; } = string.Empty;

    /// <summary>
    ///     Privacy provider (v3, AuthPriv).
    /// </summary>
    public SNMPV3PrivacyProvider PrivacyProvider { get; init; } = SNMPV3PrivacyProvider.AES;

    /// <summary>
    ///     Privacy password (v3, AuthPriv).
    /// </summary>
    public string Priv { get; init; } = string.Empty;

    /// <summary>
    ///     Credentials of v1 / v2c with a community.
    /// </summary>
    /// <param name="version">SNMP version (<see cref="SNMPVersion.V1" /> or <see cref="SNMPVersion.V2C" />).</param>
    /// <param name="community">Community, e.g. "public".</param>
    /// <returns>Credentials.</returns>
    public static SNMPCredentials FromCommunity(SNMPVersion version, string community)
    {
        return new SNMPCredentials(version) { Community = community ?? string.Empty };
    }

    /// <summary>
    ///     Only the version and the user (e.g. in a log), never the community or the passwords.
    /// </summary>
    private bool PrintMembers(StringBuilder builder)
    {
        builder.Append($"Version = {Version}");

        if (Version == SNMPVersion.V3)
            builder.Append($", Security = {Security}, Username = {Username}");

        return true;
    }
}
