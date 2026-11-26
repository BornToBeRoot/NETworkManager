using NETworkManager.Models.Cloudflare;
using Xunit;

namespace NETworkManager.Tests.Cloudflare;

public class SpeedTestResultTests
{
    [Fact]
    public void ServerWithAllValues()
    {
        Assert.Equal("Frankfurt-am-Main, DE (FRA)",
            new SpeedTestResult { ServerCity = "Frankfurt-am-Main", ServerCountry = "DE", ServerIata = "FRA" }.Server);
    }

    [Fact]
    public void ServerWithoutValuesIsNull()
    {
        // Shown as "-/-" instead of "-/-, -/- (-/-)"
        Assert.Null(new SpeedTestResult().Server);
        Assert.Null(new SpeedTestResult { ServerCity = " ", ServerCountry = "" }.Server);
    }

    [Fact]
    public void ServerWithoutCity()
    {
        Assert.Equal("DE (FRA)", new SpeedTestResult { ServerCountry = "DE", ServerIata = "FRA" }.Server);
    }

    [Fact]
    public void ServerWithOnlyTheCode()
    {
        Assert.Equal("FRA", new SpeedTestResult { ServerIata = "FRA" }.Server);
    }

    [Fact]
    public void ServerWithoutCode()
    {
        Assert.Equal("Frankfurt-am-Main, DE",
            new SpeedTestResult { ServerCity = "Frankfurt-am-Main", ServerCountry = "DE" }.Server);
    }
}
