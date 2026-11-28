using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization;
using NETworkManager.Models.Network;

namespace NETworkManager.Converters;

/// <summary>
///     Convert <see cref="LatencyPosition" /> to translated <see cref="string" />.
/// </summary>
public sealed class LatencyPositionToStringConverter : IValueConverter
{
    /// <summary>
    ///     Convert <see cref="LatencyPosition" /> to translated <see cref="string" />.
    /// </summary>
    /// <param name="value">Object from type <see cref="LatencyPosition" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Translated <see cref="LatencyPosition" />.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not LatencyPosition position
            ? "-/-"
            : ResourceTranslator.Translate(ResourceIdentifier.LatencyPosition, position);
    }

    /// <summary>
    ///     !!! Method not implemented !!!
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
