using System.Linq;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class WiFiChannelTests
{
    [Theory]
    [InlineData(2.412, 1)]
    [InlineData(2.472, 13)]
    [InlineData(2.484, 14)]
    [InlineData(5.160, 32)]
    [InlineData(5.180, 36)]
    [InlineData(5.825, 165)]
    [InlineData(5.885, 177)]
    [InlineData(5.935, 2)]
    [InlineData(5.955, 1)]
    [InlineData(7.115, 233)]
    [InlineData(5.000, -1)]
    public void GetChannelFromChannelFrequency_ReturnsTheChannel(double gigahertz, int expected)
    {
        Assert.Equal(expected, WiFi.GetChannelFromChannelFrequency(gigahertz));
    }

    [Theory]
    [InlineData(WiFiRadio.GHz2dot4, 1, 14)]
    [InlineData(WiFiRadio.GHz5, 32, 177)]
    [InlineData(WiFiRadio.GHz6, 1, 233)]
    public void GetFrequencyFromChannel_IsTheInverseOfGetChannelFromChannelFrequency(WiFiRadio radio, int first,
        int last)
    {
        // Every channel of the band (also 2.4 GHz channel 14 and 6 GHz channel 2) back and forth, in the same band
        int[] channels = radio == WiFiRadio.GHz6 ? [2, .. Enumerable.Range(first, last - first + 1)]
            : [.. Enumerable.Range(first, last - first + 1)];

        foreach (var channel in channels)
        {
            var gigahertz = WiFi.GetFrequencyFromChannel(channel, radio) / 1000.0;

            Assert.Equal(channel, WiFi.GetChannelFromChannelFrequency(gigahertz));
            Assert.Equal(radio, WiFi.GetWiFiRadioFromChannelFrequency(gigahertz));
        }
    }

    [Theory]
    [InlineData(2.412, WiFiRadio.GHz2dot4)]
    [InlineData(2.484, WiFiRadio.GHz2dot4)]
    [InlineData(5.180, WiFiRadio.GHz5)]
    [InlineData(5.885, WiFiRadio.GHz5)]
    [InlineData(5.935, WiFiRadio.GHz6)]
    [InlineData(7.115, WiFiRadio.GHz6)]
    [InlineData(3.000, WiFiRadio.Unknown)]
    public void GetWiFiRadioFromChannelFrequency_ReturnsTheBand(double gigahertz, WiFiRadio expected)
    {
        Assert.Equal(expected, WiFi.GetWiFiRadioFromChannelFrequency(gigahertz));
    }
}
