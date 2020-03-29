using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="KeyboardHookMode"/>.
    /// </summary>
    public class RemoteDesktopKeyboardHookModeTranslator : SingletonBase<RemoteDesktopKeyboardHookModeTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// </summary>
        private const string _identifier = "RemoteDesktopKeyboardHookMode_";

        /// <summary>
        /// Method to translate <see cref="KeyboardHookMode"/>.
        /// </summary>
        /// <param name="value"><see cref="KeyboardHookMode"/>.</param>
        /// <returns>Translated <see cref="KeyboardHookMode"/>.</returns>
        public string Translate(string value)
        {
            var translation = Resources.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }

        /// <summary>
        /// Method to translate <see cref="KeyboardHookMode"/>.
        /// </summary>
        /// <param name="keyboardHookMode"><see cref="KeyboardHookMode"/>.</param>
        /// <returns>Translated <see cref="KeyboardHookMode"/>.</returns>
        public string Translate(KeyboardHookMode keyboardHookMode)
        {
            return Translate(keyboardHookMode.ToString());
        }
    }
}
