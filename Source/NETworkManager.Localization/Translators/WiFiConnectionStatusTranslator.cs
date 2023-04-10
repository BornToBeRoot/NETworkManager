using NETworkManager.Models.PuTTY;
using NETworkManager.Utilities;
using Windows.Devices.WiFi;

namespace NETworkManager.Localization.Translators;

/// <summary>
/// Class to translate <see cref="LogMode"/>.
/// </summary>
public class WiFiConnectionStatusTranslator : SingletonBase<WiFiConnectionStatusTranslator>, ILocalizationStringTranslator
{
    /// <summary>
    /// Constant to identify the strings in the language files.
    /// </summary>
    private const string _identifier = "WiFiConnectionStatus_";

    /// <summary>
    /// Method to translate <see cref="WiFiConnectionStatus"/>.
    /// </summary>
    /// <param name="value"><see cref="WiFiConnectionStatus"/>.</param>
    /// <returns>Translated <see cref="WiFiConnectionStatus"/>.</returns>
    public string Translate(string value)
    {
        var translation = Resources.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

        return string.IsNullOrEmpty(translation) ? value : translation;
    }

    /// <summary>
    /// Method to translate <see cref="WiFiConnectionStatus"/>.
    /// </summary>
    /// <param name="status"><see cref="WiFiConnectionStatus"/>.</param>
    /// <returns>Translated <see cref="WiFiConnectionStatus"/>.</returns>
    public string Translate(WiFiConnectionStatus status)
    {
        return Translate(status.ToString());
    }
}
