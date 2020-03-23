using NETworkManager.Models.Network;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="DiscoveryProtocol.Protocol"/>.
    /// </summary>
    public class DiscoveryProtocolTranslator : SingletonBase<DiscoveryProtocolTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// </summary>
        private const string _identifier = "DiscoveryProtocolIdentifier_";

        /// <summary>
        /// Method to translate <see cref="DiscoveryProtocol.Protocol"/>.
        /// </summary>
        /// <param name="value"><see cref="DiscoveryProtocol.Protocol"/>.</param>
        /// <returns>Translated <see cref="DiscoveryProtocol.Protocol"/>.</returns>
        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }
    }
}
