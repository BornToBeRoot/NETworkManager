using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    public class ConnectionStateTranslator : SingletonBase<ConnectionStateTranslator>, ILocalizationStringTranslator
    {    
        /// <summary>
        /// Constant to identify the strings in the language files
        /// </summary>
        private const string _identifier = "ConnectionState_";

        /// <summary>
        /// Method to translate connection state.
        /// </summary>
        /// <param name="value">Connection state.</param>
        /// <returns>Translated connection state.</returns>
        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }
    }
}
