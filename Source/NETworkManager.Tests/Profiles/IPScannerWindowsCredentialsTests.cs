using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using NETworkManager.Profiles;
using NETworkManager.Utilities;
using Xunit;
using IPScannerProfile = NETworkManager.Profiles.Application.IPScanner;

namespace NETworkManager.Tests.Profiles;

/// <summary>
///     Windows credentials of the IP Scanner in profiles and groups: the profile before its group, the group before the
///     user running the application (no Windows credentials in the settings), and the password in the profile file.
/// </summary>
public class IPScannerWindowsCredentialsTests
{
    // Same options as ProfileManager
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
        Converters = { new JsonStringEnumConverter() }
    };

    private static ProfileInfo CreateProfile(bool useCredentials)
    {
        return new ProfileInfo
        {
            Name = "Client 1",
            Group = "Site A",
            IPScanner_UseWindowsCredentials = useCredentials,
            IPScanner_WindowsUsername = "localadmin",
            IPScanner_WindowsDomain = ".",
            IPScanner_WindowsPassword = SecureStringHelper.ConvertToSecureString("profile-password")
        };
    }

    private static GroupInfo CreateGroup(bool useCredentials)
    {
        return new GroupInfo
        {
            Name = "Site A",
            Profiles = [],
            IPScanner_UseWindowsCredentials = useCredentials,
            IPScanner_WindowsUsername = "admin",
            IPScanner_WindowsDomain = "corp.example.com",
            IPScanner_WindowsPassword = SecureStringHelper.ConvertToSecureString("group-password")
        };
    }

    [Fact]
    public void Profile_BeforeGroup()
    {
        var credentials = IPScannerProfile.GetWindowsCredentials(CreateProfile(true), CreateGroup(true));

        Assert.Equal("localadmin", credentials.Username);
        Assert.Equal(".", credentials.Domain);
        Assert.Equal("profile-password", SecureStringHelper.ConvertToString(credentials.Password));
    }

    [Fact]
    public void Group_IfProfileDoesNotUseCredentials()
    {
        var credentials = IPScannerProfile.GetWindowsCredentials(CreateProfile(false), CreateGroup(true));

        Assert.Equal("admin", credentials.Username);
        Assert.Equal("corp.example.com", credentials.Domain);
        Assert.Equal("group-password", SecureStringHelper.ConvertToString(credentials.Password));
    }

    [Fact]
    public void CurrentUser_IfNeitherUsesCredentials()
    {
        Assert.Null(IPScannerProfile.GetWindowsCredentials(CreateProfile(false), CreateGroup(false)));
        Assert.Null(IPScannerProfile.GetWindowsCredentials(CreateProfile(false), null));
    }

    [Fact]
    public void Protected_CopyWithoutPasswordInToString()
    {
        var group = CreateGroup(true);

        var credentials = IPScannerProfile.GetWindowsCredentials(CreateProfile(false), group);

        // Copy of the password (a change of the profile doesn't change an open tab), no secret in ToString
        Assert.NotSame(group.IPScanner_WindowsPassword, credentials.Password);
        Assert.Equal(@"corp.example.com\admin", credentials.ToString());
        Assert.Equal("admin", (credentials with { Domain = string.Empty }).DisplayName);
        Assert.Equal(@"CORP\admin", (credentials with { Username = @"CORP\admin", Domain = "CORP" }).DisplayName);
    }

    [Fact]
    public void Password_SavedAndLoaded()
    {
        var group = CreateGroup(true);
        group.Profiles = [CreateProfile(true)];

        var json = JsonSerializer.Serialize(new ProfileFileData { Groups = [group] }, JsonOptions);
        var loaded = JsonSerializer.Deserialize<ProfileFileData>(json, JsonOptions);

        var loadedGroup = loaded.Groups.Single();
        var loadedProfile = loadedGroup.Profiles.Single();

        Assert.True(loadedGroup.IPScanner_UseWindowsCredentials);
        Assert.Equal("admin", loadedGroup.IPScanner_WindowsUsername);
        Assert.Equal("corp.example.com", loadedGroup.IPScanner_WindowsDomain);
        Assert.Equal("group-password", SecureStringHelper.ConvertToString(loadedGroup.IPScanner_WindowsPassword));

        Assert.True(loadedProfile.IPScanner_UseWindowsCredentials);
        Assert.Equal("profile-password", SecureStringHelper.ConvertToString(loadedProfile.IPScanner_WindowsPassword));
    }

    [Fact]
    public void Password_NotSet_NotSaved()
    {
        var group = new GroupInfo { Name = "Site A", Profiles = [new ProfileInfo { Name = "Client 1" }] };

        var json = JsonSerializer.Serialize(new ProfileFileData { Groups = [group] }, JsonOptions);
        var loaded = JsonSerializer.Deserialize<ProfileFileData>(json, JsonOptions);

        Assert.Null(loaded.Groups.Single().IPScanner_WindowsPassword);
        Assert.Null(loaded.Groups.Single().Profiles.Single().IPScanner_WindowsPassword);
    }

    [Fact]
    public void CopyConstructors_KeepTheCredentials()
    {
        var profile = new ProfileInfo(CreateProfile(true));
        var group = new GroupInfo(CreateGroup(true));

        Assert.True(profile.IPScanner_UseWindowsCredentials);
        Assert.Equal("localadmin", profile.IPScanner_WindowsUsername);
        Assert.Equal("profile-password", SecureStringHelper.ConvertToString(profile.IPScanner_WindowsPassword));
        Assert.True(group.IPScanner_UseWindowsCredentials);
        Assert.Equal("corp.example.com", group.IPScanner_WindowsDomain);
        Assert.Equal("group-password", SecureStringHelper.ConvertToString(group.IPScanner_WindowsPassword));
    }

    [Fact]
    public void OldProfileFile_WithoutCredentials()
    {
        // A profile file of an older version has no IP Scanner Windows credentials
        const string json = """{ "Groups": [ { "Name": "Site A", "Profiles": [ { "Name": "Client 1" } ] } ] }""";

        var loaded = JsonSerializer.Deserialize<ProfileFileData>(json, JsonOptions);

        var group = loaded.Groups.Single();
        var profile = group.Profiles.Single();

        Assert.False(profile.IPScanner_UseWindowsCredentials);
        Assert.False(group.IPScanner_UseWindowsCredentials);
        Assert.Null(IPScannerProfile.GetWindowsCredentials(profile, group));
    }
}
