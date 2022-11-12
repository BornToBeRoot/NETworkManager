using NETworkManager.Profiles;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="ProfileViewName"/>.
    /// </summary>
    public class ProfileViewNameTranslator : SingletonBase<ProfileViewNameTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// </summary>
        private const string _identifier = "ProfileViewName_";

        /// <summary>
        /// Method to translate <see cref="ProfileViewName"/>.
        /// </summary>
        /// <param name="value"><see cref="ProfileViewName"/> as <see cref="string"/>.</param>
        /// <returns>Translated <see cref="ProfileViewName"/>.</returns>
        public string Translate(string value)
        {
            // Get the translation for the profile page
            var translation = Resources.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            // If empty, try to get application name
            return string.IsNullOrEmpty(translation) ? ApplicationNameTranslator.GetInstance().Translate(value) : translation;
        }

        /// <summary>
        /// Method to translate <see cref="ProfileViewName"/>.
        /// </summary>
        /// <param name="name"><see cref="ProfileViewName"/>.</param>
        /// <returns>Translated <see cref="ProfileViewName"/>.</returns>
        public string Translate(ProfileViewName name)
        {
            return Translate(name.ToString());
        }
    }
}