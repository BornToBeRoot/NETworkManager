using NETworkManager.Models.RemoteDesktop;
using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    /// <summary>
    /// Class to translate <see cref="AudioCaptureRedirectionMode"/>.
    /// </summary>
    public class RemoteDesktopAudioCaptureRedirectionModeTranslator : SingletonBase<RemoteDesktopAudioCaptureRedirectionModeTranslator>, ILocalizationStringTranslator
    {
        /// <summary>
        /// Constant to identify the strings in the language files.
        /// </summary>
        private const string _identifier = "RemoteDesktopAudioCaptureRedirectionMode_";

        /// <summary>
        /// Method to translate <see cref="AudioCaptureRedirectionMode"/>.
        /// </summary>
        /// <param name="value"><see cref="AudioCaptureRedirectionMode"/>.</param>
        /// <returns>Translated <see cref="AudioCaptureRedirectionMode"/>.</returns>
        public string Translate(string value)
        {
            var translation = Resources.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }

        /// <summary>
        /// Method to translate <see cref="AudioCaptureRedirectionMode"/>.
        /// </summary>
        /// <param name="audioCaptureRedirectionMode"><see cref="AudioCaptureRedirectionMode"/>.</param>
        /// <returns>Translated <see cref="AudioCaptureRedirectionMode"/>.</returns>
        public string Translate(AudioCaptureRedirectionMode audioCaptureRedirectionMode)
        {
            return Translate(audioCaptureRedirectionMode.ToString());
        }
    }
}