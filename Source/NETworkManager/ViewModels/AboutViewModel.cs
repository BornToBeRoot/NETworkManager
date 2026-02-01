using NETworkManager.Documentation;
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

/// <summary>
/// View model for the About view.
/// </summary>
public class AboutViewModel : ViewModelBase
{
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the <see cref="AboutViewModel"/> class.
    /// </summary>
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

    /// <summary>
    /// Checks for updates asynchronously.
    /// </summary>
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

    /// <summary>
    /// Gets the application version string.
    /// </summary>
    public string Version => $"{Strings.Version} {AssemblyManager.Current.Version}";

    /// <summary>
    /// Gets the text indicating who developed and maintains the application.
    /// </summary>
    public string DevelopedByText =>
        string.Format(Strings.DevelopedAndMaintainedByX + " ", Resources.NETworkManager_GitHub_User);

    /// <summary>
    /// Backing field for <see cref="IsUpdateCheckRunning"/>.
    /// </summary>
    private bool _isUpdateCheckRunning;

    /// <summary>
    /// Gets or sets a value indicating whether the update check is currently running.
    /// </summary>
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

    /// <summary>
    /// Backing field for <see cref="IsUpdateAvailable"/>.
    /// </summary>
    private bool _isUpdateAvailable;

    /// <summary>
    /// Gets or sets a value indicating whether an update is available.
    /// </summary>
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

    /// <summary>
    /// Backing field for <see cref="UpdateText"/>.
    /// </summary>
    private string _updateText;

    /// <summary>
    /// Gets the text describing the available update.
    /// </summary>
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

    /// <summary>
    /// Backing field for <see cref="UpdateReleaseUrl"/>.
    /// </summary>
    private string _updateReleaseUrl;

    /// <summary>
    /// Gets the URL for the release notes or download page.
    /// </summary>
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

    /// <summary>
    /// Backing field for <see cref="ShowUpdaterMessage"/>.
    /// </summary>
    private bool _showUpdaterMessage;

    /// <summary>
    /// Gets or sets a value indicating whether to show a message from the updater.
    /// </summary>
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

    /// <summary>
    /// Backing field for <see cref="UpdaterMessage"/>.
    /// </summary>
    private string _updaterMessage;

    /// <summary>
    /// Gets the message from the updater (e.g., no update available, error).
    /// </summary>
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

    /// <summary>
    /// Gets the collection view for the list of used libraries.
    /// </summary>
    public ICollectionView LibrariesView { get; }

    /// <summary>
    /// Backing field for <see cref="SelectedLibraryInfo"/>.
    /// </summary>
    private LibraryInfo _selectedLibraryInfo;

    /// <summary>
    /// Gets or sets the currently selected library information.
    /// </summary>
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

    /// <summary>
    /// Gets the collection view for the list of used external services.
    /// </summary>
    public ICollectionView ExternalServicesView { get; }

    /// <summary>
    /// Backing field for <see cref="SelectedExternalServicesInfo"/>.
    /// </summary>
    private ExternalServicesInfo _selectedExternalServicesInfo;

    /// <summary>
    /// Gets or sets the currently selected external service information.
    /// </summary>
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

    /// <summary>
    /// Gets the collection view for the list of used resources.
    /// </summary>
    public ICollectionView ResourcesView { get; }

    /// <summary>
    /// Backing field for <see cref="SelectedResourceInfo"/>.
    /// </summary>
    private ResourceInfo _selectedResourceInfo;

    /// <summary>
    /// Gets or sets the currently selected resource information.
    /// </summary>
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

    /// <summary>
    /// Gets the command to check for updates.
    /// </summary>
    public ICommand CheckForUpdatesCommand => new RelayCommand(_ => CheckForUpdatesAction());

    /// <summary>
    /// Action to check for updates.
    /// </summary>
    private void CheckForUpdatesAction()
    {
        CheckForUpdatesAsync();
    }

    /// <summary>
    /// Gets the command to open the website.
    /// </summary>
    public ICommand OpenWebsiteCommand => new RelayCommand(OpenWebsiteAction);

    /// <summary>
    /// Action to open the website.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    private static void OpenWebsiteAction(object url)
    {
        ExternalProcessStarter.OpenUrl((string)url);
    }

    /// <summary>
    /// Gets the command to open the documentation.
    /// </summary>
    public ICommand OpenDocumentationCommand
    {
        get { return new RelayCommand(_ => OpenDocumentationAction()); }
    }

    /// <summary>
    /// Action to open the documentation.
    /// </summary>
    private void OpenDocumentationAction()
    {
        DocumentationManager.OpenDocumentation(DocumentationIdentifier.Default);
    }

    /// <summary>
    /// Gets the command to open the license folder.
    /// </summary>
    public ICommand OpenLicenseFolderCommand => new RelayCommand(_ => OpenLicenseFolderAction());

    /// <summary>
    /// Action to open the license folder.
    /// </summary>
    private void OpenLicenseFolderAction()
    {
        Process.Start("explorer.exe", LibraryManager.GetLicenseLocation());
    }

    #endregion

    #region Events

    /// <summary>
    /// Handles the UpdateAvailable event from the Updater.
    /// </summary>
    private void Updater_UpdateAvailable(object sender, UpdateAvailableArgs e)
    {
        UpdateText = string.Format(Strings.VersionxxIsAvailable, e.Release.TagName);
        UpdateReleaseUrl = e.Release.Prerelease ? e.Release.HtmlUrl : Resources.NETworkManager_LatestReleaseUrl;

        IsUpdateCheckRunning = false;
        IsUpdateAvailable = true;
    }

    /// <summary>
    /// Handles the NoUpdateAvailable event from the Updater.
    /// </summary>
    private void Updater_NoUpdateAvailable(object sender, EventArgs e)
    {
        UpdaterMessage = Strings.NoUpdateAvailable;

        IsUpdateCheckRunning = false;
        ShowUpdaterMessage = true;
    }

    /// <summary>
    /// Handles the Error event from the Updater.
    /// </summary>
    private void Updater_Error(object sender, EventArgs e)
    {
        UpdaterMessage = Strings.ErrorCheckingApiGithubComVerifyYourNetworkConnection;

        IsUpdateCheckRunning = false;
        ShowUpdaterMessage = true;
    }

    #endregion
}
