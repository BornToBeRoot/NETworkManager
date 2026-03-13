using System;
using System.Windows.Input;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class FirewallSettingsViewModel : ViewModelBase
{
    private static readonly Lazy<FirewallSettingsViewModel> Lazy = new(() => new FirewallSettingsViewModel());

    public static FirewallSettingsViewModel Instance => Lazy.Value;

    #region Variables
    /// <summary>
    /// The view model is loading initial settings.
    /// </summary>
    private readonly bool _isLoading;

    /// <summary>
    /// Setting for combining the port history of local and remote ports.
    /// </summary>
    public bool CombinePortHistory
    {
        get;
        set
        {
            if (value == field)
                return;
            if (!_isLoading)
                SettingsManager.Current.Firewall_CombinePortHistory = value;

            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Setting to use the port separator comma instead of semicolon like it is used in WF.msc.
    /// </summary>
    /// <remarks>
    /// The PortScanner application uses semicolon. 
    /// </remarks>
    public bool UseWindowsPortSyntax
    {
        get;
        set
        {
            if (value == field)
                return;
            if (!_isLoading)
                SettingsManager.Current.Firewall_UseWindowsPortSyntax = value;
            field = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Whether the remote ports history has entries.
    /// </summary>
    public bool RemotePortsHaveItems
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
    /// Whether the local ports history has entries.
    /// </summary>
    public bool LocalPortsHaveItems
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
    /// Configurable length of history entries.
    /// </summary>
    public int MaxLengthHistory
    {
        get;
        set
        {
            if (value == field)
                return;
            field = value;
            SettingsManager.Current.Firewall_MaxLengthHistory = value;
            OnPropertyChanged();
        }
    }
    #endregion

    #region Constructor, load settings
    /// <summary>
    /// Construct the view model and load settings.
    /// </summary>
    private FirewallSettingsViewModel()
    {
        _isLoading = true;

        LoadSettings();
        
        _isLoading = false;
    }

    /// <summary>
    /// Load the settings via <see cref="SettingsManager"/>.
    /// </summary>
    private void LoadSettings()
    {
        CombinePortHistory = SettingsManager.Current.Firewall_CombinePortHistory;
        UseWindowsPortSyntax = SettingsManager.Current.Firewall_UseWindowsPortSyntax;
        LocalPortsHaveItems = SettingsManager.Current.Firewall_LocalPortsHistoryConfig?.Count > 0;
        RemotePortsHaveItems = SettingsManager.Current.Firewall_RemotePortsHistoryConfig?.Count > 0;
        MaxLengthHistory = SettingsManager.Current.Firewall_MaxLengthHistory;
        // This default value is only present when the settings are initialized for the first time,
        // because Int32Validator does not allow 0 or negative values as input.
        if (MaxLengthHistory is 0)
            MaxLengthHistory = -1;
    }
    #endregion

    #region Commands
    /// <summary>
    /// Command for <see cref="ClearLocalPortHistoryAction" />.
    /// </summary>
    public ICommand ClearLocalPortHistoryCommand => new RelayCommand(_ => ClearLocalPortHistoryAction());

    /// <summary>
    /// Action for clearing the LocalPort history. />.
    /// </summary>
    private void ClearLocalPortHistoryAction()
    {
        SettingsManager.Current.Firewall_LocalPortsHistoryConfig.Clear();
        LocalPortsHaveItems = false;
    }

    /// <summary>
    /// Command for <see cref="ClearRemotePortHistoryAction" />.
    /// </summary>
    public ICommand ClearRemotePortHistoryCommand => new RelayCommand(_ => ClearRemotePortHistoryAction());

    /// <summary>
    /// Action for clearing the LocalPort history. />.
    /// </summary>
    private void ClearRemotePortHistoryAction()
    {
        SettingsManager.Current.Firewall_RemotePortsHistoryConfig.Clear();
        RemotePortsHaveItems = false;
    }

    #endregion
}