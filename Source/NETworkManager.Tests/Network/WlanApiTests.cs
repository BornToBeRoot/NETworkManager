using System.Linq;
using NETworkManager.Models.Network;
using Xunit;

namespace NETworkManager.Tests.Network;

public class WlanApiTests
{
    // HT Operation (61): primary channel, info byte (bits 0-1 secondary channel offset, bit 2 STA channel width),
    // 20 more bytes
    private static byte[] HtOperation(byte info)
    {
        return [61, 22, 0, info, .. new byte[20]];
    }

    // VHT Operation (192): channel width, CCFS0, CCFS1, basic VHT-MCS and NSS set (2)
    private static byte[] VhtOperation(byte channelWidth, byte segment0, byte segment1)
    {
        return [192, 5, channelWidth, segment0, segment1, 0, 0];
    }

    // HE Operation (255 / 36) with the 6 GHz Operation Information: primary channel, control (bits 0-1 channel
    // width), CCFS0, CCFS1, minimum rate
    private static byte[] HeOperation6GHz(byte control, byte segment0, byte segment1)
    {
        // Parameters (3, B17 = 6 GHz Operation Information present), BSS color (1), basic HE-MCS and NSS set (2)
        byte[] body = [36, 0x00, 0x00, 0x02, 0, 0, 0, 1, control, segment0, segment1, 0];
        return [255, (byte)body.Length, .. body];
    }

    // EHT Operation (255 / 106): parameters (bit 0 = EHT Operation Information present), basic EHT-MCS and NSS set (4),
    // control (bits 0-2 channel width), CCFS0, CCFS1
    private static byte[] EhtOperation(byte control, byte segment0, byte segment1)
    {
        byte[] body = [106, 0x01, 0, 0, 0, 0, control, segment0, segment1];
        return [255, (byte)body.Length, .. body];
    }

    private static WlanApi.WlanBssChannel Parse(int primaryFrequencyMHz, params byte[][] elements)
    {
        return WlanApi.GetChannelFromInformationElements(elements.SelectMany(x => x).ToArray(), primaryFrequencyMHz);
    }

    [Fact]
    public void NoOperationElement_IsUnknown()
    {
        // SSID element only
        Assert.Equal(default, Parse(5180, [0, 4, 0x54, 0x65, 0x73, 0x74]));
    }

    [Fact]
    public void Ht20_IsCenteredOnThePrimaryChannel()
    {
        Assert.Equal(new WlanApi.WlanBssChannel(20, 2437), Parse(2437, HtOperation(0x00)));
    }

    [Theory]
    [InlineData(0x05, 2422)] // Secondary channel above: channels 1 + 5, center channel 3
    [InlineData(0x07, 2442)] // Secondary channel below: channels 9 + 5, center channel 7 (primary channel 9 = 2452)
    public void Ht40_IsCenteredBetweenPrimaryAndSecondaryChannel(byte info, int expectedCenter)
    {
        var primary = info == 0x05 ? 2412 : 2452;

        Assert.Equal(new WlanApi.WlanBssChannel(40, expectedCenter), Parse(primary, HtOperation(info)));
    }

    [Fact]
    public void Ht40WithoutSecondaryChannel_Is20()
    {
        Assert.Equal(new WlanApi.WlanBssChannel(20, 2412), Parse(2412, HtOperation(0x04)));
    }

    [Fact]
    public void Vht80_IsCenteredOnSegment0()
    {
        // Primary channel 36, 80 MHz on the channels 36 - 48 (center channel 42)
        Assert.Equal(new WlanApi.WlanBssChannel(80, 5210), Parse(5180, HtOperation(0x05), VhtOperation(1, 42, 0)));
    }

    [Fact]
    public void Vht160_IsCenteredOnSegment1()
    {
        // Primary channel 36, 160 MHz on the channels 36 - 64 (center channel 50, segment 0 = primary 80 MHz)
        Assert.Equal(new WlanApi.WlanBssChannel(160, 5250), Parse(5180, HtOperation(0x05), VhtOperation(1, 42, 50)));
    }

    [Fact]
    public void Vht160DeprecatedEncoding_IsCenteredOnSegment0()
    {
        Assert.Equal(new WlanApi.WlanBssChannel(160, 5250), Parse(5180, HtOperation(0x05), VhtOperation(2, 50, 0)));
    }

    [Fact]
    public void Vht20Or40_DefersToHt()
    {
        Assert.Equal(new WlanApi.WlanBssChannel(40, 5190), Parse(5180, HtOperation(0x05), VhtOperation(0, 0, 0)));
    }

    [Fact]
    public void VhtBeforeHt_IsParsedTheSame()
    {
        Assert.Equal(new WlanApi.WlanBssChannel(40, 5190), Parse(5180, VhtOperation(0, 0, 0), HtOperation(0x05)));
    }

    [Theory]
    [InlineData(0, 1, 0, 20, 5955)] // Channel 1
    [InlineData(1, 3, 0, 40, 5965)] // Channels 1 - 5, center channel 3
    [InlineData(2, 7, 0, 80, 5985)] // Channels 1 - 13, center channel 7
    [InlineData(3, 7, 15, 160, 6025)] // Channels 1 - 29, center channel 15 (segment 0 = primary 80 MHz)
    [InlineData(3, 15, 0, 160, 6025)] // Old encoding: segment 0 is the center
    public void He6GHz_IsCenteredOnTheChannelCenter(byte control, byte segment0, byte segment1, int expectedWidth,
        int expectedCenter)
    {
        Assert.Equal(new WlanApi.WlanBssChannel(expectedWidth, expectedCenter),
            Parse(5955, HeOperation6GHz(control, segment0, segment1)));
    }

    [Fact]
    public void He6GHz80Plus80_IsCenteredOnThePrimarySegment()
    {
        // Segments around the channels 7 and 71: 160 MHz around the primary segment (channel 7), like VHT
        Assert.Equal(new WlanApi.WlanBssChannel(160, 5985), Parse(5955, HeOperation6GHz(3, 7, 71)));
    }

    [Fact]
    public void He6GHzChannel2_IsAt5935()
    {
        Assert.Equal(new WlanApi.WlanBssChannel(20, 5935), Parse(5935, HeOperation6GHz(0, 2, 0)));
    }

    [Fact]
    public void Eht320_IsCenteredOnSegment1()
    {
        // Primary channel 1, 320 MHz on the channels 1 - 61 (center channel 31, segment 0 = primary 160 MHz)
        Assert.Equal(new WlanApi.WlanBssChannel(320, 6105),
            Parse(5955, HeOperation6GHz(3, 7, 15), EhtOperation(4, 15, 31)));
    }

    [Fact]
    public void EhtWithoutOperationInformation_KeepsHe()
    {
        byte[] eht = [255, 6, 106, 0x00, 0, 0, 0, 0];

        Assert.Equal(new WlanApi.WlanBssChannel(160, 6025), Parse(5955, HeOperation6GHz(3, 7, 15), eht));
    }

    [Fact]
    public void TruncatedElement_IsIgnored()
    {
        // Length 22, but only 3 bytes follow
        Assert.Equal(default, Parse(5180, [61, 22, 0, 0x05, 0]));
    }
}
