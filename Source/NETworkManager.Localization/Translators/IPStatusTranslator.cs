using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    public class IPStatusTranslator : SingletonBase<IPStatusTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files
        /// </summary>
        private const string _identifier = "IPStatus_";

        /// <summary>
        /// Method to translate IP status.
        /// </summary>
        /// <param name="value">IP status.</param>
        /// <returns>Translated IP status.</returns>
        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }
    }
}
