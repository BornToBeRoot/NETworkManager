using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="NetworkConnectionType"/>.
    /// </summary>
    public class RemoteDesktopNetworkConnectionTypeTranslator : SingletonBase<RemoteDesktopNetworkConnectionTypeTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// </summary>
        private const string _identifier = "RemoteDesktopNetworkConnectionType_";

        /// <summary>
        /// Method to translate <see cref="NetworkConnectionType"/>.
        /// </summary>
        /// <param name="value"><see cref="NetworkConnectionType"/>.</param>
        /// <returns>Translated <see cref="NetworkConnectionType"/>.</returns>
        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }

        /// <summary>
        /// Method to translate <see cref="NetworkConnectionType"/>.
        /// </summary>
        /// <param name="networkConnectionType"><see cref="NetworkConnectionType"/>.</param>
        /// <returns>Translated <see cref="NetworkConnectionType"/>.</returns>
        public string Translate(NetworkConnectionType networkConnectionType)
        {
            return Translate(networkConnectionType.ToString());
        }
    }
}
