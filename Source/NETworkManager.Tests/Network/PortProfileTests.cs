using System;
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

    [Fact]
    public void EncodeReferences_ReplacesNamesById()
    {
        var input = PortProfile.EncodeReferences("webserver; 8080; Database", PortProfiles);

        Assert.Equal($"{PortProfiles[0].Id:B}; 8080; {PortProfiles[2].Id:B}", input);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("22; 80")]
    [InlineData("Unknown")]
    public void EncodeReferences_NullWithoutPortProfile(string input)
    {
        Assert.Null(PortProfile.EncodeReferences(input, PortProfiles));
    }

    [Fact]
    public void DecodeReferences_CurrentNameAfterRename()
    {
        var portProfile = new PortProfileInfo("Web", "80");
        var input = PortProfile.EncodeReferences("Web; 22", [portProfile]);

        portProfile.Name = "Web (renamed)";

        Assert.True(PortProfile.TryDecodeReferences(input, [portProfile], out var decoded));
        Assert.Equal("Web (renamed); 22", decoded);
    }

    [Fact]
    public void DecodeReferences_FalseIfPortProfileIsMissing()
    {
        Assert.False(PortProfile.TryDecodeReferences($"{Guid.NewGuid():B}; 22", PortProfiles, out var decoded));
        Assert.Null(decoded);
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

    [Fact]
    public void Encode_PortsAndReferences()
    {
        var (ports, portsInput) = PortProfile.Encode("Webserver; 8080", PortProfiles);

        Assert.Equal("80; 443; 8080", ports);
        Assert.Equal($"{PortProfiles[0].Id:B}; 8080", portsInput);
    }

    [Fact]
    public void Encode_WithoutPortProfile()
    {
        var (ports, portsInput) = PortProfile.Encode(" 22; 80 ", PortProfiles);

        Assert.Equal("22; 80", ports);
        Assert.Null(portsInput);
    }

    [Fact]
    public void Decode_CurrentNameAfterRename()
    {
        var portProfile = new PortProfileInfo("Web", "80");
        var (ports, portsInput) = PortProfile.Encode("Web; 22", [portProfile]);

        portProfile.Name = "Web (renamed)";

        Assert.Equal("Web (renamed); 22", PortProfile.Decode(ports, portsInput, [portProfile], out var notFound));
        Assert.False(notFound);
    }

    [Fact]
    public void Decode_SavedPortsWithoutReferences()
    {
        Assert.Equal("22; 80", PortProfile.Decode("22; 80", null, PortProfiles, out var notFound));
        Assert.False(notFound);
    }

    [Fact]
    public void Decode_SavedPortsIfPortProfileIsMissing()
    {
        Assert.Equal("80; 22", PortProfile.Decode("80; 22", $"{Guid.NewGuid():B}; 22", PortProfiles,
            out var notFound));
        Assert.True(notFound);
    }
}
