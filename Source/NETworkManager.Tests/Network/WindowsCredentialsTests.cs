using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using Xunit;

namespace NETworkManager.Tests.Network;

/// <summary>
///     Windows credentials of a scan: a local account (".") is logged on with the computer name of each host.
/// </summary>
public class WindowsCredentialsTests
{
    private static WindowsCredentials Create(string username, string domain)
    {
        return new WindowsCredentials(username, domain, SecureStringHelper.ConvertToSecureString("password"));
    }

    [Theory]
    [InlineData("admin", ".")]
    [InlineData(@".\admin", "")]
    [InlineData(@".\admin", "CORP")] // the domain of the username is used
    public void ForHost_LocalAccount_ComputerNameAsDomain(string username, string domain)
    {
        var credentials = Create(username, domain);

        var host = credentials.ForHost("PC01");

        Assert.True(credentials.IsLocalAccount);
        Assert.Equal(@"PC01\admin", host.DisplayName);
        Assert.Same(credentials.Password, host.Password);
    }

    [Theory]
    [InlineData("admin", "CORP")]
    [InlineData(@"CORP\admin", "")]
    [InlineData("admin@corp.example.com", "")]
    [InlineData("admin", "")]
    public void ForHost_NoLocalAccount_Null(string username, string domain)
    {
        var credentials = Create(username, domain);

        Assert.False(credentials.IsLocalAccount);
        Assert.Null(credentials.ForHost("PC01"));
    }

    [Fact]
    public void ForHost_WithoutComputerName_Null()
    {
        Assert.Null(Create("admin", ".").ForHost(string.Empty));
    }
}
