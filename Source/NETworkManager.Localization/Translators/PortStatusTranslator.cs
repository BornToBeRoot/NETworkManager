using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    public class PortStatusTranslator : SingletonBase<PortStatusTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files
        /// </summary>
        private const string _identifier = "PortState_";

        /// <summary>
        /// Method to translate port status.
        /// </summary>
        /// <param name="value">Port status.</param>
        /// <returns>Translated port status.</returns>
        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }
    }
}
