using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    public class RemoteDesktopAudioCaptureRedirectionModeTranslator : SingletonBase<RemoteDesktopAudioCaptureRedirectionModeTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files
        /// </summary>
        private const string _identifier = "RemoteDesktopAudioCaptureRedirectionMode_";

        /// <summary>
        /// Method to translate remote desktop audio capture redirection mode.
        /// </summary>
        /// <param name="value">Remote desktop audio capture redirection mode.</param>
        /// <returns>Translated remote desktop audio capture redirection mode.</returns>
        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }
    }
}