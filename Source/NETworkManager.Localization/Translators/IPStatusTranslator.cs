using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    public class IPStatusTranslator : SingletonBase<IPStatusTranslator>, ITranslator
    {
        private const string _identifier = "IPStatus_";

        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }
    }
}
