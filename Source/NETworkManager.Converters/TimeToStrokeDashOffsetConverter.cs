using System;
using System.Globalization;
using System.Windows.Data;

namespace NETworkManager.Converters;

/// <summary>
///     Converts <c>Time</c> (values[0]) and <c>TimeMax</c> (values[1]) into a <see cref="double" />
///     stroke dash offset for a circular countdown ring. <c>ConverterParameter</c> must be the full
///     circumference of the ring in <c>StrokeDashArray</c> units (circumference / StrokeThickness).
/// </summary>
public sealed class TimeToStrokeDashOffsetConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (parameter is not string circumferenceStr ||
            !double.TryParse(circumferenceStr, NumberStyles.Float, CultureInfo.InvariantCulture,
                out var circumference))
            return 0.0;

        if (values.Length < 2 ||
            values[0] is not double time ||
            values[1] is not double timeMax ||
            timeMax == 0 ||
            time <= 0)
            return circumference + 1.0;

        var progress = (double)time / timeMax;

        return (1.0 - progress) * circumference;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
