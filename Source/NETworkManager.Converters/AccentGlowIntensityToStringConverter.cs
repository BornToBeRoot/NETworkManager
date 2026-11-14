using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization;
using NETworkManager.Models.Appearance;

namespace NETworkManager.Converters;

/// <summary>
///     Convert <see cref="AccentGlowIntensity" /> to translated <see cref="string" />.
/// </summary>
public sealed class AccentGlowIntensityToStringConverter : IValueConverter
{
    /// <summary>
    ///     Convert <see cref="AccentGlowIntensity" /> to translated <see cref="string" />.
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not AccentGlowIntensity intensity
            ? "-/-"
            : ResourceTranslator.Translate(ResourceIdentifier.AccentGlowIntensity, intensity);
    }

    /// <summary>
    ///     !!! Method not implemented !!!
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
