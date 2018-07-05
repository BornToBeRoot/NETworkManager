using NETworkManager.Models.Settings;
using System.Windows.Input;
using System.Diagnostics;
using NETworkManager.Models.Update;
using System;
using System.ComponentModel;
using NETworkManager.Models.Documentation;
using System.Windows.Data;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        #region Variables
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                if (value == _title)
                    return;

                _title = value;
                OnPropertyChanged();
            }
        }

        public string Version
        {
            get { return AssemblyManager.Current.Version.ToString(); }
        }

        public string BuildDate
        {
            get { return AssemblyManager.Current.BuildDate.ToString(); }
        }

        private bool _isUpdateCheckRunning;
        public bool IsUpdateCheckRunning
        {
            get { return _isUpdateCheckRunning; }
            set
            {
                if (value == _isUpdateCheckRunning)
                    return;

                _isUpdateCheckRunning = value;
                OnPropertyChanged();

            }
        }

        private bool _updateAvailable;
        public bool UpdateAvailable
        {
            get { return _updateAvailable; }
            set
            {
                if (value == _updateAvailable)
                    return;

                _updateAvailable = value;
                OnPropertyChanged();
            }
        }

        private string _updateText;
        public string UpdateText
        {
            get { return _updateText; }
            set
            {
                if (value == _updateText)
                    return;

                _updateText = value;
                OnPropertyChanged();
            }
        }

        private bool _showUpdaterMessage;
        public bool ShowUpdaterMessage
        {
            get { return _showUpdaterMessage; }
            set
            {
                if (value == _showUpdaterMessage)
                    return;

                _showUpdaterMessage = value;
                OnPropertyChanged();
            }
        }

        private string _updaterMessage;
        public string UpdaterMessage
        {
            get { return _updaterMessage; }
            set
            {
                if (value == _updaterMessage)
                    return;

                _updaterMessage = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _librariesView;
        public ICollectionView LibrariesView
        {
            get { return _librariesView; }
        }

        private LibraryInfo _selectedLibraryInfo;
        public LibraryInfo SelectedLibraryInfo
        {
            get { return _selectedLibraryInfo; }
            set
            {
                if (value == _selectedLibraryInfo)
                    return;

                _selectedLibraryInfo = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _resourcesView;
        public ICollectionView ResourcesView
        {
            get { return _resourcesView; }
        }

        private ResourceInfo _selectedResourceInfo;
        public ResourceInfo SelectedResourceInfo
        {
            get { return _selectedResourceInfo; }
            set
            {
                if (value == _selectedResourceInfo)
                    return;

                _selectedResourceInfo = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor
        public AboutViewModel()
        {
            _librariesView = CollectionViewSource.GetDefaultView(LibraryManager.List);
            _librariesView.SortDescriptions.Add(new SortDescription(nameof(LibraryInfo.Library), ListSortDirection.Ascending));

            _resourcesView = CollectionViewSource.GetDefaultView(ResourceManager.List);
            _resourcesView.SortDescriptions.Add(new SortDescription(nameof(ResourceInfo.Resource), ListSortDirection.Ascending));
        }
        #endregion

        #region Commands & Actions
        public ICommand CheckForUpdatesCommand
        {
            get { return new RelayCommand(p => CheckForUpdatesAction()); }
        }

        private void CheckForUpdatesAction()
        {
            CheckForUpdates();
        }

        public ICommand OpenWebsiteCommand
        {
            get { return new RelayCommand(p => OpenWebsiteAction(p)); }
        }

        private void OpenWebsiteAction(object url)
        {
            Process.Start((string)url);
        }

        public ICommand OpenLicenseFolderCommand
        {
            get { return new RelayCommand(p => OpenLicenseFolderAction()); }
        }

        private void OpenLicenseFolderAction()
        {
            OpenLicenseFolder();
        }
        #endregion

        #region Methods
        private void CheckForUpdates()
        {
            UpdateAvailable = false;
            ShowUpdaterMessage = false;

            IsUpdateCheckRunning = true;

            Updater updater = new Updater();

            updater.UpdateAvailable += Updater_UpdateAvailable;
            updater.NoUpdateAvailable += Updater_NoUpdateAvailable;
            updater.Error += Updater_Error;

            updater.Check();
        }

        private void OpenLicenseFolder()
        {
            Process.Start(LibraryManager.GetLicenseLocation());
        }
        #endregion

        #region Events
        private void Updater_UpdateAvailable(object sender, UpdateAvailableArgs e)
        {
            UpdateText = string.Format(LocalizationManager.GetStringByKey("String_VersionxxAvailable"), e.Version);

            IsUpdateCheckRunning = false;
            UpdateAvailable = true;
        }

        private void Updater_NoUpdateAvailable(object sender, System.EventArgs e)
        {
            UpdaterMessage = LocalizationManager.GetStringByKey("String_NoUpdateAvailable");

            IsUpdateCheckRunning = false;
            ShowUpdaterMessage = true;
        }

        private void Updater_Error(object sender, EventArgs e)
        {
            UpdaterMessage = LocalizationManager.GetStringByKey("String_ErrorCheckingApiGithubComVerifyYourNetworkConnection"); ;

            IsUpdateCheckRunning = false;
            ShowUpdaterMessage = true;
        }
        #endregion
    }
}