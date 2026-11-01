using System.Security;
using NETworkManager.Utilities;

namespace NETworkManager.Models.Network;

/// <summary>
///     SNMP version and credentials of a profile / group as kept by an IP Scanner tab: the community and the passwords
///     protected (<see cref="SecureString" />), converted to plain text (<see cref="ToCredentials" />) only for the SNMP
///     request of a scan.
/// </summary>
/// <param name="Version">SNMP version.</param>
public sealed record SNMPSecureCredentials(SNMPVersion Version)
{
    /// <summary>
    ///     Community (v1 / v2c).
    /// </summary>
    public SecureString Community { get; init; }

    /// <summary>
    ///     Security level (v3).
    /// </summary>
    public SNMPV3Security Security { get; init; } = SNMPV3Security.AuthPriv;

    /// <summary>
    ///     Username (v3).
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    ///     Authentication provider (v3).
    /// </summary>
    public SNMPV3AuthenticationProvider AuthenticationProvider { get; init; } = SNMPV3AuthenticationProvider.SHA1;

    /// <summary>
    ///     Authentication password (v3).
    /// </summary>
    public SecureString Auth { get; init; }

    /// <summary>
    ///     Privacy provider (v3).
    /// </summary>
    public SNMPV3PrivacyProvider PrivacyProvider { get; init; } = SNMPV3PrivacyProvider.AES;

    /// <summary>
    ///     Privacy password (v3).
    /// </summary>
    public SecureString Priv { get; init; }

    /// <summary>
    ///     Converts the credentials to plain text for the SNMP request of a scan.
    /// </summary>
    /// <returns>Credentials of the scan.</returns>
    public SNMPCredentials ToCredentials()
    {
        return new SNMPCredentials(Version)
        {
            Community = ToString(Community),
            Security = Security,
            Username = Username ?? string.Empty,
            AuthenticationProvider = AuthenticationProvider,
            Auth = ToString(Auth),
            PrivacyProvider = PrivacyProvider,
            Priv = ToString(Priv)
        };
    }

    /// <summary>
    ///     Only the version and the user (e.g. in a log), never the community or the passwords.
    /// </summary>
    private bool PrintMembers(System.Text.StringBuilder builder)
    {
        builder.Append($"Version = {Version}");

        if (Version == SNMPVersion.V3)
            builder.Append($", Security = {Security}, Username = {Username}");

        return true;
    }

    private static string ToString(SecureString value)
    {
        return value == null ? string.Empty : SecureStringHelper.ConvertToString(value);
    }
}
