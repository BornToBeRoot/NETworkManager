using System.Security;
using NETworkManager.Models.Network;

namespace NETworkManager.Profiles.Application;

public static class IPScanner
{
    /// <summary>
    ///     Gets the SNMP version and credentials of a profile for the IP Scanner: its own (if overridden), otherwise the
    ///     ones of its group (if overridden), otherwise null (the settings are used). Protected, converted to plain text
    ///     only for the SNMP request of a scan (see <see cref="SNMPSecureCredentials.ToCredentials" />).
    /// </summary>
    /// <param name="profile">Profile the tab is opened with.</param>
    /// <returns>Credentials, null if neither the profile nor its group overrides them.</returns>
    public static SNMPSecureCredentials GetSNMPCredentials(ProfileInfo profile)
    {
        return GetSNMPCredentials(profile, ProfileManager.GetGroupByName(profile.Group));
    }

    /// <summary>
    ///     Gets the SNMP version and credentials of a profile and its group, see
    ///     <see cref="GetSNMPCredentials(ProfileInfo)" />.
    /// </summary>
    /// <param name="profile">Profile.</param>
    /// <param name="group">Group of the profile, null if it can't be found.</param>
    /// <returns>Credentials, null if neither the profile nor its group overrides them.</returns>
    public static SNMPSecureCredentials GetSNMPCredentials(ProfileInfo profile, GroupInfo group)
    {
        if (profile.IPScanner_OverrideSNMP)
            return CreateCredentials(profile.IPScanner_SNMPVersion, profile.IPScanner_SNMPCommunity,
                profile.IPScanner_SNMPSecurity, profile.IPScanner_SNMPUsername,
                profile.IPScanner_SNMPAuthenticationProvider, profile.IPScanner_SNMPAuth,
                profile.IPScanner_SNMPPrivacyProvider, profile.IPScanner_SNMPPriv);

        if (group is { IPScanner_OverrideSNMP: true })
            return CreateCredentials(group.IPScanner_SNMPVersion, group.IPScanner_SNMPCommunity,
                group.IPScanner_SNMPSecurity, group.IPScanner_SNMPUsername,
                group.IPScanner_SNMPAuthenticationProvider, group.IPScanner_SNMPAuth,
                group.IPScanner_SNMPPrivacyProvider, group.IPScanner_SNMPPriv);

        return null;
    }

    /// <summary>
    ///     Creates the credentials of the values of a profile or group (copies of the passwords, a change of the profile
    ///     doesn't change an open tab).
    /// </summary>
    private static SNMPSecureCredentials CreateCredentials(SNMPVersion version, SecureString community,
        SNMPV3Security security, string username, SNMPV3AuthenticationProvider authenticationProvider,
        SecureString auth, SNMPV3PrivacyProvider privacyProvider, SecureString priv)
    {
        return new SNMPSecureCredentials(version)
        {
            Community = community?.Copy(),
            Security = security,
            Username = username ?? string.Empty,
            AuthenticationProvider = authenticationProvider,
            Auth = auth?.Copy(),
            PrivacyProvider = privacyProvider,
            Priv = priv?.Copy()
        };
    }
}
