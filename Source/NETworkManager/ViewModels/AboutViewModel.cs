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
    /// Gets or sets a value indicating whether the update check is currently running.
    /// </summary>
    public bool IsUpdateCheckRunning
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether an update is available.
    /// </summary>
    public bool IsUpdateAvailable
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the text describing the available update.
    /// </summary>
    public string UpdateText
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the URL for the release notes or download page.
    /// </summary>
    public string UpdateReleaseUrl
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to show a message from the updater.
    /// </summary>
    public bool ShowUpdaterMessage
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the message from the updater (e.g., no update available, error).
    /// </summary>
    public string UpdaterMessage
    {
        get;
        private set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the collection view for the list of used libraries.
    /// </summary>
    public ICollectionView LibrariesView { get; }

    /// <summary>
    /// Gets or sets the currently selected library information.
    /// </summary>
    public LibraryInfo SelectedLibraryInfo
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the collection view for the list of used external services.
    /// </summary>
    public ICollectionView ExternalServicesView { get; }

    /// <summary>
    /// Gets or sets the currently selected external service information.
    /// </summary>
    public ExternalServicesInfo SelectedExternalServicesInfo
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the collection view for the list of used resources.
    /// </summary>
    public ICollectionView ResourcesView { get; }

    /// <summary>
    /// Gets or sets the currently selected resource information.
    /// </summary>
    public ResourceInfo SelectedResourceInfo
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
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
