using NETworkManager.Profiles;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="GroupViewName"/>.
    /// </summary>
    public class GroupViewNameTranslator : SingletonBase<GroupViewNameTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// For groups this is the same identifier as for the profiles.
        /// </summary>
        private const string _identifier = "ProfileViewName_";

        /// <summary>
        /// Method to translate <see cref="GroupViewName"/>.
        /// </summary>
        /// <param name="value"><see cref="GroupViewName"/> as <see cref="string"/>.</param>
        /// <returns>Translated <see cref="GroupViewName"/>.</returns>
        public string Translate(string value)
        {
            var translation = Resources.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }

        /// <summary>
        /// Method to translate <see cref="GroupViewName"/>.
        /// </summary>
        /// <param name="name"><see cref="GroupViewName"/>.</param>
        /// <returns>Translated <see cref="GroupViewName"/>.</returns>
        public string Translate(GroupViewName name)
        {
            return Translate(name.ToString());
        }
    }
}