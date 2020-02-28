using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    public class RemoteDesktopNetworkConnectionTypeTranslator : SingletonBase<RemoteDesktopNetworkConnectionTypeTranslator>, ITranslator
    {
        private const string _identifier = "RemoteDesktopNetworkConnectionType_";

        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }
    }
}
