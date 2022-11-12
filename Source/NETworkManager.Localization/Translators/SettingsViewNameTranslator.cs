using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="SettingsViewName"/>.
    /// </summary>
    public class SettingsViewNameTranslator : SingletonBase<SettingsViewNameTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// </summary>
        private const string _identifier = "SettingsViewName_";

        /// <summary>
        /// Method to translate <see cref="SettingsViewName"/>.
        /// </summary>
        /// <param name="value"><see cref="SettingsViewName"/> as <see cref="string"/>.</param>
        /// <returns>Translated <see cref="SettingsViewName"/>.</returns>
        public string Translate(string value)
        {
            // Get the translation for the settings page
            var translation = Resources.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            // If empty, try to get application name
            return string.IsNullOrEmpty(translation) ? ApplicationNameTranslator.GetInstance().Translate(value) : translation;
        }

        /// <summary>
        /// Method to translate <see cref="SettingsViewName"/>.
        /// </summary>
        /// <param name="name"><see cref="SettingsViewName"/>.</param>
        /// <returns>Translated <see cref="SettingsViewName"/>.</returns>
        public string Translate(SettingsViewName name)
        {
            return Translate(name.ToString());
        }
    }
}