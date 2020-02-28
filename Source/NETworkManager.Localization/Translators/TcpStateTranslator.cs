using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    public class TcpStateTranslator : SingletonBase<TcpStateTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files
        /// </summary>
        private const string _identifier = "TcpState_";

        /// <summary>
        /// Method to translate TCP state.
        /// </summary>
        /// <param name="value">TCP state.</param>
        /// <returns>Translated TCP state.</returns>
        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }
    }
}