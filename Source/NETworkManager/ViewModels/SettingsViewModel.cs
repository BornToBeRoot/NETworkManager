using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using NETworkManager.Documentation;
using NETworkManager.Localization;
using NETworkManager.Models;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    #region Variables
    private int _selectedTabIndex;

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (value == _selectedTabIndex)
                return;

            _selectedTabIndex = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView SettingsViews { get; private set; }

    private SettingsName _searchLastSelectedSettingsName;

    private string _search;
    public string Search
    {
        get => _search;
        set
        {
            if (value == _search)
                return;

            _search = value;

            // Store the current selected settings view name
            if (SelectedSettingsView != null)
                _searchLastSelectedSettingsName = SelectedSettingsView.Name;

            // Refresh (apply filter)
            SettingsViews.Refresh();

            // Try to select the last selected application
            if (!SettingsViews.IsEmpty && SelectedSettingsView == null)
                SelectedSettingsView = SettingsViews.Cast<SettingsViewInfo>().FirstOrDefault(x => x.Name == _searchLastSelectedSettingsName) ?? SettingsViews.Cast<SettingsViewInfo>().FirstOrDefault();

            // Show note if nothing was found
            SearchNothingFound = SettingsViews.IsEmpty;            

            OnPropertyChanged();
        }
    }

    private bool _searchNothingFound;
    public bool SearchNothingFound
    {
        get => _searchNothingFound;
        set
        {
            if (value == _searchNothingFound)
                return;

            _searchNothingFound = value;
            OnPropertyChanged();
        }
    }

    private UserControl _settingsContent;
    public UserControl SettingsContent
    {
        get => _settingsContent;
        private set
        {
            if (Equals(value, _settingsContent))
                return;

            _settingsContent = value;
            OnPropertyChanged();
        }
    }

    private SettingsViewInfo _selectedSettingsView;
    public SettingsViewInfo SelectedSettingsView
    {
        get => _selectedSettingsView;
        set
        {
            if (value == _selectedSettingsView)
                return;

            if (value != null)
                ChangeSettingsContent(value);

            _selectedSettingsView = value;
            OnPropertyChanged();
        }
    }

    private SettingsGeneralView _settingsGeneralView;
    private SettingsWindowView _settingsWindowView;
    private SettingsAppearanceView _settingsAppearanceView;
    private SettingsLanguageView _settingsLanguageView;
    private SettingsNetworkView _settingsNetworkView;
    private SettingsStatusView _settingsStatusView;
    private SettingsHotkeysView _settingsHotKeysView;
    private SettingsAutostartView _settingsAutostartView;
    private SettingsUpdateView _settingsUpdateView;
    private SettingsSettingsView _settingsSettingsView;
    private SettingsProfilesView _settingsProfilesView;
    private DashboardSettingsView _dashboardSettingsView;
    private IPScannerSettingsView _ipScannerSettingsView;
    private PortScannerSettingsView _portScannerSettingsView;
    private PingMonitorSettingsView _pingMonitorSettingsView;
    private TracerouteSettingsView _tracerouteSettingsView;
    private DNSLookupSettingsView _dnsLookupSettingsViewModel;
    private RemoteDesktopSettingsView _remoteDesktopSettingsView;
    private PowerShellSettingsView _powerShellSettingsView;
    private PuTTYSettingsView _puTTYSettingsView;
    private AWSSessionManagerSettingsView _awsSessionManagerSettingsView;
    private TigerVNCSettingsView _tigerVNCSettingsView;
    private WebConsoleSettingsView _webConsoleSettingsView;
    private SNMPSettingsView _snmpSettingsView;
    private SNTPLookupSettingsView _sntpLookupSettingsView;
    private WakeOnLANSettingsView _wakeOnLANSettingsView;
    private BitCalculatorSettingsView _bitCalculatorSettingsView;
    #endregion

    #region Contructor, load settings
    public SettingsViewModel()
    {
        LoadSettings();
    }

    private void LoadSettings()
    {
        SettingsViews = new CollectionViewSource { Source = SettingsViewManager.List }.View;
        SettingsViews.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SettingsViewInfo.Group)));
        SettingsViews.SortDescriptions.Add(new SortDescription(nameof(SettingsViewInfo.Name), ListSortDirection.Ascending));
        SettingsViews.Filter = o =>
        {
            if (string.IsNullOrEmpty(Search))
                return true;

            if (o is not SettingsViewInfo info)
                return false;

            var regex = new Regex(@" |-");

            var search = regex.Replace(Search, "");

            // Search by TranslatedName and Name
            return regex.Replace(ResourceTranslator.Translate(ResourceIdentifier.SettingsName, info.Name), "").IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || (regex.Replace(info.Name.ToString(), "").IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1);
        };
    }
    #endregion

    #region ICommands & Actions
    public ICommand ClearSearchCommand => new RelayCommand(_ => ClearSearchAction());

    private void ClearSearchAction()
    {
        Search = string.Empty;
    }
    #endregion

    #region Methods
    public void ChangeSettingsView(ApplicationName applicationName)
    {
        if (SettingsViews.IsEmpty)
            return;

        // Try to find application in (filtered) settings views
        var selectedApplicationSettingsView = SettingsViews.Cast<SettingsViewInfo>().FirstOrDefault(x => x.Name.ToString() == applicationName.ToString());

        // Update selected settings view if application is found in (filtered) settings views
        if (selectedApplicationSettingsView != null)
            SelectedSettingsView = selectedApplicationSettingsView;

        // Set default settings view
        SelectedSettingsView ??= SettingsViews.Cast<SettingsViewInfo>().FirstOrDefault();
    }

    private void ChangeSettingsContent(SettingsViewInfo settingsViewInfo)
    {
        switch (settingsViewInfo.Name)
        {
            case SettingsName.General:
                _settingsGeneralView ??= new SettingsGeneralView();

                SettingsContent = _settingsGeneralView;
                break;
            case SettingsName.Window:
                _settingsWindowView ??= new SettingsWindowView();

                SettingsContent = _settingsWindowView;
                break;
            case SettingsName.Appearance:
                _settingsAppearanceView ??= new SettingsAppearanceView();

                SettingsContent = _settingsAppearanceView;
                break;
            case SettingsName.Language:
                _settingsLanguageView ??= new SettingsLanguageView();

                SettingsContent = _settingsLanguageView;
                break;
            case SettingsName.Network:
                _settingsNetworkView ??= new SettingsNetworkView();

                SettingsContent = _settingsNetworkView;
                break;
            case SettingsName.Status:
                _settingsStatusView ??= new SettingsStatusView();

                SettingsContent = _settingsStatusView;
                break;
            case SettingsName.HotKeys:
                _settingsHotKeysView ??= new SettingsHotkeysView();

                SettingsContent = _settingsHotKeysView;
                break;
            case SettingsName.Autostart:
                _settingsAutostartView ??= new SettingsAutostartView();

                SettingsContent = _settingsAutostartView;
                break;
            case SettingsName.Update:
                _settingsUpdateView ??= new SettingsUpdateView();

                SettingsContent = _settingsUpdateView;
                break;
            case SettingsName.Settings:
                _settingsSettingsView ??= new SettingsSettingsView();

                SettingsContent = _settingsSettingsView;
                break;
            case SettingsName.Profiles:
                _settingsProfilesView ??= new SettingsProfilesView();

                SettingsContent = _settingsProfilesView;
                break;
            case SettingsName.Dashboard:
                _dashboardSettingsView ??= new DashboardSettingsView();

                SettingsContent = _dashboardSettingsView;
                break;
            case SettingsName.IPScanner:
                _ipScannerSettingsView ??= new IPScannerSettingsView();

                SettingsContent = _ipScannerSettingsView;
                break;
            case SettingsName.PortScanner:
                _portScannerSettingsView ??= new PortScannerSettingsView();

                SettingsContent = _portScannerSettingsView;
                break;
            case SettingsName.PingMonitor:
                _pingMonitorSettingsView ??= new PingMonitorSettingsView();

                SettingsContent = _pingMonitorSettingsView;
                break;
            case SettingsName.Traceroute:
                _tracerouteSettingsView ??= new TracerouteSettingsView();

                SettingsContent = _tracerouteSettingsView;
                break;
            case SettingsName.DNSLookup:
                _dnsLookupSettingsViewModel ??= new DNSLookupSettingsView();

                SettingsContent = _dnsLookupSettingsViewModel;
                break;
            case SettingsName.RemoteDesktop:
                _remoteDesktopSettingsView ??= new RemoteDesktopSettingsView();

                SettingsContent = _remoteDesktopSettingsView;
                break;
            case SettingsName.PowerShell:
                _powerShellSettingsView ??= new PowerShellSettingsView();

                SettingsContent = _powerShellSettingsView;
                break;
            case SettingsName.PuTTY:
                _puTTYSettingsView ??= new PuTTYSettingsView();

                SettingsContent = _puTTYSettingsView;
                break;
            case SettingsName.AWSSessionManager:
                _awsSessionManagerSettingsView ??= new AWSSessionManagerSettingsView();

                SettingsContent = _awsSessionManagerSettingsView;
                break;
            case SettingsName.TigerVNC:
                _tigerVNCSettingsView ??= new TigerVNCSettingsView();

                SettingsContent = _tigerVNCSettingsView;
                break;
            case SettingsName.WebConsole:
                _webConsoleSettingsView ??= new WebConsoleSettingsView();

                SettingsContent = _webConsoleSettingsView;
                break;
            case SettingsName.SNMP:
                _snmpSettingsView ??= new SNMPSettingsView();

                SettingsContent = _snmpSettingsView;
                break;
            case SettingsName.SNTPLookup:
                _sntpLookupSettingsView ??= new SNTPLookupSettingsView();

                SettingsContent = _sntpLookupSettingsView;
                break;
            case SettingsName.WakeOnLAN:
                _wakeOnLANSettingsView ??= new WakeOnLANSettingsView();

                SettingsContent = _wakeOnLANSettingsView;
                break;
            case SettingsName.BitCalculator:
                _bitCalculatorSettingsView ??= new BitCalculatorSettingsView();

                SettingsContent = _bitCalculatorSettingsView;
                break;
        }
    }

    public DocumentationIdentifier GetDocumentationIdentifier()
    {
        return SelectedTabIndex switch
        {
            0 => DocumentationManager.GetIdentifierBySettingsName(SelectedSettingsView.Name),
            1 => DocumentationIdentifier.Profiles,
            _ => DocumentationIdentifier.Default,
        };
    }
    #endregion
}
