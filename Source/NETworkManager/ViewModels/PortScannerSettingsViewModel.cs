using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System;
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

        // Own view (not the default view of the collection, which the port profiles dialog shows too)
        PortProfiles = new CollectionViewSource { Source = SettingsManager.Current.PortScanner_PortProfiles }.View;
        PortProfiles.SortDescriptions.Add(
            new SortDescription(nameof(PortProfileInfo.Name), ListSortDirection.Ascending));

        SelectedPortProfile = PortProfiles.Cast<PortProfileInfo>().FirstOrDefault();

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        ShowAllResults = SettingsManager.Current.PortScanner_ShowAllResults;
        Timeout = SettingsManager.Current.PortScanner_Timeout;
        Attempts = SettingsManager.Current.PortScanner_Attempts;
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

    public ICommand EditPortProfileCommand => new RelayCommand(_ => EditPortProfileAction(), PortProfileSelected_CanExecute);

    /// <summary>
    /// Checks if a port profile is selected (edit and delete need one, e.g. F2 / Del without a selection).
    /// </summary>
    private bool PortProfileSelected_CanExecute(object parameter)
    {
        return SelectedPortProfile != null;
    }

    private void EditPortProfileAction()
    {
        _ = EditPortProfile();
    }

    public ICommand DeletePortProfileCommand =>
        new RelayCommand(_ => DeletePortProfileAction(), PortProfileSelected_CanExecute);

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

        var childWindowViewModel = new PortProfileViewModel(async instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            SettingsManager.Current.PortScanner_PortProfiles.Remove(SelectedPortProfile);
            SettingsManager.Current.PortScanner_PortProfiles.Add(new PortProfileInfo(instance.Name,
                instance.Ports));
        }, async _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, true,
            SelectedPortProfile);

        childWindow.Title = Strings.EditPortProfile;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    private async Task DeletePortProfile()
    {
        var result = await DialogHelper.ShowConfirmationMessageAsync(Application.Current.MainWindow,
            Strings.DeletePortProfile,
            Strings.DeletePortProfileMessage + Environment.NewLine + Environment.NewLine +
            DialogHelper.FormatItemList([SelectedPortProfile.Name]),
            ChildWindowIcon.Info,
            Strings.Delete);

        if (!result)
            return;

        SettingsManager.Current.PortScanner_PortProfiles.Remove(SelectedPortProfile);

        // Select first item after deletion
        SelectedPortProfile = PortProfiles.Cast<PortProfileInfo>().FirstOrDefault();
    }

    #endregion
}