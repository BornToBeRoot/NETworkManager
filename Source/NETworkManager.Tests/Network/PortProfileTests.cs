using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class PortProfileTests
{
    private static readonly PortProfileInfo[] PortProfiles =
    [
        new("Webserver", "80; 443"),
        new("Well-known ports", "1-1024"),
        new("Database", "1433-1434; 3306")
    ];

    [Theory]
    [InlineData("80", "80")]
    [InlineData("80; 443; 1-1024", "80; 443; 1-1024")]
    [InlineData("Webserver", "80; 443")]
    [InlineData("webserver", "80; 443")]
    [InlineData(" Webserver ;8080", "80; 443; 8080")]
    [InlineData("Webserver;Database;22", "80; 443; 1433-1434; 3306; 22")]
    [InlineData("Well-known ports; 8080", "1-1024; 8080")]
    public void TryResolvePorts_Valid(string input, string expectedPorts)
    {
        Assert.True(PortProfile.TryResolvePorts(input, PortProfiles, out var ports));
        Assert.Equal(expectedPorts, ports);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Unknown")]
    [InlineData("Webserver; Unknown")]
    [InlineData("80;")]
    [InlineData("70000")]
    public void TryResolvePorts_Invalid(string input)
    {
        Assert.False(PortProfile.TryResolvePorts(input, PortProfiles, out var ports));
        Assert.Null(ports);
    }

    [Theory]
    [InlineData("Webserver", true)]
    [InlineData("Well-known ports", true)]
    [InlineData("Web; Mail", false)]
    [InlineData("80", false)]
    [InlineData("1-1024", false)]
    [InlineData(" ", false)]
    [InlineData(null, false)]
    public void IsValidName(string name, bool isValid)
    {
        Assert.Equal(isValid, PortProfile.IsValidName(name));
    }
}
