using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using MahApps.Metro.Controls.Dialogs;
using NETworkManager.Localization.Resources;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.ViewModels;

public class IPScannerSettingsViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    private readonly IDialogCoordinator _dialogCoordinator;

    private bool _showAllResults;

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

    private int _icmpAttempts;

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

    private int _icmpTimeout;

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

    private int _icmpBuffer;

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

    private bool _resolveHostname;

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

    private bool _portScanEnabled;

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

    private string _portScanPorts;

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

    private int _portScanTimeout;

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

    private bool _netBIOSEnabled;

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

    private int _netBIOSTimeout;

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

    private bool _resolveMACAddress;

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

    public ICollectionView CustomCommands { get; }

    private CustomCommandInfo _selectedCustomCommand = new();

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

    private int _maxHostThreads;

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

    private int _maxPortThreads;

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

    public ICommand AddCustomCommandCommand => new RelayCommand(_ => AddCustomCommandAction());

    private void AddCustomCommandAction()
    {
        AddCustomCommand();
    }

    public ICommand EditCustomCommandCommand => new RelayCommand(_ => EditCustomCommandAction());

    private void EditCustomCommandAction()
    {
        EditCustomCommand();
    }

    public ICommand DeleteCustomCommandCommand => new RelayCommand(_ => DeleteCustomCommandAction());

    private void DeleteCustomCommandAction()
    {
        DeleteCustomCommand();
    }

    #endregion

    #region Methods

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

    private async void DeleteCustomCommand()
    {
        var customDialog = new CustomDialog
        {
            Title = Strings.DeleteCustomCommand
        };

        var confirmDeleteViewModel = new ConfirmDeleteViewModel(_ =>
            {
                _dialogCoordinator.HideMetroDialogAsync(this, customDialog);

                SettingsManager.Current.IPScanner_CustomCommands.Remove(SelectedCustomCommand);
            }, _ => { _dialogCoordinator.HideMetroDialogAsync(this, customDialog); },
            Strings.DeleteCustomCommandMessage);

        customDialog.Content = new ConfirmDeleteDialog
        {
            DataContext = confirmDeleteViewModel
        };

        await _dialogCoordinator.ShowMetroDialogAsync(this, customDialog);
    }

    #endregion
}