using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    public class RemoteDesktopAudioCaptureRedirectionModeTranslator : SingletonBase<RemoteDesktopAudioCaptureRedirectionModeTranslator>, ITranslator
    {
        private const string _identifier = "RemoteDesktopAudioCaptureRedirectionMode_";

        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }
    }
}