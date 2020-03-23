using NETworkManager.Models.Network;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="DiscoveryProtocol"/>.
    /// </summary>
    public class DiscoveryProtocolTranslator : SingletonBase<DiscoveryProtocolTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// </summary>
        private const string _identifier = "DiscoveryProtocolIdentifier_";

        /// <summary>
        /// Method to translate <see cref="DiscoveryProtocol"/>.
        /// </summary>
        /// <param name="value"><see cref="DiscoveryProtocol"/>.</param>
        /// <returns>Translated <see cref="DiscoveryProtocol"/>.</returns>
        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }

        /// <summary>
        /// Method to translate <see cref="DiscoveryProtocol"/>.
        /// </summary>
        /// <param name="discoveryProtocol"><see cref="DiscoveryProtocol"/>.</param>
        /// <returns>Translated <see cref="DiscoveryProtocol"/>.</returns>
        public string Translate(DiscoveryProtocol discoveryProtocol)
        {
            return Translate(discoveryProtocol.ToString());
        }
    }
}
