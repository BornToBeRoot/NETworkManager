using MahApps.Metro;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="Accent"/>.
    /// </summary>
    public class AccentTranslator : SingletonBase<AccentTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// </summary>
        private const string _identifier = "Accent_";

        /// <summary>
        /// Method to translate <see cref="Accent"/>.
        /// </summary>
        /// <param name="value"><see cref="Accent"/> as <see cref="string"/>.</param>
        /// <returns>Translated <see cref="Accent"/>.</returns>
        public string Translate(string value)
        {
            var translation = Resources.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }

        /// <summary>
        /// Method to translate <see cref="Accent"/>.
        /// </summary>
        /// <param name="accent"><see cref="Accent"/>.</param>
        /// <returns>Translated <see cref="Accent"/>.</returns>
        public string Translate(Accent accent)
        {
            return Translate(accent.ToString());
        }
    }
}