using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="SettingsViewGroup"/>.
    /// </summary>
    public class SettingsViewGroupTranslator : SingletonBase<SettingsViewGroupTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// </summary>
        private const string _identifier = "SettingsViewGroup_";
        
        /// <summary>
        /// Method to translate <see cref="SettingsViewGroup"/>.
        /// </summary>
        /// <param name="value"><see cref="SettingsViewGroup"/> as <see cref="string"/>.</param>
        /// <returns>Translated <see cref="SettingsViewGroup"/>.</returns>
        public string Translate(string value)
        {
            var translation = Resources.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }

        /// <summary>
        /// Method to translate <see cref="SettingsViewGroup"/>.
        /// </summary>
        /// <param name="name"><see cref="SettingsViewGroup"/>.</param>
        /// <returns>Translated <see cref="SettingsViewGroup"/>.</returns>
        public string Translate(SettingsViewGroup group)
        {
            return Translate(group.ToString());
        }
    }
}