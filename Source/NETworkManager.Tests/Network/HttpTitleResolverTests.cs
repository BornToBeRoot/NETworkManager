using System.Text;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class HttpTitleResolverTests
{
    [Theory]
    [InlineData("<html><head><title>Router Login</title></head></html>", "Router Login")]
    [InlineData("<TITLE>Upper case</TITLE>", "Upper case")]
    [InlineData("<title id=\"t\">With attribute</title>", "With attribute")]
    [InlineData("<title>\r\n   Multi\r\n   line  \r\n</title>", "Multi line")]
    [InlineData("<title>Tom &amp; Jerry &#8211; &quot;NAS&quot;</title>", "Tom & Jerry – \"NAS\"")]
    [InlineData("<title>First</title><title>Second</title>", "First")]
    [InlineData("<html><body>No title</body></html>", "")]
    [InlineData("<title>Not closed", "")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void ParseTitle(string html, string expected)
    {
        Assert.Equal(expected, HttpTitleResolver.ParseTitle(html));
    }

    [Fact]
    public void ParseTitle_TooLong_Shortened()
    {
        var title = HttpTitleResolver.ParseTitle($"<title>{new string('a', 500)}</title>");

        Assert.Equal(new string('a', 200) + "...", title);
    }

    [Theory]
    [InlineData(80, new[] { "http" })]
    [InlineData(8080, new[] { "http" })]
    [InlineData(443, new[] { "https", "http" })]
    [InlineData(9000, new[] { "https", "http" })]
    public void GetSchemes(int port, string[] expected)
    {
        Assert.Equal(expected, HttpTitleResolver.GetSchemes(port));
    }

    [Fact]
    public void Decode_DefaultUTF8()
    {
        Assert.Equal("<title>Grüße</title>",
            HttpTitleResolver.Decode(Encoding.UTF8.GetBytes("<title>Grüße</title>"), null));
    }

    [Fact]
    public void Decode_CharsetOfHeader()
    {
        Assert.Equal("<title>Grüße</title>",
            HttpTitleResolver.Decode(Encoding.Latin1.GetBytes("<title>Grüße</title>"), "iso-8859-1"));
    }

    [Fact]
    public void Decode_CharsetOfMetaTag()
    {
        const string html = "<meta charset=\"ISO-8859-1\"><title>Grüße</title>";

        Assert.Equal(html, HttpTitleResolver.Decode(Encoding.Latin1.GetBytes(html), null));
    }

    [Fact]
    public void Decode_UnknownCharset_UTF8()
    {
        Assert.Equal("<title>Test</title>",
            HttpTitleResolver.Decode(Encoding.UTF8.GetBytes("<title>Test</title>"), "unknown-charset"));
    }
}
