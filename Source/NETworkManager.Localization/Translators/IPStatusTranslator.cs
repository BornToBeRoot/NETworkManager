using NETworkManager.Utilities;
using System.Net.NetworkInformation;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="IPStatus"/>.
    /// </summary>
    public class IPStatusTranslator : SingletonBase<IPStatusTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files
        /// </summary>
        private const string _identifier = "IPStatus_";

        /// <summary>
        /// Method to translate <see cref="IPStatus"/>.
        /// </summary>
        /// <param name="value"><see cref="IPStatus"/>.</param>
        /// <returns>Translated <see cref="IPStatus"/>.</returns>
        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }

        /// <summary>
        /// Method to translate <see cref="IPStatus"/>.
        /// </summary>
        /// <param name="ipStatus"><see cref="IPStatus"/>.</param>
        /// <returns>Translated <see cref="IPStatus"/>.</returns>
        public string Translate(IPStatus ipStatus)
        {
            return Translate(ipStatus.ToString());
        }
    }
}
