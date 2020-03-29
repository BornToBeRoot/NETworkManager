using NETworkManager.Models.PuTTY;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="LogMode"/>.
    /// </summary>
    public class PuTTYLogModeTranslator : SingletonBase<PuTTYLogModeTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// </summary>
        private const string _identifier = "PuTTYLogMode_";

        /// <summary>
        /// Method to translate <see cref="LogMode"/>.
        /// </summary>
        /// <param name="value"><see cref="LogMode"/>.</param>
        /// <returns>Translated <see cref="LogMode"/>.</returns>
        public string Translate(string value)
        {
            var translation = Resources.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }

        /// <summary>
        /// Method to translate <see cref="LogMode"/>.
        /// </summary>
        /// <param name="logMode"><see cref="LogMode"/>.</param>
        /// <returns>Translated <see cref="LogMode"/>.</returns>
        public string Translate(LogMode logMode)
        {
            return Translate(logMode.ToString());
        }
    }
}