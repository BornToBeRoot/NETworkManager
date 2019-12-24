using NETworkManager.Models.Settings;
using System.Windows.Input;
using System.Diagnostics;
using NETworkManager.Models.Update;
using System;
using System.ComponentModel;
using NETworkManager.Models.Documentation;
using System.Windows.Data;
using NETworkManager.Resources.Localization;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels
{
    public class AboutViewModel : ViewModelBase
    {
        #region Variables
        public string Version => $"{Strings.Version} {AssemblyManager.Current.Version.Major}.{AssemblyManager.Current.Version.Minor}";
        public string Patch => $"{Strings.Patch} {AssemblyManager.Current.Version.Build}";
        public bool IsPatchVisible => AssemblyManager.Current.Version.Build > 0;

        private bool _isUpdateCheckRunning;
        public bool IsUpdateCheckRunning
        {
            get => _isUpdateCheckRunning;
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
            get => _updateAvailable;
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
            get => _updateText;
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
            get => _showUpdaterMessage;
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
            get => _updaterMessage;
            set
            {
                if (value == _updaterMessage)
                    return;

                _updaterMessage = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView LibrariesView { get; }

        private LibraryInfo _selectedLibraryInfo;
        public LibraryInfo SelectedLibraryInfo
        {
            get => _selectedLibraryInfo;
            set
            {
                if (value == _selectedLibraryInfo)
                    return;

                _selectedLibraryInfo = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView ExternalServicesView { get; }

        private ExternalServicesInfo _selectedExternalServicesInfo;
        public ExternalServicesInfo SelectedExternalServicesInfo
        {
            get => _selectedExternalServicesInfo;
            set
            {
                if (value == _selectedExternalServicesInfo)
                    return;

                _selectedExternalServicesInfo = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView ResourcesView { get; }

        private ResourceInfo _selectedResourceInfo;
        public ResourceInfo SelectedResourceInfo
        {
            get => _selectedResourceInfo;
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
            LibrariesView = CollectionViewSource.GetDefaultView(LibraryManager.List);
            LibrariesView.SortDescriptions.Add(new SortDescription(nameof(LibraryInfo.Name), ListSortDirection.Ascending));

            ExternalServicesView = CollectionViewSource.GetDefaultView(ExternalServicesManager.List);
            ExternalServicesView.SortDescriptions.Add(new SortDescription(nameof(ExternalServicesInfo.Name), ListSortDirection.Ascending));

            ResourcesView = CollectionViewSource.GetDefaultView(ResourceManager.List);
            ResourcesView.SortDescriptions.Add(new SortDescription(nameof(ResourceInfo.Name), ListSortDirection.Ascending));
        }
        #endregion

        #region Commands & Actions
        public ICommand CheckForUpdatesCommand => new RelayCommand(p => CheckForUpdatesAction());

        private void CheckForUpdatesAction()
        {
            CheckForUpdates();
        }

        public ICommand OpenWebsiteCommand => new RelayCommand(OpenWebsiteAction);

        private static void OpenWebsiteAction(object url)
        {
            Process.Start((string)url);
        }

        public ICommand OpenDocumentationCommand
        {
            get { return new RelayCommand(p => OpenDocumentationAction()); }
        }

        private void OpenDocumentationAction()
        {
            DocumentationManager.OpenDocumentation(DocumentationIdentifier.Default);
        }

        public ICommand OpenLicenseFolderCommand => new RelayCommand(p => OpenLicenseFolderAction());

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

            var updater = new Updater();

            updater.UpdateAvailable += Updater_UpdateAvailable;
            updater.NoUpdateAvailable += Updater_NoUpdateAvailable;
            updater.ClientIncompatibleWithNewVersion += Updater_ClientIncompatibleWithNewVersion; ;
            updater.Error += Updater_Error;

            updater.Check();
        }

        public void OpenLicenseFolder() => Process.Start(LibraryManager.GetLicenseLocation());
        #endregion

        #region Events
        private void Updater_UpdateAvailable(object sender, UpdateAvailableArgs e)
        {
            UpdateText = string.Format(Strings.VersionxxIsAvailable, e.Version);

            IsUpdateCheckRunning = false;
            UpdateAvailable = true;
        }

        private void Updater_NoUpdateAvailable(object sender, EventArgs e)
        {
            UpdaterMessage = Strings.NoUpdateAvailable;

            IsUpdateCheckRunning = false;
            ShowUpdaterMessage = true;
        }

        private void Updater_ClientIncompatibleWithNewVersion(object sender, EventArgs e)
        {
            UpdaterMessage = Strings.YourSystemOSIsIncompatibleWithTheLatestRelease;

            IsUpdateCheckRunning = false;
            ShowUpdaterMessage = true;
        }

        private void Updater_Error(object sender, EventArgs e)
        {
            UpdaterMessage = Strings.ErrorCheckingApiGithubComVerifyYourNetworkConnection;

            IsUpdateCheckRunning = false;
            ShowUpdaterMessage = true;
        }
        #endregion
    }
}