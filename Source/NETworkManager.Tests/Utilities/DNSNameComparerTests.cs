using NETworkManager.Utilities;
using Xunit;

namespace NETworkManager.Tests.Utilities;

public class DNSNameComparerTests
{
    [Theory]
    [InlineData("Printer.local.", "printer.LOCAL.")]
    [InlineData("B\u00fcro.local.", "b\u00fcro.local.")]
    public void AsciiLettersAreComparedCaseInsensitively(string x, string y)
    {
        Assert.True(DNSNameComparer.Instance.Equals(x, y));
        Assert.Equal(DNSNameComparer.Instance.GetHashCode(x), DNSNameComparer.Instance.GetHashCode(y));
    }

    [Theory]
    [InlineData("B\u00fcro.local.", "B\u00dcro.local.")] // u umlaut: only ASCII letters are folded
    [InlineData("\u212a.local.", "k.local.")] // Kelvin sign
    [InlineData("\u0131.local.", "i.local.")] // dotless i
    [InlineData("a.local.", "a.local")]
    public void OtherCharactersAreComparedExactly(string x, string y) =>
        Assert.False(DNSNameComparer.Instance.Equals(x, y));

    [Fact]
    public void ToLowerOnlyChangesAsciiLetters() =>
        Assert.Equal("office b\u00dcro.local.", DNSNameComparer.ToLower("Office B\u00dcRO.Local."));

    [Theory]
    [InlineData("Printer._IPP._tcp.local.", "._ipp._tcp.local.", true)]
    [InlineData("Printer._ipp._tcp.local.", "._ipps._tcp.local.", false)]
    [InlineData("local.", "x.local.", false)]
    public void EndsWith(string name, string suffix, bool expected) =>
        Assert.Equal(expected, DNSNameComparer.EndsWith(name, suffix));
}
