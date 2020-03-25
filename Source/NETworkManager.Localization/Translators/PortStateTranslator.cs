using NETworkManager.Models.Network;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="PortState"/>.
    /// </summary>
    public class PortStateTranslator : SingletonBase<PortStateTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files
        /// </summary>
        private const string _identifier = "PortState_";

        /// <summary>
        /// Method to translate <see cref="PortState"/>.
        /// </summary>
        /// <param name="value"><see cref="PortState"/>.</param>
        /// <returns>Translated <see cref="PortState"/>.</returns>
        public string Translate(string value)
        {
            var translation = Resources.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }

        /// <summary>
        /// Method to translate <see cref="PortState"/>.
        /// </summary>
        /// <param name="portState"><see cref="PortState"/>.</param>
        /// <returns>Translated <see cref="PortState"/>.</returns>
        public string Translate(PortState portState)
        {
            return Translate(portState.ToString());
        }
    }
}
