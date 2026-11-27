using NETworkManager.Models.IPApi;
using Xunit;

namespace NETworkManager.Tests.IPApi;

public class IPGeolocationInfoTests
{
    [Theory]
    [InlineData(0, "UTC+00:00")] // e.g. London in winter (was shown as "-/-")
    [InlineData(3600, "UTC+01:00")]
    [InlineData(19800, "UTC+05:30")]
    [InlineData(-12600, "UTC-03:30")]
    [InlineData(-36000, "UTC-10:00")]
    [InlineData(50400, "UTC+14:00")]
    public void UtcOffsetIsFormatted(int offsetSeconds, string expected)
    {
        Assert.Equal(expected, IPGeolocationInfo.FormatUtcOffset(offsetSeconds));
    }

    [Theory]
    [InlineData("Hesse", "HE", "Hesse (HE)")]
    [InlineData("Hesse", "", "Hesse")]
    [InlineData("Hesse", null, "Hesse")]
    [InlineData("", "HE", "HE")]
    [InlineData(null, null, null)]
    [InlineData("", "", null)]
    public void NameWithCodeHasNoEmptyBrackets(string name, string code, string expected)
    {
        Assert.Equal(expected, IPGeolocationInfo.FormatNameWithCode(name, code));
    }

    [Fact]
    public void RegionWithCodeUsesTheRegionName()
    {
        var info = new IPGeolocationInfo { RegionName = "Hesse", Region = "HE" };

        Assert.Equal("Hesse (HE)", info.RegionWithCode);
    }
}
