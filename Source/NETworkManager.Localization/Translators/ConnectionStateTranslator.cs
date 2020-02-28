using NETworkManager.Utilities;

namespace NETworkManager.Localization.Translators
{
    public class ConnectionStateTranslator : SingletonBase<ConnectionStateTranslator>, ITranslator
    {       
        private const string _identifier = "ConnectionState_";

        public string Translate(string value)
        {
            var translation = LanguageFiles.Strings.ResourceManager.GetString(_identifier + value, LocalizationManager.GetInstance().Culture);

            return string.IsNullOrEmpty(translation) ? value : translation;
        }
    }
}
