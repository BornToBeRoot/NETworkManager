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
    /// Gets or sets a value indicating whether the details of every host are expanded in the result.
    /// </summary>
    public bool ExpandHostView
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_ExpandHostView = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the hosts are pinged.
    /// </summary>
    public bool ICMPEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_ICMPEnabled = value;

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
    /// Gets or sets the number of attempts to connect to a port if there was no answer.
    /// </summary>
    public int PortScanAttempts
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_PortScanAttempts = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether hosts in a local subnet are found by their neighbor entry (ARP / NDP).
    /// </summary>
    public bool ARPEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_ARPEnabled = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the operating system is guessed from the TTL of the ping reply.
    /// </summary>
    public bool OSGuessEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_OSGuessEnabled = value;

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
    /// Gets or sets a value indicating whether the mDNS hostname is resolved.
    /// </summary>
    public bool MDNSEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_MDNSEnabled = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the mDNS timeout in milliseconds.
    /// </summary>
    public int MDNSTimeout
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_MDNSTimeout = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the title of web pages is read.
    /// </summary>
    public bool HttpTitleEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_HttpTitleEnabled = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the timeout of a web request in milliseconds.
    /// </summary>
    public int HttpTitleTimeout
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_HttpTitleTimeout = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the SNMP system information is read.
    /// </summary>
    public bool SNMPEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_SNMPEnabled = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets the SNMP versions that can be used to read the system information (v1, v2c - v3 needs credentials).
    /// </summary>
    public SNMPVersion[] SNMPVersions { get; } = [SNMPVersion.V1, SNMPVersion.V2C];

    /// <summary>
    /// Gets or sets the SNMP version (v1 or v2c).
    /// </summary>
    public SNMPVersion SNMPVersion
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_SNMPVersion = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the SNMP community (saved as plain text).
    /// </summary>
    public string SNMPCommunity
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_SNMPCommunity = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Gets or sets the SNMP timeout in milliseconds.
    /// </summary>
    public int SNMPTimeout
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.IPScanner_SNMPTimeout = value;

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
    /// Gets or sets the selected custom commands (several can be deleted at once).
    /// </summary>
    public IList SelectedCustomCommands
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
        ExpandHostView = SettingsManager.Current.IPScanner_ExpandHostView;
        ICMPEnabled = SettingsManager.Current.IPScanner_ICMPEnabled;
        ICMPAttempts = SettingsManager.Current.IPScanner_ICMPAttempts;
        ICMPTimeout = SettingsManager.Current.IPScanner_ICMPTimeout;
        ICMPBuffer = SettingsManager.Current.IPScanner_ICMPBuffer;
        OSGuessEnabled = SettingsManager.Current.IPScanner_OSGuessEnabled;
        ResolveHostname = SettingsManager.Current.IPScanner_ResolveHostname;
        PortScanEnabled = SettingsManager.Current.IPScanner_PortScanEnabled;
        PortScanPorts = SettingsManager.Current.IPScanner_PortScanPorts;
        PortScanTimeout = SettingsManager.Current.IPScanner_PortScanTimeout;
        PortScanAttempts = SettingsManager.Current.IPScanner_PortScanAttempts;
        NetBIOSEnabled = SettingsManager.Current.IPScanner_NetBIOSEnabled;
        NetBIOSTimeout = SettingsManager.Current.IPScanner_NetBIOSTimeout;
        ARPEnabled = SettingsManager.Current.IPScanner_ARPEnabled;
        ResolveMACAddress = SettingsManager.Current.IPScanner_ResolveMACAddress;
        MDNSEnabled = SettingsManager.Current.IPScanner_MDNSEnabled;
        MDNSTimeout = SettingsManager.Current.IPScanner_MDNSTimeout;
        HttpTitleEnabled = SettingsManager.Current.IPScanner_HttpTitleEnabled;
        HttpTitleTimeout = SettingsManager.Current.IPScanner_HttpTitleTimeout;
        SNMPEnabled = SettingsManager.Current.IPScanner_SNMPEnabled;
        SNMPVersion = SettingsManager.Current.IPScanner_SNMPVersion;
        SNMPCommunity = SettingsManager.Current.IPScanner_SNMPCommunity;
        SNMPTimeout = SettingsManager.Current.IPScanner_SNMPTimeout;
        MaxHostThreads = SettingsManager.Current.IPScanner_MaxHostThreads;
        MaxPortThreads = SettingsManager.Current.IPScanner_MaxPortThreads;
    }

    #endregion

    #region ICommand & Actions

    /// <summary>
    /// Gets the command to restore the default ports of the port scan (e.g. overwritten by accident). Disabled while
    /// the default ports are set.
    /// </summary>
    public ICommand RestoreDefaultPortScanPortsCommand => new RelayCommand(
        _ => PortScanPorts = GlobalStaticConfiguration.IPScanner_PortScanPorts,
        _ => PortScanPorts != GlobalStaticConfiguration.IPScanner_PortScanPorts);

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
    public ICommand EditCustomCommandCommand =>
        new RelayCommand(_ => EditCustomCommandAction(), EditCustomCommand_CanExecute);

    /// <summary>
    /// Checks if exactly one custom command is selected (e.g. F2 without a selection or with several selected).
    /// </summary>
    private bool EditCustomCommand_CanExecute(object parameter)
    {
        return SelectedCustomCommand != null && SelectedCustomCommands.Count == 1;
    }

    /// <summary>
    /// Action to edit a custom command.
    /// </summary>
    private void EditCustomCommandAction()
    {
        EditCustomCommand();
    }

    /// <summary>
    /// Gets the command to delete the selected custom commands.
    /// </summary>
    public ICommand DeleteCustomCommandCommand =>
        new RelayCommand(_ => DeleteCustomCommandAction(), DeleteCustomCommand_CanExecute);

    /// <summary>
    /// Checks if at least one custom command is selected (e.g. Del without a selection).
    /// </summary>
    private bool DeleteCustomCommand_CanExecute(object parameter)
    {
        return SelectedCustomCommands.Count > 0;
    }

    /// <summary>
    /// Action to delete a custom command.
    /// </summary>
    private void DeleteCustomCommandAction()
    {
        _ = DeleteCustomCommand();
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
    /// Deletes the selected custom commands (after a confirmation that lists them).
    /// </summary>
    private async Task DeleteCustomCommand()
    {
        var customCommands = SelectedCustomCommands.Cast<CustomCommandInfo>().ToList();

        var result = await DialogHelper.ShowConfirmationMessageAsync(Application.Current.MainWindow,
            customCommands.Count == 1 ? Strings.DeleteCustomCommand : Strings.DeleteCustomCommands,
            (customCommands.Count == 1 ? Strings.DeleteCustomCommandMessage : Strings.DeleteCustomCommandsMessage) +
            Environment.NewLine + Environment.NewLine +
            DialogHelper.FormatItemList([.. customCommands.Select(x => x.Name)]),
            ChildWindowIcon.Info,
            Strings.Delete);

        if (!result)
            return;

        foreach (var customCommand in customCommands)
            SettingsManager.Current.IPScanner_CustomCommands.Remove(customCommand);
    }

    #endregion
}