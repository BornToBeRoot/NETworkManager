using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Utilities;

namespace NETworkManager.Converters;

/// <summary>
/// Converts a byte-per-second value to a human-readable speed string.
/// Pass the converter parameter "bytes" to format as byte/s (e.g. "1.54 MB/s");
/// any other value (default) formats as bit/s (e.g. "12.3 Mbit/s") to match the chart axis.
/// </summary>
public sealed class BandwidthBytesToSpeedConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return "-/-";

        var bytesPerSecond = (long)value;

        return parameter as string == "bytes"
            ? $"{FileSizeConverter.GetBytesReadable(bytesPerSecond)}/s"
            : $"{FileSizeConverter.GetBytesReadable(bytesPerSecond * 8)}it/s";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
