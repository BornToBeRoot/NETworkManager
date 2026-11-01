using System.Collections.Generic;
using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Security;

namespace NETworkManager.Models.Network;

/// <summary>
///     Methods for SNMPv3 (SNMP tool, SNMP of the IP Scanner): the privacy provider of the credentials and the error codes
///     of a report.
/// </summary>
public static class SNMPV3Helper
{
    /// <summary>
    ///     Known SNMPv3 error codes (usmStats of a report) with their Object Identifier.
    /// </summary>
    private static readonly Dictionary<ObjectIdentifier, SNMPV3ErrorCode> ErrorOIDs = new()
    {
        { new ObjectIdentifier("1.3.6.1.6.3.15.1.1.3.0"), SNMPV3ErrorCode.UnknownUserName },
        { new ObjectIdentifier("1.3.6.1.6.3.15.1.1.5.0"), SNMPV3ErrorCode.AuthenticationFailed }
    };

    /// <summary>
    ///     Gets the SNMPv3 error code of a variable of a report (e.g. usmStatsUnknownUserNames).
    /// </summary>
    /// <param name="oid">Object Identifier of the variable.</param>
    /// <param name="errorCode">Error code, <see cref="SNMPV3ErrorCode.None" /> if unknown.</param>
    /// <returns>True if the error code is known.</returns>
    public static bool TryGetErrorCode(ObjectIdentifier oid, out SNMPV3ErrorCode errorCode)
    {
        if (ErrorOIDs.TryGetValue(oid, out errorCode))
            return true;

        errorCode = SNMPV3ErrorCode.None;

        return false;
    }

    /// <summary>
    ///     Creates the privacy provider (incl. the authentication provider) of SNMPv3 credentials.
    /// </summary>
    /// <param name="security">Security level.</param>
    /// <param name="authProvider">Authentication provider (AuthNoPriv, AuthPriv).</param>
    /// <param name="auth">Authentication password (AuthNoPriv, AuthPriv).</param>
    /// <param name="privProvider">Privacy provider (AuthPriv).</param>
    /// <param name="priv">Privacy password (AuthPriv).</param>
    /// <returns>Privacy provider.</returns>
    public static IPrivacyProvider CreatePrivacyProvider(SNMPV3Security security,
        SNMPV3AuthenticationProvider authProvider, string auth, SNMPV3PrivacyProvider privProvider, string priv)
    {
        return security switch
        {
            SNMPV3Security.AuthPriv => CreatePrivacyProvider(privProvider, priv,
                CreateAuthenticationProvider(authProvider, auth)),
            SNMPV3Security.AuthNoPriv => new DefaultPrivacyProvider(CreateAuthenticationProvider(authProvider, auth)),
            _ => new DefaultPrivacyProvider(DefaultAuthenticationProvider.Instance)
        };
    }

    /// <summary>
    ///     Creates the privacy provider of a privacy password.
    /// </summary>
    private static IPrivacyProvider CreatePrivacyProvider(SNMPV3PrivacyProvider privProvider, string priv,
        IAuthenticationProvider authenticationProvider)
    {
        var privacyPassword = new OctetString(priv ?? string.Empty);

        return privProvider switch
        {
#pragma warning disable CS0618 // Allow outdated algorithms. We provide the function also for old devices. The user should use newer algorithms...
            SNMPV3PrivacyProvider.DES => new DESPrivacyProvider(privacyPassword, authenticationProvider),
#pragma warning restore CS0618 // Allow outdated algorithms. We provide the function also for old devices. The user should use newer algorithms...
            SNMPV3PrivacyProvider.AES => new AESPrivacyProvider(privacyPassword, authenticationProvider),
            SNMPV3PrivacyProvider.AES192 => new AES192PrivacyProvider(privacyPassword, authenticationProvider),
            SNMPV3PrivacyProvider.AES256 => new AES256PrivacyProvider(privacyPassword, authenticationProvider),
            _ => null
        };
    }

    /// <summary>
    ///     Creates the authentication provider of an authentication password.
    /// </summary>
    private static IAuthenticationProvider CreateAuthenticationProvider(SNMPV3AuthenticationProvider authProvider,
        string auth)
    {
        var authenticationPassword = new OctetString(auth ?? string.Empty);

        return authProvider switch
        {
#pragma warning disable CS0618 // Allow outdated algorithms. We provide the function also for old devices. The user should use newer algorithms...
            SNMPV3AuthenticationProvider.MD5 => new MD5AuthenticationProvider(authenticationPassword),
            SNMPV3AuthenticationProvider.SHA1 => new SHA1AuthenticationProvider(authenticationPassword),
#pragma warning restore CS0618 // Allow outdated algorithms. We provide the function also for old devices. The user should use newer algorithms...
            SNMPV3AuthenticationProvider.SHA256 => new SHA256AuthenticationProvider(authenticationPassword),
            SNMPV3AuthenticationProvider.SHA384 => new SHA384AuthenticationProvider(authenticationPassword),
            SNMPV3AuthenticationProvider.SHA512 => new SHA512AuthenticationProvider(authenticationPassword),
            _ => null
        };
    }
}
