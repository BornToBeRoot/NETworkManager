using System.Linq;
using NETworkManager.Models.Network;
using NETworkManager.Utilities;
using Xunit;

namespace NETworkManager.Tests.Network;

public class MDNSServiceTypeProfileTests
{
    private static readonly MDNSServiceTypeProfileInfo[] Profiles =
    [
        new("Printers", "_ipp._tcp; _ipps._tcp"),
        new("Web", "_http._tcp; _https._tcp"),
        new("Broken", "_ipp._tcp; printers")
    ];

    [Theory]
    [InlineData("Printers")]
    [InlineData("My printers (office)")]
    [InlineData("ipp")]
    public void ValidNames(string name) => Assert.True(MDNSServiceTypeProfile.IsValidName(name));

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("*")]
    [InlineData("_ipp._tcp")]
    [InlineData("A; B")]
    public void InvalidNames(string name) => Assert.False(MDNSServiceTypeProfile.IsValidName(name));

    [Fact]
    public void ParsesTheServiceTypesOfAProfile()
    {
        Assert.True(MDNSServiceTypeProfile.TryParseServiceTypes(" _IPP._tcp; _ipps._tcp.local.;_ipp._tcp ", out var types));
        Assert.Equal(new[] { "_ipp._tcp.local.", "_ipps._tcp.local." }, types);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("*")]
    [InlineData("_ipp._tcp; *")]
    [InlineData("_ipp._tcp; Printers")]
    [InlineData("_ipp._tcp;")]
    public void RejectsInvalidServiceTypesOfAProfile(string input) =>
        Assert.False(MDNSServiceTypeProfile.TryParseServiceTypes(input, out _));

    [Fact]
    public void ResolvesProfilesAndServiceTypesWithoutDuplicates()
    {
        Assert.True(MDNSServiceTypeProfile.TryResolve("printers; _http._tcp; _ipp._tcp", Profiles, out var types));
        Assert.Equal(new[] { "_ipp._tcp.local.", "_ipps._tcp.local.", "_http._tcp.local." }, types);
    }

    [Fact]
    public void AllServiceTypesReplaceTheOthers()
    {
        Assert.True(MDNSServiceTypeProfile.TryResolve("Web; *", Profiles, out var types));
        Assert.Equal(new[] { "*" }, types);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Unknown")]
    [InlineData("Printers; Unknown")]
    [InlineData("Broken")]
    [InlineData("_ipp._tcp;")]
    public void RejectsUnknownProfilesAndInvalidEntries(string input) =>
        Assert.False(MDNSServiceTypeProfile.TryResolve(input, Profiles, out _));

    [Fact]
    public void DefaultProfilesAreValidAndUnique()
    {
        var profiles = MDNSServiceTypeProfile.GetDefaultList();

        Assert.All(profiles, x =>
        {
            Assert.True(MDNSServiceTypeProfile.IsValidName(x.Name), x.Name);
            Assert.True(MDNSServiceTypeProfile.TryParseServiceTypes(x.ServiceTypes, out _), x.ServiceTypes);
        });
        Assert.Equal(profiles.Count, profiles.Select(x => x.Name.ToLowerInvariant()).Distinct().Count());
        Assert.Equal(profiles.Count, profiles.Select(x => x.Id).Distinct().Count());
    }

    [Fact]
    public void DefaultProfilesUseNoInvalidServiceNames() =>
        Assert.All(MDNSServiceTypeProfile.GetDefaultList().SelectMany(x => x.ServiceTypes.Split(';')),
            x => Assert.True(MDNSServiceType.TryNormalize(x, out _), x));
}
