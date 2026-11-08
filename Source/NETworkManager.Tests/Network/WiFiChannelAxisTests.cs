using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class WiFiChannelAxisTests
{
    [Theory]
    [InlineData(2412, WiFiRadio.GHz2dot4, 1)]
    [InlineData(2484, WiFiRadio.GHz2dot4, 14)]
    // 40 MHz on the channels 1 + 5: center at channel 3
    [InlineData(2422, WiFiRadio.GHz2dot4, 3)]
    // 80 MHz on the channels 36 - 48: center at channel 42 (B19)
    [InlineData(5210, WiFiRadio.GHz5, 42)]
    [InlineData(5180, WiFiRadio.GHz5, 36)]
    [InlineData(5955, WiFiRadio.GHz6, 1)]
    // 6 GHz channel 2 is below channel 1
    [InlineData(5935, WiFiRadio.GHz6, -3)]
    public void FrequencyToChannel_ReturnsTheChannelNumber(double frequencyMHz, WiFiRadio radio, double expected)
    {
        Assert.Equal(expected, WiFiChannelAxis.FrequencyToChannel(frequencyMHz, radio), 6);
    }

    [Theory]
    [InlineData(36, 36)]
    [InlineData(64, 64)]
    // The range 64 - 100 is compressed to 16 units
    [InlineData(100, 80)]
    [InlineData(149, 129)]
    [InlineData(177, 157)]
    public void ChannelToPosition_CompressesTheFiveGHzGap(double channel, double expected)
    {
        Assert.Equal(expected, WiFiChannelAxis.ChannelToPosition(channel, WiFiRadio.GHz5));
    }

    [Theory]
    [InlineData(WiFiRadio.GHz2dot4, 13)]
    [InlineData(WiFiRadio.GHz6, 129)]
    public void ChannelToPosition_KeepsTheChannelOutsideOfFiveGHz(WiFiRadio radio, double channel)
    {
        Assert.Equal(channel, WiFiChannelAxis.ChannelToPosition(channel, radio));
    }

    [Theory]
    [InlineData(36)]
    [InlineData(64)]
    [InlineData(100)]
    [InlineData(144)]
    // Labels above 144 (B9)
    [InlineData(149)]
    [InlineData(165)]
    [InlineData(177)]
    public void PositionToChannel_IsTheInverseOfChannelToPosition(int channel)
    {
        var position = (int)WiFiChannelAxis.ChannelToPosition(channel, WiFiRadio.GHz5);

        Assert.Equal(channel, WiFiChannelAxis.PositionToChannel(position, WiFiRadio.GHz5));
    }

    [Theory]
    // 20 MHz on channel 1 / 233
    [InlineData(5955, 20, true, false)]
    [InlineData(7115, 20, false, true)]
    // 160 MHz on the channels 97 - 125 (center 111) / 129 - 157 (center 143)
    [InlineData(6505, 160, true, false)]
    [InlineData(6665, 160, false, true)]
    // 320 MHz around channel 127 covers 95 - 159: both charts (B32)
    [InlineData(6585, 320, true, true)]
    // Channel 2 (5935 MHz)
    [InlineData(5935, 20, true, false)]
    public void GetSixGHzCharts_ReturnsTheChartsOfTheChannel(double centerFrequencyMHz, int bandwidthMHz,
        bool expectedLower, bool expectedUpper)
    {
        Assert.Equal((expectedLower, expectedUpper),
            WiFiChannelAxis.GetSixGHzCharts(centerFrequencyMHz, bandwidthMHz));
    }
}
