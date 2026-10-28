using System;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class NamedReferenceInputTests
{
    private sealed class Item(Guid id, string name) : INamedReference
    {
        public Guid Id { get; } = id;
        public string Name { get; } = name;
    }

    private static readonly Item Alpha = new(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Alpha");
    private static readonly Item Beta = new(Guid.Parse("22222222-2222-2222-2222-222222222222"), " Beta ");
    private static readonly Item WithoutId = new(Guid.Empty, "Legacy");

    private static readonly Item[] Items = [Alpha, Beta, WithoutId];

    private static Item Find(string entry) => NamedReferenceInput.FindByName(Items, entry);

    [Theory]
    [InlineData("Alpha", "Alpha")]
    [InlineData("alpha", "Alpha")]
    [InlineData("  ALPHA ", "Alpha")]
    [InlineData("Beta", " Beta ")]
    public void FindByName_IgnoresCaseAndSpaces(string name, string expected)
    {
        Assert.Equal(expected, NamedReferenceInput.FindByName(Items, name)?.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("Gamma")]
    public void FindByName_NullIfNotFound(string name)
    {
        Assert.Null(NamedReferenceInput.FindByName(Items, name));
    }

    [Fact]
    public void EncodeReferences_ReplacesNamesAndTrimsOtherEntries()
    {
        Assert.Equal("{11111111-1111-1111-1111-111111111111}; 8080; {22222222-2222-2222-2222-222222222222}",
            NamedReferenceInput.EncodeReferences(" alpha ;8080 ; Beta", Find));
    }

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    [InlineData("8080; Gamma")]
    [InlineData("Legacy")]
    public void EncodeReferences_NullWithoutReference(string input)
    {
        Assert.Null(NamedReferenceInput.EncodeReferences(input, Find));
    }

    [Fact]
    public void EncodeReferences_KeepsEntriesSkippedByFindItem()
    {
        Assert.Equal("Alpha; {22222222-2222-2222-2222-222222222222}",
            NamedReferenceInput.EncodeReferences("Alpha; Beta", entry => entry.Trim() == "Alpha" ? null : Find(entry)));
    }

    [Fact]
    public void DecodeReferences_RoundTrip()
    {
        var encoded = NamedReferenceInput.EncodeReferences("Alpha; 8080", Find);

        Assert.True(NamedReferenceInput.TryDecodeReferences(encoded, Items, out var input));
        Assert.Equal("Alpha; 8080", input);
    }

    [Fact]
    public void DecodeReferences_CurrentName()
    {
        var renamed = new Item(Alpha.Id, "Alpha (renamed)");

        Assert.True(NamedReferenceInput.TryDecodeReferences("{11111111-1111-1111-1111-111111111111};  8080 ",
            [renamed], out var input));
        Assert.Equal("Alpha (renamed); 8080", input);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("{33333333-3333-3333-3333-333333333333}; 8080")]
    public void DecodeReferences_FalseIfEmptyOrItemIsMissing(string inputWithReferences)
    {
        Assert.False(NamedReferenceInput.TryDecodeReferences(inputWithReferences, Items, out var input));
        Assert.Null(input);
    }
}
