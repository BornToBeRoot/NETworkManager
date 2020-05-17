using ControlzEx.Theming;
using MahApps.Metro;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="AppTheme"/>.
    /// </summary>
    public class AppThemeTranslator : SingletonBase<AppThemeTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// </summary>
        private const string _identifier = "AppTheme_";

        /// <summary>
        /// Method to translate <see cref="AppTheme"/>.
        /// </summary>
        /// <param name="value"><see cref="AppTheme"/> as <see cref="string"/>.</param>
        /// <returns>Translated <see cref="AppTheme"/>.</returns>
        public string Translate(string value)
        {
            var translation = Resources.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }

        /// <summary>
        /// Method to translate <see cref="AppTheme"/>.
        /// </summary>
        /// <param name="appTheme"><see cref="AppTheme"/>.</param>
        /// <returns>Translated <see cref="AppTheme"/>.</returns>
        public string Translate(Theme appTheme)
        {
            return Translate(appTheme.ToString());
        }
    }
}