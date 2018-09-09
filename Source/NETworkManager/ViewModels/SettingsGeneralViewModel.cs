using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System.Collections.ObjectModel;
using System.Linq;

namespace NETworkManager.ViewModels
{
    public class SettingsGeneralViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        public ObservableCollection<ApplicationViewInfo> ApplicationViewCollection { get; set; }

        private ApplicationViewInfo _defaultApplicationViewSelectedItem;
        public ApplicationViewInfo DefaultApplicationViewSelectedItem
        {
            get => _defaultApplicationViewSelectedItem;
            set
            {
                if (value == _defaultApplicationViewSelectedItem)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.General_DefaultApplicationViewName = value.Name;

                _defaultApplicationViewSelectedItem = value;
                OnPropertyChanged();
            }
        }

        private int _historyListEntries;
        public int HistoryListEntries
        {
            get => _historyListEntries;
            set
            {
                if (value == _historyListEntries)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.General_HistoryListEntries = value;

                _historyListEntries = value;
                OnPropertyChanged();
            }
        }
        #endregion        

        #region Constructor, LoadSettings
        public SettingsGeneralViewModel()
        {
            _isLoading = true;

            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            ApplicationViewCollection = new ObservableCollection<ApplicationViewInfo>(SettingsManager.Current.General_ApplicationList);

            DefaultApplicationViewSelectedItem = ApplicationViewCollection.FirstOrDefault(x => x.Name == SettingsManager.Current.General_DefaultApplicationViewName);
            HistoryListEntries = SettingsManager.Current.General_HistoryListEntries;
        }
        #endregion
    }
}