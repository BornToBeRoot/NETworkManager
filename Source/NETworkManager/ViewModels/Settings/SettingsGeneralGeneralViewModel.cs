using NETworkManager.Models.Settings;
using NETworkManager.Views;
using System.Collections.ObjectModel;
using System.Linq;

namespace NETworkManager.ViewModels.Settings
{
    public class SettingsGeneralGeneralViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

        public ObservableCollection<ApplicationViewInfo> ApplicationViewCollection { get; set; }

        private ApplicationViewInfo _defaultApplicationViewSelectedItem;
        public ApplicationViewInfo DefaultApplicationViewSelectedItem
        {
            get { return _defaultApplicationViewSelectedItem; }
            set
            {
                if (value == _defaultApplicationViewSelectedItem)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Application_DefaultApplicationViewName = value.Name;

                _defaultApplicationViewSelectedItem = value;
                OnPropertyChanged();
            }
        }

        private int _historyListEntries;
        public int HistoryListEntries
        {
            get { return _historyListEntries; }
            set
            {
                if (value == _historyListEntries)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.Application_HistoryListEntries = value;

                _historyListEntries = value;
                OnPropertyChanged();
            }
        }
        #endregion        

        #region Constructor, LoadSettings
        public SettingsGeneralGeneralViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            ApplicationViewCollection = new ObservableCollection<ApplicationViewInfo>(ApplicationViewManager.List.OrderBy(x => x.Name));

            DefaultApplicationViewSelectedItem = ApplicationViewCollection.FirstOrDefault(x => x.Name == SettingsManager.Current.Application_DefaultApplicationViewName);
            HistoryListEntries = SettingsManager.Current.Application_HistoryListEntries;
        }
        #endregion
    }
}