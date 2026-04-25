using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

/// <summary>
/// View model for the IP scanner settings.
/// </summary>
public class IPScannerSettingsViewModel : ViewModelBase
{
    #region Variables

    /// <summary>
    /// Indicates whether the view model is loading.
    /// </summary>
    private readonly bool _isLoading;

    /// <summary>
    /// Gets or sets a value indicating whether to show all results.
    /// </summary>
    public bool ShowAllResults
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_ShowAllResults = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the number of ICMP attempts.
    /// </summary>
    public int ICMPAttempts
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_ICMPAttempts = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the ICMP timeout in milliseconds.
    /// </summary>
    public int ICMPTimeout
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_ICMPTimeout = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the ICMP buffer size.
    /// </summary>
    public int ICMPBuffer
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_ICMPBuffer = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to resolve the hostname.
    /// </summary>
    public bool ResolveHostname
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_ResolveHostname = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether port scanning is enabled.
    /// </summary>
    public bool PortScanEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_PortScanEnabled = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the ports to scan.
    /// </summary>
    public string PortScanPorts
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_PortScanPorts = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the port scan timeout in milliseconds.
    /// </summary>
    public int PortScanTimeout
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_PortScanTimeout = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether NetBIOS is enabled.
    /// </summary>
    public bool NetBIOSEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_NetBIOSEnabled = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the NetBIOS timeout in milliseconds.
    /// </summary>
    public int NetBIOSTimeout
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_NetBIOSTimeout = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to resolve the MAC address.
    /// </summary>
    public bool ResolveMACAddress
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_ResolveMACAddress = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the collection view of custom commands.
    /// </summary>
    public ICollectionView CustomCommands { get; }

    /// <summary>
    /// Gets or sets the selected custom command.
    /// </summary>
    public CustomCommandInfo SelectedCustomCommand
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    } = new();

    /// <summary>
    /// Gets or sets the maximum number of host threads.
    /// </summary>
    public int MaxHostThreads
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_MaxHostThreads = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the maximum number of port threads.
    /// </summary>
    public int MaxPortThreads
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_MaxPortThreads = value;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="IPScannerSettingsViewModel"/> class.
    /// </summary>
    public IPScannerSettingsViewModel()
    {
        _isLoading = true;

        CustomCommands = CollectionViewSource.GetDefaultView(SettingsManager.Current.IPScanner_CustomCommands);
        CustomCommands.SortDescriptions.Add(new SortDescription(nameof(CustomCommandInfo.Name),
            ListSortDirection.Ascending));

        LoadSettings();

        _isLoading = false;
    }

    /// <summary>
    /// Loads the settings.
    /// </summary>
    private void LoadSettings()
    {
        ShowAllResults = SettingsManager.Current.IPScanner_ShowAllResults;
        ICMPAttempts = SettingsManager.Current.IPScanner_ICMPAttempts;
        ICMPTimeout = SettingsManager.Current.IPScanner_ICMPTimeout;
        ICMPBuffer = SettingsManager.Current.IPScanner_ICMPBuffer;
        ResolveHostname = SettingsManager.Current.IPScanner_ResolveHostname;
        PortScanEnabled = SettingsManager.Current.IPScanner_PortScanEnabled;
        PortScanPorts = SettingsManager.Current.IPScanner_PortScanPorts;
        PortScanTimeout = SettingsManager.Current.IPScanner_PortScanTimeout;
        NetBIOSEnabled = SettingsManager.Current.IPScanner_NetBIOSEnabled;
        NetBIOSTimeout = SettingsManager.Current.IPScanner_NetBIOSTimeout;
        ResolveMACAddress = SettingsManager.Current.IPScanner_ResolveMACAddress;
        MaxHostThreads = SettingsManager.Current.IPScanner_MaxHostThreads;
        MaxPortThreads = SettingsManager.Current.IPScanner_MaxPortThreads;
    }

    #endregion

    #region ICommand & Actions

    /// <summary>
    /// Gets the command to add a custom command.
    /// </summary>
    public ICommand AddCustomCommandCommand => new RelayCommand(_ => AddCustomCommandAction());

    /// <summary>
    /// Action to add a custom command.
    /// </summary>
    private void AddCustomCommandAction()
    {
        AddCustomCommand();
    }

    /// <summary>
    /// Gets the command to edit a custom command.
    /// </summary>
    public ICommand EditCustomCommandCommand => new RelayCommand(_ => EditCustomCommandAction());

    /// <summary>
    /// Action to edit a custom command.
    /// </summary>
    private void EditCustomCommandAction()
    {
        EditCustomCommand();
    }

    /// <summary>
    /// Gets the command to delete a custom command.
    /// </summary>
    public ICommand DeleteCustomCommandCommand => new RelayCommand(_ => DeleteCustomCommandAction());

    /// <summary>
    /// Action to delete a custom command.
    /// </summary>
    private void DeleteCustomCommandAction()
    {
        DeleteCustomCommand().ConfigureAwait(false);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Add a new custom command.
    /// </summary>
    private Task AddCustomCommand()
    {
        var childWindow = new CustomCommandChildWindow();

        var childWindowViewModel = new CustomCommandViewModel(instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            SettingsManager.Current.IPScanner_CustomCommands.Add(new CustomCommandInfo(
                instance.ID,
                instance.Name,
                instance.FilePath,
                instance.Arguments)
            );
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        });

        childWindow.Title = Strings.AddCustomCommand;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    /// <summary>
    /// Edit the selected custom command.
    /// </summary>
    public Task EditCustomCommand()
    {

        var childWindow = new CustomCommandChildWindow();

        var childWindowViewModel = new CustomCommandViewModel(instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            SettingsManager.Current.IPScanner_CustomCommands.Remove(SelectedCustomCommand);
            SettingsManager.Current.IPScanner_CustomCommands.Add(new CustomCommandInfo(instance.ID, instance.Name,
                instance.FilePath, instance.Arguments));
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        }, true, SelectedCustomCommand);

        childWindow.Title = Strings.EditCustomCommand;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        return Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    /// <summary>
    /// Deletes the selected custom command.
    /// </summary>
    private async Task DeleteCustomCommand()
    {
        var result = await DialogHelper.ShowConfirmationMessageAsync(Application.Current.MainWindow,
            Strings.DeleteCustomCommand,
            Strings.DeleteCustomCommandMessage,
            ChildWindowIcon.Info,
            Strings.Delete);

        if (!result)
            return;

        SettingsManager.Current.IPScanner_CustomCommands.Remove(SelectedCustomCommand);
    }

    #endregion
}