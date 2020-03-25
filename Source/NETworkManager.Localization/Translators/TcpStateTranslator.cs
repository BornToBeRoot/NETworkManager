using NETworkManager.Utilities;
using System.Net.NetworkInformation;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="TcpState"/>.
    /// </summary>
    public class TcpStateTranslator : SingletonBase<TcpStateTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// </summary>
        private const string _identifier = "TcpState_";


        /// <summary>
        /// Method to translate <see cref="TcpState"/>.
        /// </summary>
        /// <param name="value"><see cref="TcpState"/>.</param>
        /// <returns>Translated <see cref="TcpState"/>.</returns>
        public string Translate(string value)
        {
            var translation = Resources.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }

        /// <summary>
        /// Method to translate <see cref="TcpState"/>.
        /// </summary>
        /// <param name="tcpState"><see cref="TcpState"/>.</param>
        /// <returns>Translated <see cref="TcpState"/>.</returns>
        public string Translate(TcpState tcpState)
        {
            return Translate(tcpState.ToString());
        }
    }
}