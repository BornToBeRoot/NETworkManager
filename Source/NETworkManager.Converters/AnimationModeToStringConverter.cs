using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization;
using NETworkManager.Models.Appearance;

namespace NETworkManager.Converters;

/// <summary>
///     Convert <see cref="AnimationMode" /> to translated <see cref="string" />.
/// </summary>
public sealed class AnimationModeToStringConverter : IValueConverter
{
    /// <summary>
    ///     Convert <see cref="AnimationMode" /> to translated <see cref="string" />.
    /// </summary>
    /// <param name="value">Object from type <see cref="AnimationMode" />.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Translated <see cref="AnimationMode" />.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not AnimationMode animationMode
            ? "-/-"
            : ResourceTranslator.Translate(ResourceIdentifier.AnimationMode, animationMode);
    }

    /// <summary>
    ///     !!! Method not implemented !!!
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
