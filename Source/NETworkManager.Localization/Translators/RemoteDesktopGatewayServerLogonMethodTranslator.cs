using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators;

/// <summary>
/// Class to translate <see cref="GatewayUserSelectedCredsSource"/>.
/// </summary>
public class RemoteDesktopGatewayServerLogonMethodTranslator : SingletonBase<RemoteDesktopGatewayServerLogonMethodTranslator>, ILocalizationStringTranslator
{
    /// <summary>
    /// Constant to identify the strings in the language files.
    /// </summary>
    private const string _identifier = "RemoteDesktopGatewayServerLogonMethod_";

    /// <summary>
    /// Method to translate <see cref="GatewayUserSelectedCredsSource"/>.
    /// </summary>
    /// <param name="value"><see cref="GatewayUserSelectedCredsSource"/>.</param>
    /// <returns>Translated <see cref="GatewayUserSelectedCredsSource"/>.</returns>
    public string Translate(string value)
    {
        var translation = Resources.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

        return string.IsNullOrEmpty(translation) ? value : translation;
    }

    /// <summary>
    /// Method to translate <see cref="GatewayUserSelectedCredsSource"/>.
    /// </summary>
    /// <param name="gatewayUserSelectedCredsSource"><see cref="GatewayUserSelectedCredsSource"/>.</param>
    /// <returns>Translated <see cref="GatewayUserSelectedCredsSource"/>.</returns>
    public string Translate(GatewayUserSelectedCredsSource gatewayUserSelectedCredsSource)
    {
        return Translate(gatewayUserSelectedCredsSource.ToString());
    }
}