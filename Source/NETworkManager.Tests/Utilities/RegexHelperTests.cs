using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using NETworkManager.Utilities;
using Xunit;

namespace NETworkManager.Tests.Utilities;

public class RegexHelperTests
{
    [Theory]
    [InlineData("00:11:22:33:44:55", true)]
    [InlineData("00-11-22-33-44-55", true)]
    [InlineData("001122334455", true)]
    [InlineData("0011.2233.4455", true)]
    [InlineData("0011 2233 4455", false)]
    [InlineData("0011x2233x4455", false)]
    [InlineData("00:11:22:33:44", false)]
    public void MACAddress(string input, bool isMatch)
    {
        Assert.Equal(isMatch, Regex.IsMatch(input, RegexHelper.MACAddressRegex));
    }

    [Theory]
    [InlineData("00:11:22", true)]
    [InlineData("001122", true)]
    [InlineData("0011.22", true)]
    [InlineData("0011 22", false)]
    [InlineData("0011x22", false)]
    public void MACAddressFirst3Bytes(string input, bool isMatch)
    {
        Assert.Equal(isMatch, Regex.IsMatch(input, RegexHelper.MACAddressFirst3BytesRegex));
    }

    [Theory]
    [InlineData("2001:db8::/32", true)]
    [InlineData("::ffff:192.168.0.1/96", true)]
    [InlineData("sss2001:db8::/32", false)]
    [InlineData("2001:db8::/129", false)]
    public void IPv6AddressCidr(string input, bool isMatch)
    {
        Assert.Equal(isMatch, Regex.IsMatch(input, RegexHelper.IPv6AddressCidrRegex));
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("3389", true)]
    [InlineData("65535", true)]
    [InlineData("", false)]
    [InlineData("0", false)]
    [InlineData("65536", false)]
    [InlineData("05555", false)]
    public void Port(string input, bool isMatch)
    {
        Assert.Equal(isMatch, Regex.IsMatch(input, RegexHelper.PortRegex));
    }

    [Theory]
    [InlineData("server-01", true)]
    [InlineData("server-01.example.com", true)]
    [InlineData("example.com.", true)]
    [InlineData("123abc", true)]
    [InlineData("-server", false)]
    [InlineData("server-", false)]
    [InlineData("server.-bad-.com", false)]
    [InlineData("a..b", false)]
    // IPv4 addresses, shortened or partly typed ones are not hostnames (checked separately)
    [InlineData("10.0.0.1", false)]
    [InlineData("10.0.0.", false)]
    [InlineData("192.168.1", false)]
    [InlineData("1234", false)]
    public void HostnameOrDomain(string input, bool isMatch)
    {
        Assert.Equal(isMatch, RegexHelper.HostnameOrDomainRegex().IsMatch(input));
    }

    [Theory]
    [InlineData("borntoberoot.net", true)]
    [InlineData("local.example.com", true)]
    [InlineData("example", false)]
    public void Domain(string input, bool isMatch)
    {
        Assert.Equal(isMatch, Regex.IsMatch(input, RegexHelper.DomainRegex));
    }

    [Theory]
    [InlineData("22", true)]
    [InlineData("22/tcp", true)]
    [InlineData("22/TCP", true)]
    [InlineData("80-90", true)]
    [InlineData("80-90/udp", true)]
    [InlineData("22;80-90/tcp;443", true)]
    [InlineData("22;", false)]
    [InlineData("ssh", false)]
    [InlineData("22/http", false)]
    [InlineData("123456", false)]
    public void PortLookupEntries(string input, bool isMatch)
    {
        Assert.Equal(isMatch, RegexHelper.PortLookupEntriesRegex().IsMatch(input));
    }

    [Theory]
    [InlineData("22", true, false, false, false)]
    [InlineData("22/tcp", false, true, false, false)]
    [InlineData("80-90", false, false, true, false)]
    [InlineData("80-90/sctp", false, false, false, true)]
    [InlineData("ssh", false, false, false, false)]
    public void PortLookupEntry(string input, bool port, bool portAndProtocol, bool portRange,
        bool portRangeAndProtocol)
    {
        Assert.Equal(port, RegexHelper.PortLookupPortRegex().IsMatch(input));
        Assert.Equal(portAndProtocol, RegexHelper.PortLookupPortAndProtocolRegex().IsMatch(input));
        Assert.Equal(portRange, RegexHelper.PortLookupPortRangeRegex().IsMatch(input));
        Assert.Equal(portRangeAndProtocol, RegexHelper.PortLookupPortRangeAndProtocolRegex().IsMatch(input));
    }

    [Theory]
    [InlineData("00:11:22", true)]
    [InlineData("00-11-22", true)]
    [InlineData("001122", false)]
    [InlineData("facade", false)]
    public void MACAddressFirst3BytesWithSeparator(string input, bool isMatch)
    {
        Assert.Equal(isMatch, RegexHelper.MACAddressFirst3BytesWithSeparatorRegex().IsMatch(input));
    }

    [Theory]
    [InlineData("AS13335", true)]
    [InlineData("as13335", true)]
    [InlineData("13335", false)]
    [InlineData("AS", false)]
    public void ASNumberWithPrefix(string input, bool isMatch)
    {
        Assert.Equal(isMatch, RegexHelper.ASNumberWithPrefixRegex().IsMatch(input));
    }

    [Theory]
    [InlineData("https://borntoberoot.net", true)]
    [InlineData("HTTP://borntoberoot.net", true)]
    [InlineData("ftp://borntoberoot.net", false)]
    [InlineData("borntoberoot.net", false)]
    public void HttpOrHttpsScheme(string input, bool isMatch)
    {
        Assert.Equal(isMatch, RegexHelper.HttpOrHttpsSchemeRegex().IsMatch(input));
    }

    // An invalid input with many labels took exponentially long (catastrophic backtracking).
    [Fact]
    public void DomainAndHostnameNoBacktracking()
    {
        var input = "a" + string.Concat(Enumerable.Repeat(".a", 100)) + "!";

        var stopwatch = Stopwatch.StartNew();

        Assert.DoesNotMatch(RegexHelper.DomainRegex, input);
        Assert.DoesNotMatch(RegexHelper.HostnameOrDomainRegex(), input);

        Assert.True(stopwatch.ElapsedMilliseconds < 1000);
    }
}
