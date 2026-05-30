using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization.Resources;

namespace NETworkManager.Converters;

/// <summary>
/// Converts a byte count to its exact value with thousands separators and a "Bytes" unit
/// (e.g. "6,783,176,192 Bytes"). Intended for tooltips that complement a human-readable size.
/// </summary>
public sealed class BytesToExactStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value != null ? string.Format(culture, "{0:N0} {1}", (long)value, Strings.Bytes) : "-/-";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
