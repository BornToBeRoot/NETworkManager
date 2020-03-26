using NETworkManager.Models.Application;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="Name"/>.
    /// </summary>
    public class ApplicationNameTranslator : SingletonBase<ApplicationNameTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// </summary>
        private const string _identifier = "ApplicationName_";

        /// <summary>
        /// Method to translate <see cref="Name"/>.
        /// </summary>
        /// <param name="value"><see cref="Name"/> as <see cref="string"/>.</param>
        /// <returns>Translated <see cref="Name"/>.</returns>
        public string Translate(string value)
        {
            var translation = Resources.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }

        /// <summary>
        /// Method to translate <see cref="Name"/>.
        /// </summary>
        /// <param name="name"><see cref="Name"/>.</param>
        /// <returns>Translated <see cref="Name"/>.</returns>
        public string Translate(Name name)
        {
            return Translate(name.ToString());
        }
    }
}