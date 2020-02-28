using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    public class DiscoveryProtocolTranslator : SingletonBase<DiscoveryProtocolTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files
        /// </summary>
        private const string _identifier = "DiscoveryProtocolIdentifier_";

        /// <summary>
        /// Method to translate discovery protocol.
        /// </summary>
        /// <param name="value">Discovery protocol.</param>
        /// <returns>Translated discovery protocol.</returns>
        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }
    }
}
