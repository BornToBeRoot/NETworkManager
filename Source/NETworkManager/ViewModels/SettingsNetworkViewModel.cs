using MahApps.Metro.SimpleChildWindow;
using NETworkManager.Localization.Resources;
using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class SettingsNetworkViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    /// <summary>
    ///     Default values for the DNS server profile dialog.
    /// </summary>
    private readonly ServerConnectionInfo _profileDialogDefaultValues = new("1.1.1.1", 53, TransportProtocol.Udp);

    public bool UseCustomDNSServer
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Network_UseCustomDNSServer = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public string CustomDNSServersDisplay
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

    public bool AddDNSSuffix
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Network_AddDNSSuffix = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool UseCustomDNSSuffix
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Network_UseCustomDNSSuffix = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public string CustomDNSSuffix
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Network_CustomDNSSuffix = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool ResolveHostnamePreferIPv4
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.Network_ResolveHostnamePreferIPv4 = value;

            field = value;
            OnPropertyChanged();
        }
    }

    public bool ResolveHostnamePreferIPv6
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

    #region Constructor, LoadSettings

    public SettingsNetworkViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        UseCustomDNSServer = SettingsManager.Current.Network_UseCustomDNSServer;

        RefreshCustomDNSServersDisplay();

        AddDNSSuffix = SettingsManager.Current.Network_AddDNSSuffix;
        UseCustomDNSSuffix = SettingsManager.Current.Network_UseCustomDNSSuffix;
        CustomDNSSuffix = SettingsManager.Current.Network_CustomDNSSuffix;

        if (SettingsManager.Current.Network_ResolveHostnamePreferIPv4)
            ResolveHostnamePreferIPv4 = true;
        else
            ResolveHostnamePreferIPv6 = true;
    }

    private void RefreshCustomDNSServersDisplay()
    {
        CustomDNSServersDisplay = SettingsManager.Current.Network_CustomDNSServers.Count == 0 ? Strings.NotSet : string.Join("; ", SettingsManager.Current.Network_CustomDNSServers);
    }

    #endregion

    #region Commands

    public ICommand EditCustomDNSServersCommand => new RelayCommand(_ => EditCustomDNSServersAction());

    private void EditCustomDNSServersAction()
    {
        _ = EditCustomDNSServers();
    }

    #endregion

    #region Methods

    /// <summary>
    ///     Opens the DNS server profile dialog to edit the global custom DNS servers. The profile name
    ///     is fixed and read-only because there is only a single global list, not multiple named profiles.
    /// </summary>
    private async Task EditCustomDNSServers()
    {
        var childWindow = new ServerConnectionInfoProfileChildWindow();

        var info = new ServerConnectionInfoProfile(Strings.Default,
            [.. SettingsManager.Current.Network_CustomDNSServers]);

        var childWindowViewModel = new ServerConnectionInfoProfileViewModel(instance =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;

            SettingsManager.Current.Network_CustomDNSServers = new(instance.Servers);

            RefreshCustomDNSServersDisplay();
        }, _ =>
        {
            childWindow.IsOpen = false;
            ConfigurationManager.Current.IsChildWindowOpen = false;
        },
            ([], true, true),
            _profileDialogDefaultValues, info, true);

        childWindow.Title = Strings.EditDNSServer;

        childWindow.DataContext = childWindowViewModel;

        ConfigurationManager.Current.IsChildWindowOpen = true;

        await Application.Current.MainWindow.ShowChildWindowAsync(childWindow);
    }

    #endregion
}
