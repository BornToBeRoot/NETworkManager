using System;

namespace NETworkManager.Models.Network;

/// <summary>
///     Position of a WiFi channel on the X axis of a channel chart. The axis is in channel-number space (5 MHz per
///     unit, linear with the frequency). In the 5 GHz chart the unused range between channel 64 (UNII-2) and channel
///     100 (UNII-2 Extended) is compressed; the 6 GHz band is split into a lower (1 - 125) and an upper (129 - 233)
///     chart.
/// </summary>
public static class WiFiChannelAxis
{
    /// <summary>
    ///     Channel that splits the 6 GHz band into the lower (1 - 125) and the upper (129 - 233) chart. It sits between
    ///     the operating channels 125 and 129.
    /// </summary>
    public const int SixGHzSplitChannel = 127;

    // The 5 GHz range 64 - 100 is shown with 16 units instead of 36: channels above 64 are shifted left, leaving a
    // small gap that still has room for the channel-width trapezoids.
    private const int FiveGHzGapStartChannel = 64;
    private const int FiveGHzGapEndChannel = 100;
    private const int FiveGHzGapDisplayUnits = 16;
    private const int FiveGHzGapShift = FiveGHzGapEndChannel - FiveGHzGapStartChannel - FiveGHzGapDisplayUnits;

    /// <summary>
    ///     Converts a frequency (e.g. the center of a channel) to channel-number space (5 MHz per unit).
    /// </summary>
    /// <param name="frequencyMHz">Frequency in MHz like 2422 or 5210.</param>
    /// <param name="radio">Band of the frequency.</param>
    /// <returns>Channel number, fractional between two channels (e.g. 3 for 2422 MHz, 42 for 5210 MHz).</returns>
    public static double FrequencyToChannel(double frequencyMHz, WiFiRadio radio)
    {
        return radio switch
        {
            // Channel 14 is at 2484 MHz and does not follow the standard ch×5 + 2407 spacing.
            WiFiRadio.GHz2dot4 => (int)Math.Round(frequencyMHz) == 2484 ? 14.0 : (frequencyMHz - 2407) / 5.0,
            WiFiRadio.GHz5 => (frequencyMHz - 5000) / 5.0,
            // Channel 2 is at 5935 MHz, below channel 1 (5955 MHz) - drawn at its real position (-3)
            WiFiRadio.GHz6 => (frequencyMHz - 5950) / 5.0,
            _ => 0
        };
    }

    /// <summary>
    ///     Maps a channel number to its position on the X axis (the 5 GHz gap is compressed).
    /// </summary>
    /// <param name="channel">Channel number (see <see cref="FrequencyToChannel" />).</param>
    /// <param name="radio">Band of the channel.</param>
    /// <returns>Position on the X axis.</returns>
    public static double ChannelToPosition(double channel, WiFiRadio radio)
    {
        if (radio == WiFiRadio.GHz5 && channel > FiveGHzGapStartChannel)
            return channel - FiveGHzGapShift;

        return channel;
    }

    /// <summary>
    ///     Maps a position on the X axis back to the channel number (the inverse of <see cref="ChannelToPosition" />),
    ///     for the labels of the axis.
    /// </summary>
    /// <param name="position">Position on the X axis (rounded to a whole channel).</param>
    /// <param name="radio">Band of the chart.</param>
    /// <returns>Channel number at the position.</returns>
    public static int PositionToChannel(int position, WiFiRadio radio)
    {
        if (radio == WiFiRadio.GHz5 && position > FiveGHzGapStartChannel)
            return position + FiveGHzGapShift;

        return position;
    }

    /// <summary>
    ///     Gets the 6 GHz charts a channel is drawn in: the lower (1 - 125), the upper (129 - 233) or both, if the
    ///     channel crosses the split (e.g. 320 MHz around channel 127 covers 95 - 159).
    /// </summary>
    /// <param name="centerFrequencyMHz">Center frequency of the whole channel in MHz.</param>
    /// <param name="bandwidthMHz">Channel width in MHz (20, 40, 80, 160, 320).</param>
    /// <returns>Whether the channel is drawn in the lower and / or the upper chart.</returns>
    public static (bool Lower, bool Upper) GetSixGHzCharts(double centerFrequencyMHz, int bandwidthMHz)
    {
        var center = FrequencyToChannel(centerFrequencyMHz, WiFiRadio.GHz6);

        // Channel-number space uses 5 MHz per unit: half of the width is bandwidth / 10 units
        var halfWidth = bandwidthMHz / 10.0;

        return (center - halfWidth < SixGHzSplitChannel, center + halfWidth > SixGHzSplitChannel);
    }
}
