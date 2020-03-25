using NETworkManager.Models.Network;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="Protocol"/>.
    /// </summary>
    public class DiscoveryProtocolTranslator : SingletonBase<DiscoveryProtocolTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// </summary>
        private const string _identifier = "DiscoveryProtocolIdentifier_";

        /// <summary>
        /// Method to translate <see cref="Protocol"/>.
        /// </summary>
        /// <param name="value"><see cref="Protocol"/>.</param>
        /// <returns>Translated <see cref="Protocol"/>.</returns>
        public string Translate(string value)
        {
            var translation = Resources.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }

        /// <summary>
        /// Method to translate <see cref="Protocol"/>.
        /// </summary>
        /// <param name="discoveryProtocol"><see cref="Protocol"/>.</param>
        /// <returns>Translated <see cref="Protocol"/>.</returns>
        public string Translate(Protocol discoveryProtocol)
        {
            return Translate(discoveryProtocol.ToString());
        }
    }
}
