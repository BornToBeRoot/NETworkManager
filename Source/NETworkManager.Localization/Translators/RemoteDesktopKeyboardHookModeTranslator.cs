using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    public class RemoteDesktopKeyboardHookModeTranslator : SingletonBase<RemoteDesktopKeyboardHookModeTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files
        /// </summary>
        private const string _identifier = "RemoteDesktopKeyboardHookMode_";

        /// <summary>
        /// Method to translate remote desktop keyboard hook mode.
        /// </summary>
        /// <param name="value">Remote desktop keyboard hook mode.</param>
        /// <returns>Translated remote desktop keyboard hook mode.</returns>
        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }
    }
}
