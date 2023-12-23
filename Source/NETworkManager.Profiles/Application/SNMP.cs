using NETworkManager.Models.Network;
using NETworkManager.Settings;

namespace NETworkManager.Profiles.Application;

public static class SNMP
{
    public static SNMPSessionInfo CreateSessionInfo()
    {
        return new SNMPSessionInfo
        {
            Mode = SettingsManager.Current.SNMP_Mode,

            Version = SettingsManager.Current.SNMP_Version,
            Security = SettingsManager.Current.SNMP_Security,
            AuthenticationProvider = SettingsManager.Current.SNMP_AuthenticationProvider,
            PrivacyProvider = SettingsManager.Current.SNMP_PrivacyProvider
        };
    }

    public static SNMPSessionInfo CreateSessionInfo(ProfileInfo profile)
    {
        SNMPSessionInfo info = new();

        // Get group info
        var group = ProfileManager.GetGroup(profile.Group);

        info.Host = profile.SNMP_Host;

        // OID and Mode
        if (profile.SNMP_OverrideOIDAndMode)
        {
            info.Mode = profile.SNMP_Mode;
            info.OID = profile.SNMP_OID;
        }
        else if (group.SNMP_OverrideOIDAndMode)
        {
            info.Mode = group.SNMP_Mode;
            info.OID = group.SNMP_OID;
        }

        // Version and Auth
        if (profile.SNMP_OverrideVersionAndAuth)
        {
            info.Version = profile.SNMP_Version;
            info.Community = profile.SNMP_Community;
            info.Security = profile.SNMP_Security;
            info.Username = profile.SNMP_Username;
            info.AuthenticationProvider = profile.SNMP_AuthenticationProvider;
            info.Auth = profile.SNMP_Auth;
            info.PrivacyProvider = profile.SNMP_PrivacyProvider;
            info.Priv = profile.SNMP_Priv;
        }
        else if (group.SNMP_OverrideVersionAndAuth)
        {
            info.Version = group.SNMP_Version;
            info.Community = group.SNMP_Community;
            info.Security = group.SNMP_Security;
            info.Username = group.SNMP_Username;
            info.AuthenticationProvider = group.SNMP_AuthenticationProvider;
            info.Auth = group.SNMP_Auth;
            info.PrivacyProvider = group.SNMP_PrivacyProvider;
            info.Priv = group.SNMP_Priv;
        }

        return info;
    }
}