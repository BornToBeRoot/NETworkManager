using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="AutoRefreshTime.TimeUnit" />
    /// </summary>
    public class TimeUnitTranslator : SingletonBase<TimeUnitTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// </summary>
        private const string _identifier = "TimeUnit_";

        /// <summary>
        /// Method to translate <see cref="AutoRefreshTime.TimeUnit"/>.
        /// </summary>
        /// <param name="value"><see cref="AutoRefreshTime.TimeUnit"/> as <see cref="string"/>.</param>
        /// <returns>Translated <see cref="AutoRefreshTime.TimeUnit"/>.</returns>
        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }

        /// <summary>
        /// Method to translate <see cref="AutoRefreshTime.TimeUnit"/>.
        /// </summary>
        /// <param name="timeUnit"><see cref="AutoRefreshTime.TimeUnit"/>.</param>
        /// <returns>Translated <see cref="AutoRefreshTime.TimeUnit"/>.</returns>
        public string Translate(AutoRefreshTime.TimeUnit timeUnit)
        {
            return Translate(timeUnit.ToString());
        }
    }
}