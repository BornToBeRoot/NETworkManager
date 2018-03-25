using System.Linq;
using System.Collections.ObjectModel;
using NETworkManager.Models.Settings;
using NETworkManager.Utils;

namespace NETworkManager.ViewModels.Settings
{
    public class SettingsGeneralLanguageViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

        public ObservableCollection<LocalizationInfo> LanguageCollection { get; set; }

        private LocalizationInfo _localizationSelectedItem;
        public LocalizationInfo LocalizationSelectedItem
        {
            get { return _localizationSelectedItem; }
            set
            {
                if (value == _localizationSelectedItem)
                    return;

                if (!_isLoading)
                {
                    LocalizationManager.Change(value);

                    SettingsManager.Current.Localization_CultureCode = value.Code;
                }

                _localizationSelectedItem = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Construtor, LoadSettings
        public SettingsGeneralLanguageViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            LanguageCollection = new ObservableCollection<LocalizationInfo>(LocalizationManager.List);
            LocalizationSelectedItem = LanguageCollection.FirstOrDefault(x => x.Code == LocalizationManager.Current.Code);
        }

        #endregion
    }
}