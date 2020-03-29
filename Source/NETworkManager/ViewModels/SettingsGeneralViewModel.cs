using NETworkManager.Models;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class SettingsGeneralViewModel : ViewModelBase
    {
        #region Variables
        private readonly bool _isLoading;

        //public ObservableCollection<ApplicationViewInfo> ApplicationViewCollection { get; set; }
        public ICollectionView Applications { get; private set; }

        private ApplicationInfo _defaultApplicationSelectedItem;
        public ApplicationInfo DefaultApplicationSelectedItem
        {
            get => _defaultApplicationSelectedItem;
            set
            {
                if (value == _defaultApplicationSelectedItem)
                    return;

                if (value != null && !_isLoading)
                    SettingsManager.Current.General_DefaultApplicationViewName = value.Name;

                _defaultApplicationSelectedItem = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView ApplicationsVisible { get; private set; }

        private ApplicationInfo _visibleApplicationSelectedItem;
        public ApplicationInfo VisibleApplicationSelectedItem
        {
            get => _visibleApplicationSelectedItem;
            set
            {
                if (value == _visibleApplicationSelectedItem)
                    return;

                _visibleApplicationSelectedItem = value;

                ValidateHideVisibleApplications();

                OnPropertyChanged();
            }
        }

        private bool _isVisibleToHideApplicationEnabled;
        public bool IsVisibleToHideApplicationEnabled
        {
            get => _isVisibleToHideApplicationEnabled;
            set
            {
                if (value == _isVisibleToHideApplicationEnabled)
                    return;

                _isVisibleToHideApplicationEnabled = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView ApplicationsHidden { get; private set; }

        private ApplicationInfo _hiddenApplicationSelectedItem;
        public ApplicationInfo HiddenApplicationSelectedItem
        {
            get => _hiddenApplicationSelectedItem;
            set
            {
                if (value == _hiddenApplicationSelectedItem)
                    return;

                _hiddenApplicationSelectedItem = value;

                ValidateHideVisibleApplications();

                OnPropertyChanged();
            }
        }

        private bool _isHideToVisibleApplicationEnabled;
        public bool IsHideToVisibleApplicationEnabled
        {
            get => _isHideToVisibleApplicationEnabled;
            set
            {
                if (value == _isHideToVisibleApplicationEnabled)
                    return;

                _isHideToVisibleApplicationEnabled = value;
                OnPropertyChanged();
            }
        }

        private int _backgroundJobInterval;
        public int BackgroundJobInterval
        {
            get => _backgroundJobInterval;
            set
            {
                if (value == _backgroundJobInterval)
                    return;

                if (!_isLoading)
                    SettingsManager.Current.General_BackgroundJobInterval = value;

                _backgroundJobInterval = value;
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

            SettingsManager.Current.General_ApplicationList.CollectionChanged += General_ApplicationList_CollectionChanged;

            _isLoading = false;
        }

        private void General_ApplicationList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ApplicationsVisible.Refresh();
            ApplicationsHidden.Refresh();
        }

        private void LoadSettings()
        {
            //ApplicationViewCollection = new ObservableCollection<ApplicationViewInfo>(SettingsManager.Current.General_ApplicationList);
            Applications = new CollectionViewSource { Source = SettingsManager.Current.General_ApplicationList }.View;

            ApplicationsVisible = new CollectionViewSource { Source = SettingsManager.Current.General_ApplicationList }.View;
            ApplicationsVisible.Filter = o =>
            {
                if (!(o is ApplicationInfo info))
                    return false;

                return info.IsVisible;
            };

            ApplicationsHidden = new CollectionViewSource { Source = SettingsManager.Current.General_ApplicationList }.View;

            ApplicationsHidden.Filter = o =>
            {
                if (!(o is ApplicationInfo info))
                    return false;

                return !info.IsVisible;
            };

            ValidateHideVisibleApplications();

            DefaultApplicationSelectedItem = Applications.Cast<ApplicationInfo>().FirstOrDefault(x => x.Name == SettingsManager.Current.General_DefaultApplicationViewName);
            BackgroundJobInterval = SettingsManager.Current.General_BackgroundJobInterval;
            HistoryListEntries = SettingsManager.Current.General_HistoryListEntries;
        }
        #endregion

        #region ICommands & Actions
        public ICommand VisibleToHideApplicationCommand
        {
            get { return new RelayCommand(p => VisibleToHideApplicationAction()); }
        }

        private void VisibleToHideApplicationAction()
        {
            var index = 0;

            var newDefaultApplication = DefaultApplicationSelectedItem.Name == VisibleApplicationSelectedItem.Name;

            for (var i = 0; i < SettingsManager.Current.General_ApplicationList.Count; i++)
            {
                if (SettingsManager.Current.General_ApplicationList[i].Name != VisibleApplicationSelectedItem.Name)
                    continue;

                index = i;

                break;
            }

            // Remove and add will fire a collection changed event --> detected in MainWindow
            var info = SettingsManager.Current.General_ApplicationList[index];

            info.IsVisible = false;

            SettingsManager.Current.General_ApplicationList.RemoveAt(index);
            SettingsManager.Current.General_ApplicationList.Insert(index, info);

            if (newDefaultApplication)
                DefaultApplicationSelectedItem = ApplicationsVisible.Cast<ApplicationInfo>().FirstOrDefault();

            ValidateHideVisibleApplications();
        }

        public ICommand HideToVisibleApplicationCommand
        {
            get { return new RelayCommand(p => HideToVisibleApplicationAction()); }
        }

        private void HideToVisibleApplicationAction()
        {
            var index = 0;

            for (var i = 0; i < SettingsManager.Current.General_ApplicationList.Count; i++)
            {
                if (SettingsManager.Current.General_ApplicationList[i].Name == HiddenApplicationSelectedItem.Name)
                {
                    index = i;
                    break;
                }
            }

            // Remove and add will fire a collection changed event --> detected in MainWindow
            var info = SettingsManager.Current.General_ApplicationList[index];

            info.IsVisible = true;

            SettingsManager.Current.General_ApplicationList.RemoveAt(index);
            SettingsManager.Current.General_ApplicationList.Insert(index, info);

            ValidateHideVisibleApplications();
        }
        #endregion

        #region Methods

        private void ValidateHideVisibleApplications()
        {
            IsVisibleToHideApplicationEnabled = ApplicationsVisible.Cast<ApplicationInfo>().Count() > 1 && VisibleApplicationSelectedItem != null;
            IsHideToVisibleApplicationEnabled = ApplicationsHidden.Cast<ApplicationInfo>().Any() && HiddenApplicationSelectedItem != null;
        }
        #endregion
    }
}