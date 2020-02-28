using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    public class RemoteDesktopNetworkConnectionTypeTranslator : SingletonBase<RemoteDesktopNetworkConnectionTypeTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files
        /// </summary>
        private const string _identifier = "RemoteDesktopNetworkConnectionType_";

        /// <summary>
        /// Method to translate remote desktop network connection type.
        /// </summary>
        /// <param name="value">Remote desktop network connection type.</param>
        /// <returns>Translated remote desktop network connection type.</returns>
        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }
    }
}
