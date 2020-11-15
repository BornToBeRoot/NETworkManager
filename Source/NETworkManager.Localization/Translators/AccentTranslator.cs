using NETworkManager.Models.Appearance;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    
    ///<summary>
    /// Class to translate <see cref="Accent"/>.
    /// </summary>
    public class AccentTranslator : SingletonBase<AccentTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// </summary>
        private const string _identifier = "Accent_";

        /// <summary>
        /// Method to translate <see cref="AccentColorInfo"/>.
        /// </summary>
        /// <param name="value"><see cref="AccentColorInfo"/> as <see cref="string"/>.</param>
        /// <returns>Translated <see cref="AccentColorInfo"/>.</returns>
        public string Translate(string value)
        {
            var translation = Resources.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }
    }
}