using MahApps.Metro.Controls.Dialogs;
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
    /// The dialog coordinator instance.
    /// </summary>
    private readonly IDialogCoordinator _dialogCoordinator;

    /// <summary>
    /// Backing field for <see cref="ShowAllResults"/>.
    /// </summary>
    private bool _showAllResults;

    /// <summary>
    /// Gets or sets a value indicating whether to show all results.
    /// </summary>
    public bool ShowAllResults
    {
        get => _showAllResults;
        set
        {
            if (value == _showAllResults)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_ShowAllResults = value;

            _showAllResults = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="ICMPAttempts"/>.
    /// </summary>
    private int _icmpAttempts;

    /// <summary>
    /// Gets or sets the number of ICMP attempts.
    /// </summary>
    public int ICMPAttempts
    {
        get => _icmpAttempts;
        set
        {
            if (value == _icmpAttempts)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_ICMPAttempts = value;

            _icmpAttempts = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="ICMPTimeout"/>.
    /// </summary>
    private int _icmpTimeout;

    /// <summary>
    /// Gets or sets the ICMP timeout in milliseconds.
    /// </summary>
    public int ICMPTimeout
    {
        get => _icmpTimeout;
        set
        {
            if (value == _icmpTimeout)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_ICMPTimeout = value;

            _icmpTimeout = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="ICMPBuffer"/>.
    /// </summary>
    private int _icmpBuffer;

    /// <summary>
    /// Gets or sets the ICMP buffer size.
    /// </summary>
    public int ICMPBuffer
    {
        get => _icmpBuffer;
        set
        {
            if (value == _icmpBuffer)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_ICMPBuffer = value;

            _icmpBuffer = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="ResolveHostname"/>.
    /// </summary>
    private bool _resolveHostname;

    /// <summary>
    /// Gets or sets a value indicating whether to resolve the hostname.
    /// </summary>
    public bool ResolveHostname
    {
        get => _resolveHostname;
        set
        {
            if (value == _resolveHostname)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_ResolveHostname = value;

            _resolveHostname = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="PortScanEnabled"/>.
    /// </summary>
    private bool _portScanEnabled;

    /// <summary>
    /// Gets or sets a value indicating whether port scanning is enabled.
    /// </summary>
    public bool PortScanEnabled
    {
        get => _portScanEnabled;
        set
        {
            if (value == _portScanEnabled)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_PortScanEnabled = value;

            _portScanEnabled = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="PortScanPorts"/>.
    /// </summary>
    private string _portScanPorts;

    /// <summary>
    /// Gets or sets the ports to scan.
    /// </summary>
    public string PortScanPorts
    {
        get => _portScanPorts;
        set
        {
            if (value == _portScanPorts)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_PortScanPorts = value;

            _portScanPorts = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="PortScanTimeout"/>.
    /// </summary>
    private int _portScanTimeout;

    /// <summary>
    /// Gets or sets the port scan timeout in milliseconds.
    /// </summary>
    public int PortScanTimeout
    {
        get => _portScanTimeout;
        set
        {
            if (value == _portScanTimeout)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_PortScanTimeout = value;

            _portScanTimeout = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="NetBIOSEnabled"/>.
    /// </summary>
    private bool _netBIOSEnabled;

    /// <summary>
    /// Gets or sets a value indicating whether NetBIOS is enabled.
    /// </summary>
    public bool NetBIOSEnabled
    {
        get => _netBIOSEnabled;
        set
        {
            if (value == _netBIOSEnabled)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_NetBIOSEnabled = value;

            _netBIOSEnabled = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="NetBIOSTimeout"/>.
    /// </summary>
    private int _netBIOSTimeout;

    /// <summary>
    /// Gets or sets the NetBIOS timeout in milliseconds.
    /// </summary>
    public int NetBIOSTimeout
    {
        get => _netBIOSTimeout;
        set
        {
            if (value == _netBIOSTimeout)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_NetBIOSTimeout = value;

            _netBIOSTimeout = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="ResolveMACAddress"/>.
    /// </summary>
    private bool _resolveMACAddress;

    /// <summary>
    /// Gets or sets a value indicating whether to resolve the MAC address.
    /// </summary>
    public bool ResolveMACAddress
    {
        get => _resolveMACAddress;
        set
        {
            if (value == _resolveMACAddress)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_ResolveMACAddress = value;

            _resolveMACAddress = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the collection view of custom commands.
    /// </summary>
    public ICollectionView CustomCommands { get; }

    /// <summary>
    /// Backing field for <see cref="SelectedCustomCommand"/>.
    /// </summary>
    private CustomCommandInfo _selectedCustomCommand = new();

    /// <summary>
    /// Gets or sets the selected custom command.
    /// </summary>
    public CustomCommandInfo SelectedCustomCommand
    {
        get => _selectedCustomCommand;
        set
        {
            if (value == _selectedCustomCommand)
                return;

            _selectedCustomCommand = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="MaxHostThreads"/>.
    /// </summary>
    private int _maxHostThreads;

    /// <summary>
    /// Gets or sets the maximum number of host threads.
    /// </summary>
    public int MaxHostThreads
    {
        get => _maxHostThreads;
        set
        {
            if (value == _maxHostThreads)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_MaxHostThreads = value;

            _maxHostThreads = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Backing field for <see cref="MaxPortThreads"/>.
    /// </summary>
    private int _maxPortThreads;

    /// <summary>
    /// Gets or sets the maximum number of port threads.
    /// </summary>
    public int MaxPortThreads
    {
        get => _maxPortThreads;
        set
        {
            if (value == _maxPortThreads)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_MaxPortThreads = value;

            _maxPortThreads = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="IPScannerSettingsViewModel"/> class.
    /// </summary>
    /// <param name="instance">The dialog coordinator instance.</param>
    public IPScannerSettingsViewModel(IDialogCoordinator instance)
    {
        _isLoading = true;

        _dialogCoordinator = instance;

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
    /// Adds a new custom command.
    /// </summary>
    private async void AddCustomCommand()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.AddCustomCommand
        };

        var customCommandViewModel = new CustomCommandViewModel(instance =>
        {
            _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

            SettingsManager.Current.IPScanner_CustomCommands.Add(new CustomCommandInfo(instance.ID, instance.Name,
                instance.FilePath, instance.Arguments));
        }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); });

        customDialog.Content = new CustomCommandDialog
        {
            DataContext = customCommandViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    /// <summary>
    /// Edits the selected custom command.
    /// </summary>
    public async void EditCustomCommand()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.EditCustomCommand
        };

        var customCommandViewModel = new CustomCommandViewModel(instance =>
        {
            _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

            SettingsManager.Current.IPScanner_CustomCommands.Remove(SelectedCustomCommand);
            SettingsManager.Current.IPScanner_CustomCommands.Add(new CustomCommandInfo(instance.ID, instance.Name,
                instance.FilePath, instance.Arguments));
        }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); }, true, SelectedCustomCommand);

        customDialog.Content = new CustomCommandDialog
        {
            DataContext = customCommandViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
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