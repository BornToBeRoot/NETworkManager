using NETworkManager.Models.Network;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="ConnectionState"/>.
    /// </summary>
    public class ConnectionStateTranslator : SingletonBase<ConnectionStateTranslator>, ILocalizationStringTranslator
    {    
        /// <summary>
        /// Constant to identify the strings in the language files
        /// </summary>
        private const string _identifier = "ConnectionState_";

        /// <summary>
        /// Method to translate <see cref="ConnectionState"/>.
        /// </summary>
        /// <param name="value"><see cref="ConnectionState"/> as <see cref="string"/>.</param>
        /// <returns>Translated <see cref="ConnectionState"/>.</returns>
        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }

        /// <summary>
        /// Method to translate <see cref="ConnectionState"/>.
        /// </summary>
        /// <param name="connectionState"><see cref="ConnectionState"/>.</param>
        /// <returns>Translated <see cref="ConnectionState"/>.</returns>
        public string Translate(ConnectionState connectionState)
        {
            return Translate(connectionState.ToString());
        }
    }
}
