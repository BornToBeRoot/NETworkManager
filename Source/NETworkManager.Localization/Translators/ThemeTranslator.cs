using NETworkManager.Models.Appearance;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="AppTheme"/>.
    /// </summary>
    public class ThemeTranslator : SingletonBase<ThemeTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// </summary>
        private const string _identifier = "Theme_";

        /// <summary>
        /// Method to translate <see cref="ThemeColorInfo"/>.
        /// </summary>
        /// <param name="value"><see cref="ThemeColorInfo"/> as <see cref="string"/>.</param>
        /// <returns>Translated <see cref="ThemeColorInfo"/>.</returns>
        public string Translate(string value)
        {
            var translation = Resources.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }
    }
}