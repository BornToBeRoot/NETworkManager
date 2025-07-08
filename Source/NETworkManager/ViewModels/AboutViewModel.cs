﻿using NETworkManager.Documentation;
using NETworkManager.Localization.Resources;
using NETworkManager.Properties;
using NETworkManager.Settings;
using NETworkManager.Update;
using NETworkManager.Utilities;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class AboutViewModel : ViewModelBase
{
    #region Constructor

    public AboutViewModel()
    {
        LibrariesView = CollectionViewSource.GetDefaultView(LibraryManager.List);
        LibrariesView.SortDescriptions.Add(new SortDescription(nameof(LibraryInfo.Name), ListSortDirection.Ascending));

        ExternalServicesView = CollectionViewSource.GetDefaultView(ExternalServicesManager.List);
        ExternalServicesView.SortDescriptions.Add(new SortDescription(nameof(ExternalServicesInfo.Name),
            ListSortDirection.Ascending));

        ResourcesView = CollectionViewSource.GetDefaultView(ResourceManager.List);
        ResourcesView.SortDescriptions.Add(new SortDescription(nameof(ResourceInfo.Name), ListSortDirection.Ascending));
    }

    #endregion

    #region Methods

    private async Task CheckForUpdatesAsync()
    {
        IsUpdateAvailable = false;
        ShowUpdaterMessage = false;

        IsUpdateCheckRunning = true;

        // Show a loading animation for the user
        await Task.Delay(GlobalStaticConfiguration.ApplicationUIRefreshInterval);

        var updater = new Updater();

        updater.UpdateAvailable += Updater_UpdateAvailable;
        updater.NoUpdateAvailable += Updater_NoUpdateAvailable;
        updater.Error += Updater_Error;

        updater.CheckOnGitHub(Resources.NETworkManager_GitHub_User, Resources.NETworkManager_GitHub_Repo,
            AssemblyManager.Current.Version, SettingsManager.Current.Update_CheckForPreReleases);
    }

    #endregion

    #region Variables

    public string Version => $"{Strings.Version} {AssemblyManager.Current.Version}";

    public string DevelopedByText =>
        string.Format(Strings.DevelopedAndMaintainedByX + " ", Resources.NETworkManager_GitHub_User);

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

    private bool _isUpdateAvailable;

    public bool IsUpdateAvailable
    {
        get => _isUpdateAvailable;
        set
        {
            if (value == _isUpdateAvailable)
                return;

            _isUpdateAvailable = value;
            OnPropertyChanged();
        }
    }

    private string _updateText;

    public string UpdateText
    {
        get => _updateText;
        private set
        {
            if (value == _updateText)
                return;

            _updateText = value;
            OnPropertyChanged();
        }
    }

    private string _updateReleaseUrl;

    public string UpdateReleaseUrl
    {
        get => _updateReleaseUrl;
        private set
        {
            if (value == _updateReleaseUrl)
                return;

            _updateReleaseUrl = value;
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
        private set
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

    #region Commands & Actions

    public ICommand CheckForUpdatesCommand => new RelayCommand(_ => CheckForUpdatesAction());

    private void CheckForUpdatesAction()
    {
        CheckForUpdatesAsync();
    }

    public ICommand OpenWebsiteCommand => new RelayCommand(OpenWebsiteAction);

    private static void OpenWebsiteAction(object url)
    {
        ExternalProcessStarter.OpenUrl((string)url);
    }

    public ICommand OpenDocumentationCommand
    {
        get { return new RelayCommand(_ => OpenDocumentationAction()); }
    }

    private void OpenDocumentationAction()
    {
        DocumentationManager.OpenDocumentation(DocumentationIdentifier.Default);
    }

    public ICommand OpenLicenseFolderCommand => new RelayCommand(_ => OpenLicenseFolderAction());

    private void OpenLicenseFolderAction()
    {
        Process.Start("explorer.exe", LibraryManager.GetLicenseLocation());
    }

    #endregion

    #region Events

    private void Updater_UpdateAvailable(object sender, UpdateAvailableArgs e)
    {
        UpdateText = string.Format(Strings.VersionxxIsAvailable, e.Release.TagName);
        UpdateReleaseUrl = e.Release.Prerelease ? e.Release.HtmlUrl : Resources.NETworkManager_LatestReleaseUrl;

        IsUpdateCheckRunning = false;
        IsUpdateAvailable = true;
    }

    private void Updater_NoUpdateAvailable(object sender, EventArgs e)
    {
        UpdaterMessage = Strings.NoUpdateAvailable;

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
