using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using NETworkManager.Models.Network;
using NETworkManager.Profiles;
using NETworkManager.Utilities;
using Xunit;
using IPScannerProfile = NETworkManager.Profiles.Application.IPScanner;

namespace NETworkManager.Tests.Profiles;

/// <summary>
///     SNMP override of the IP Scanner in profiles and groups: the profile before its group, the group before the
///     settings, and the secrets in the profile file.
/// </summary>
public class IPScannerSNMPCredentialsTests
{
    // Same options as ProfileManager
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        Converters = { new JsonStringEnumConverter() }
    };

    private static ProfileInfo CreateProfile(bool overrideSNMP)
    {
        return new ProfileInfo
        {
            Name = "Switches",
            Group = "Site A",
            IPScanner_OverrideSNMP = overrideSNMP,
            IPScanner_SNMPVersion = SNMPVersion.V2C,
            IPScanner_SNMPCommunity = SecureStringHelper.ConvertToSecureString("profile-community")
        };
    }

    private static GroupInfo CreateGroup(bool overrideSNMP)
    {
        return new GroupInfo
        {
            Name = "Site A",
            Profiles = [],
            IPScanner_OverrideSNMP = overrideSNMP,
            IPScanner_SNMPVersion = SNMPVersion.V3,
            IPScanner_SNMPSecurity = SNMPV3Security.AuthPriv,
            IPScanner_SNMPUsername = "monitor",
            IPScanner_SNMPAuthenticationProvider = SNMPV3AuthenticationProvider.SHA256,
            IPScanner_SNMPAuth = SecureStringHelper.ConvertToSecureString("group-auth"),
            IPScanner_SNMPPrivacyProvider = SNMPV3PrivacyProvider.AES256,
            IPScanner_SNMPPriv = SecureStringHelper.ConvertToSecureString("group-priv")
        };
    }

    [Fact]
    public void Profile_BeforeGroup()
    {
        var credentials = IPScannerProfile.GetSNMPCredentials(CreateProfile(true), CreateGroup(true));

        Assert.Equal(SNMPVersion.V2C, credentials.Version);
        Assert.Equal("profile-community", credentials.Community);
    }

    [Fact]
    public void Group_IfProfileDoesNotOverride()
    {
        var credentials = IPScannerProfile.GetSNMPCredentials(CreateProfile(false), CreateGroup(true));

        Assert.Equal(SNMPVersion.V3, credentials.Version);
        Assert.Equal(SNMPV3Security.AuthPriv, credentials.Security);
        Assert.Equal("monitor", credentials.Username);
        Assert.Equal(SNMPV3AuthenticationProvider.SHA256, credentials.AuthenticationProvider);
        Assert.Equal("group-auth", credentials.Auth);
        Assert.Equal(SNMPV3PrivacyProvider.AES256, credentials.PrivacyProvider);
        Assert.Equal("group-priv", credentials.Priv);
    }

    [Fact]
    public void Settings_IfNeitherOverrides()
    {
        Assert.Null(IPScannerProfile.GetSNMPCredentials(CreateProfile(false), CreateGroup(false)));
        Assert.Null(IPScannerProfile.GetSNMPCredentials(CreateProfile(false), null));
    }

    [Fact]
    public void Secrets_SavedAndLoaded()
    {
        var group = CreateGroup(true);
        group.Profiles = [CreateProfile(true)];

        var json = JsonSerializer.Serialize(new ProfileFileData { Groups = [group] }, JsonOptions);
        var loaded = JsonSerializer.Deserialize<ProfileFileData>(json, JsonOptions);

        var loadedGroup = loaded.Groups.Single();
        var loadedProfile = loadedGroup.Profiles.Single();

        Assert.True(loadedGroup.IPScanner_OverrideSNMP);
        Assert.Equal("monitor", loadedGroup.IPScanner_SNMPUsername);
        Assert.Equal("group-auth", SecureStringHelper.ConvertToString(loadedGroup.IPScanner_SNMPAuth));
        Assert.Equal("group-priv", SecureStringHelper.ConvertToString(loadedGroup.IPScanner_SNMPPriv));

        Assert.True(loadedProfile.IPScanner_OverrideSNMP);
        Assert.Equal("profile-community", SecureStringHelper.ConvertToString(loadedProfile.IPScanner_SNMPCommunity));

        // Not set: not saved as a secret
        Assert.Null(loadedProfile.IPScanner_SNMPAuth);
    }

    [Fact]
    public void CopyConstructors_KeepTheOverride()
    {
        var profile = new ProfileInfo(CreateProfile(true));
        var group = new GroupInfo(CreateGroup(true));

        Assert.True(profile.IPScanner_OverrideSNMP);
        Assert.Equal("profile-community", SecureStringHelper.ConvertToString(profile.IPScanner_SNMPCommunity));
        Assert.True(group.IPScanner_OverrideSNMP);
        Assert.Equal("group-priv", SecureStringHelper.ConvertToString(group.IPScanner_SNMPPriv));
    }

    [Fact]
    public void OldProfileFile_WithoutOverride()
    {
        // A profile file of an older version has no IP Scanner SNMP values
        const string json = """{ "Groups": [ { "Name": "Site A", "Profiles": [ { "Name": "Switches" } ] } ] }""";

        var loaded = JsonSerializer.Deserialize<ProfileFileData>(json, JsonOptions);

        var profile = loaded.Groups.Single().Profiles.Single();

        Assert.False(profile.IPScanner_OverrideSNMP);
        Assert.Null(IPScannerProfile.GetSNMPCredentials(profile, loaded.Groups.Single()));
        Assert.Equal(new List<string>(), [.. loaded.Groups.Where(x => x.IPScanner_OverrideSNMP).Select(x => x.Name)]);
    }
}
