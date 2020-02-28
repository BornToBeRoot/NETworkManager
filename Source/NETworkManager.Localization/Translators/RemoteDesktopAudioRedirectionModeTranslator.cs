using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    public class RemoteDesktopAudioRedirectionModeTranslator : SingletonBase<RemoteDesktopAudioRedirectionModeTranslator>, ITranslator
    {
        private const string _identifier = "RemoteDesktopAudioRedirectionMode_";

        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }
    }
}
