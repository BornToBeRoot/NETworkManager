using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    public class DiscoveryProtocolTranslator : SingletonBase<DiscoveryProtocolTranslator>, ITranslator
    {
        private const string _identifier = "DiscoveryProtocolIdentifier_";

        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }
    }
}
