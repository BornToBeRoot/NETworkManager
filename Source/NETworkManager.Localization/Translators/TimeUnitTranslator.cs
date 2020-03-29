using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="TimeUnit" />
    /// </summary>
    public class TimeUnitTranslator : SingletonBase<TimeUnitTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// </summary>
        private const string _identifier = "TimeUnit_";

        /// <summary>
        /// Method to translate <see cref="TimeUnit"/>.
        /// </summary>
        /// <param name="value"><see cref="TimeUnit"/> as <see cref="string"/>.</param>
        /// <returns>Translated <see cref="TimeUnit"/>.</returns>
        public string Translate(string value)
        {
            var translation = Resources.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }

        /// <summary>
        /// Method to translate <see cref="TimeUnit"/>.
        /// </summary>
        /// <param name="timeUnit"><see cref="TimeUnit"/>.</param>
        /// <returns>Translated <see cref="TimeUnit"/>.</returns>
        public string Translate(TimeUnit timeUnit)
        {
            return Translate(timeUnit.ToString());
        }
    }
}