using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
using System.Collections;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
/// ViewModel for the Port Scanner settings.
/// </summary>
public class PortScannerSettingsViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    /// <summary>
    /// Gets the collection view of port profiles.
    /// </summary>
    public ICollectionView PortProfiles { get; }

    /// <summary>
    /// Gets or sets the selected port profile.
    /// </summary>
    public PortProfileInfo SelectedPortProfile
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
    /// Gets or sets the selected port profiles (several can be deleted at once).
    /// </summary>
    public IList SelectedPortProfiles
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new ArrayList();

    /// <summary>
    /// Gets or sets a value indicating whether to show all results (open and closed ports).
    /// </summary>
    public bool ShowAllResults
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PortScanner_ShowAllResults = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the timeout for port scanning in milliseconds.
    /// </summary>
    public int Timeout
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PortScanner_Timeout = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the number of attempts to connect to a port if there was no answer.
    /// </summary>
    public int Attempts
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PortScanner_Attempts = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to resolve hostnames.
    /// </summary>
    public bool ResolveHostname
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PortScanner_ResolveHostname = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets whether every host is pinged before its ports are scanned (hosts without answer are skipped).
    /// </summary>
    public bool PingHostsBeforeScan
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PortScanner_PingHostsBeforeScan = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets whether the number of hosts and ports scanned at the same time is adjusted to the scan (instead of
    /// <see cref="MaxHostThreads"/> and <see cref="MaxPortThreads"/>).
    /// </summary>
    public bool AutoAdjustThreads
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PortScanner_AutoAdjustThreads = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the maximum number of threads for scanning hosts.
    /// </summary>
    public int MaxHostThreads
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PortScanner_MaxHostThreads = value;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MaxPortThreadsMaximum));
        }
    }

    /// <summary>
    /// Gets the maximum of <see cref="MaxHostThreads"/>: 256, but host threads x port threads at most
    /// <see cref="GlobalStaticConfiguration.PortScanner_MaxConnections"/> (each connection uses a local port).
    /// </summary>
    public int MaxHostThreadsMaximum =>
        Math.Min(256, GlobalStaticConfiguration.PortScanner_MaxConnections / Math.Max(1, MaxPortThreads));

    /// <summary>
    /// Gets or sets the maximum number of threads for scanning ports.
    /// </summary>
    public int MaxPortThreads
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.PortScanner_MaxPortThreads = value;

            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(MaxHostThreadsMaximum));
        }
    }

    /// <summary>
    /// Gets the maximum of <see cref="MaxPortThreads"/>: 512, but host threads x port threads at most
    /// <see cref="GlobalStaticConfiguration.PortScanner_MaxConnections"/> (each connection uses a local port).
    /// </summary>
    public int MaxPortThreadsMaximum =>
        Math.Min(512, GlobalStaticConfiguration.PortScanner_MaxConnections / Math.Max(1, MaxHostThreads));

    #endregion

    #region Constructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="PortScannerSettingsViewModel"/> class.
    /// </summary>
    public PortScannerSettingsViewModel()
    {
        _isLoading = true;

        // Own view (not the default view of the collection), sort and selection only apply to the settings
        PortProfiles = new CollectionViewSource { Source = SettingsManager.Current.PortScanner_PortProfiles }.View;
        PortProfiles.SortDescriptions.Add(
            new SortDescription(nameof(PortProfileInfo.Name), ListSortDirection.Ascending));

        SelectFirstPortProfile();

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        ShowAllResults = SettingsManager.Current.PortScanner_ShowAllResults;
        Timeout = SettingsManager.Current.PortScanner_Timeout;
        Attempts = SettingsManager.Current.PortScanner_Attempts;
        PingHostsBeforeScan = SettingsManager.Current.PortScanner_PingHostsBeforeScan;
        ResolveHostname = SettingsManager.Current.PortScanner_ResolveHostname;
        AutoAdjustThreads = SettingsManager.Current.PortScanner_AutoAdjustThreads;
        MaxHostThreads = SettingsManager.Current.PortScanner_MaxHostThreads;
        MaxPortThreads = SettingsManager.Current.PortScanner_MaxPortThreads;
    }

    #endregion

    #region ICommand & Actions

    public ICommand AddPortProfileCommand => new RelayCommand(_ => AddPortProfileAction());

    private void AddPortProfileAction()
    {
        _ = AddPortProfile();
    }

    public ICommand EditPortProfileCommand =>
        new RelayCommand(_ => EditPortProfileAction(), EditPortProfile_CanExecute);

    /// <summary>
    /// Checks if a port profile is selected (edit and delete need one, e.g. F2 / Del without a selection).
    /// </summary>
    /// <summary>
    /// Checks if exactly one port profile is selected (e.g. F2 without a selection or with several selected).
    /// </summary>
    private bool EditPortProfile_CanExecute(object parameter)
    {
        return SelectedPortProfile != null && SelectedPortProfiles.Count == 1;
    }

    /// <summary>
    /// Checks if at least one port profile is selected (e.g. Del without a selection).
    /// </summary>
    private bool DeletePortProfile_CanExecute(object parameter)
    {
        return SelectedPortProfiles.Count > 0;
    }

    private void EditPortProfileAction()
    {
        _ = EditPortProfile();
    }

    public ICommand DeletePortProfileCommand =>
        new RelayCommand(_ => DeletePortProfileAction(), DeletePortProfile_CanExecute);

    private void DeletePortProfileAction()
    {
        _ = DeletePortProfile();
    }

    #endregion

    #region Methods

    private Task AddPortProfile()
    {
        var childWindow = new PortProfileChildWindow();

        var childWindowViewModel = new PortProfileViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            SettingsManager.Current.PortScanner_PortProfiles.Add(new PortProfileInfo(instance.Name, instance.Ports));
        }, async _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        });

        childWindow.Title = Strings.AddPortProfile;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    public Task EditPortProfile()
    {
        var childWindow = new PortProfileChildWindow();

        var portProfile = SelectedPortProfile;

        var childWindowViewModel = new PortProfileViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            // Same ID, profiles that reference the port profile still find it (also with a new name)
            var editedPortProfile = new PortProfileInfo(instance.Name, instance.Ports);

            if (portProfile.Id != Guid.Empty)
                editedPortProfile.Id = portProfile.Id;

            SettingsManager.Current.PortScanner_PortProfiles.Remove(portProfile);
            SettingsManager.Current.PortScanner_PortProfiles.Add(editedPortProfile);
        }, async _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, true,
            portProfile);

        childWindow.Title = Strings.EditPortProfile;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    /// <summary>
    /// Deletes the selected port profiles (after a confirmation that lists them).
    /// </summary>
    private async Task DeletePortProfile()
    {
        var portProfiles = SelectedPortProfiles.Cast<PortProfileInfo>().ToList();

        var result = await DialogHelper.ShowConfirmationMessageAsync(Application.Current.MainWindow,
            portProfiles.Count == 1 ? Strings.DeletePortProfile : Strings.DeletePortProfiles,
            (portProfiles.Count == 1 ? Strings.DeletePortProfileMessage : Strings.DeletePortProfilesMessage) +
            Environment.NewLine + Environment.NewLine +
            DialogHelper.FormatItemList([.. portProfiles.Select(x => x.Name)]),
            ChildWindowIcon.Info,
            Strings.Delete);

        if (!result)
            return;

        foreach (var portProfile in portProfiles)
            SettingsManager.Current.PortScanner_PortProfiles.Remove(portProfile);

        SelectFirstPortProfile();
    }

    /// <summary>
    /// Selects the first port profile (also in the list of selected port profiles, which the grid applies).
    /// </summary>
    private void SelectFirstPortProfile()
    {
        SelectedPortProfile = PortProfiles.Cast<PortProfileInfo>().FirstOrDefault();
        SelectedPortProfiles = SelectedPortProfile != null ? new ArrayList { SelectedPortProfile } : new ArrayList();
    }

    #endregion
}