using NETworkManager.Settings;
using Xunit;

namespace NETworkManager.Tests.Settings;

public class CommandLineManagerTests
{
    [Theory]
    [InlineData("1280x800", 1280, 800)]
    [InlineData("1200X800", 1200, 800)]
    [InlineData("800x600", 800, 600)]
    public void ParsesTheScreenshotSize(string value, int expectedWidth, int expectedHeight)
    {
        Assert.True(CommandLineManager.TryParseScreenshotSize(value, out var width, out var height));
        Assert.Equal(expectedWidth, width);
        Assert.Equal(expectedHeight, height);
    }

    [Theory]
    [InlineData("")]
    [InlineData("1280")]
    [InlineData("1280x")]
    [InlineData("x800")]
    [InlineData("1280x800x600")]
    [InlineData("abcxdef")]
    [InlineData("799x600")]
    [InlineData("800x599")]
    [InlineData("-1280x800")]
    [InlineData("7681x800")]
    public void RejectsAnInvalidScreenshotSize(string value)
    {
        Assert.False(CommandLineManager.TryParseScreenshotSize(value, out _, out _));
    }
}
