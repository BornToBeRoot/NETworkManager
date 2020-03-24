using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="AudioRedirectionMode"/>.
    /// </summary>
    public class RemoteDesktopAudioRedirectionModeTranslator : SingletonBase<RemoteDesktopAudioRedirectionModeTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// </summary>
        private const string _identifier = "RemoteDesktopAudioRedirectionMode_";

        /// <summary>
        /// Method to translate <see cref="AudioRedirectionMode"/>.
        /// </summary>
        /// <param name="value"><see cref="AudioRedirectionMode"/>.</param>
        /// <returns>Translated <see cref="AudioRedirectionMode"/>.</returns>
        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }

        /// <summary>
        /// Method to translate <see cref="AudioRedirectionMode"/>.
        /// </summary>
        /// <param name="audioRedirectionMode"><see cref="AudioRedirectionMode"/>.</param>
        /// <returns>Translated <see cref="AudioRedirectionMode"/>.</returns>
        public string Translate(AudioRedirectionMode audioRedirectionMode)
        {
            return Translate(audioRedirectionMode.ToString());
        }
    }
}
