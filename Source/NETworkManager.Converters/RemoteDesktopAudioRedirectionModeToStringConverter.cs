using System;
using System.Globalization;
using System.Windows.Data;
using NETworkManager.Localization;
using NETworkManager.Models.RemoteDesktop;

namespace NETworkManager.Converters;

/// <summary>
/// Convert <see cref="AudioRedirectionMode"/> to translated <see cref="string"/> or wise versa.
/// </summary>
public sealed class RemoteDesktopAudioRedirectionModeToStringConverter : IValueConverter
{
    /// <summary>
    /// Convert <see cref="AudioRedirectionMode"/> to translated <see cref="string"/>. 
    /// </summary>
    /// <param name="value">Object from type <see cref="AudioRedirectionMode"/>.</param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns>Translated <see cref="AudioRedirectionMode"/>.</returns>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is not AudioRedirectionMode audioRedirectionMode ? "-/-" : ResourceTranslator.Translate(ResourceIdentifier.RemoteDesktopAudioRedirectionMode, audioRedirectionMode);
    }

    /// <summary>
    /// !!! Method not implemented !!!
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
