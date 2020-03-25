using NETworkManager.Models;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="Application.Name"/>.
    /// </summary>
    public class ApplicationNameTranslator : SingletonBase<ApplicationNameTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// </summary>
        private const string _identifier = "ApplicationName_";

        /// <summary>
        /// Method to translate <see cref="Application.Name"/>.
        /// </summary>
        /// <param name="value"><see cref="Application.Name"/> as <see cref="string"/>.</param>
        /// <returns>Translated <see cref="Application.Name"/>.</returns>
        public string Translate(string value)
        {
            var translation = Resources.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }

        /// <summary>
        /// Method to translate <see cref="Application.Name"/>.
        /// </summary>
        /// <param name="name"><see cref="Application.Name"/>.</param>
        /// <returns>Translated <see cref="Application.Name"/>.</returns>
        public string Translate(Application.Name name)
        {
            return Translate(name.ToString());
        }
    }
}