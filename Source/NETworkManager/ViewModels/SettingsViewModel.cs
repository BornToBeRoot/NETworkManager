using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using NETworkManager.Documentation;
using NETworkManager.Localization.Translators;
using NETworkManager.Models;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using NETworkManager.Views;

namespace NETworkManager.ViewModels
{
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

        private string _search;
        public string Search
        {
            get => _search;
            set
            {
                if (value == _search)
                    return;

                _search = value;

                SettingsViews.Refresh();

                // Show note when there was nothing found
                SearchNothingFound = !SettingsViews.Cast<SettingsViewInfo>().Any();

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
            set
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

        private SettingsGeneralView _settingsGerneralView;
        private SettingsWindowView _settingsWindowView;
        private SettingsAppearanceView _settingsApperanceView;
        private SettingsLanguageView _settingsLanguageView;
        private SettingsNetworkView _settingsNetworkView;
        private SettingsStatusView _settingsStatusView;
        private SettingsHotKeysView _settingsHotKeysView;
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
        private SNMPSettingsView _snmpSettingsView;
        private WakeOnLANSettingsView _wakeOnLANSettingsView;
        //private WhoisSettingsView _whoisSettingsView;
        private BitCalculatorSettingsView _bitCalculatorSettingsView;
        #endregion

        #region Contructor, load settings
        public SettingsViewModel(ApplicationName applicationName)
        {
            LoadSettings();

            ChangeSettingsView(applicationName);
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
                return regex.Replace(SettingsViewNameTranslator.GetInstance().Translate(info.Name), "").IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || (regex.Replace(info.Name.ToString(), "").IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1);
            };
        }
        #endregion

        #region ICommands & Actions
        public ICommand ClearSearchCommand => new RelayCommand(p => ClearSearchAction());

        private void ClearSearchAction()
        {
            Search = string.Empty;
        }
        #endregion

        #region Methods
        public void ChangeSettingsView(ApplicationName applicationName)
        {
            // Don't change the view, if the user has filtered the settings...
            if (!string.IsNullOrEmpty(Search))
                return;

            if (Enum.GetNames(typeof(SettingsViewName)).Contains(applicationName.ToString()) && ApplicationName.None.ToString() != applicationName.ToString())
                SelectedSettingsView = SettingsViews.SourceCollection.Cast<SettingsViewInfo>().FirstOrDefault(x => x.Name.ToString() == applicationName.ToString());
            else
                SelectedSettingsView = SettingsViews.SourceCollection.Cast<SettingsViewInfo>().FirstOrDefault(x => x.Name == SettingsViewName.General);
        }

        private void ChangeSettingsContent(SettingsViewInfo settingsViewInfo)
        {
            switch (settingsViewInfo.Name)
            {
                case SettingsViewName.General:
                    _settingsGerneralView ??= new SettingsGeneralView();

                    SettingsContent = _settingsGerneralView;
                    break;
                case SettingsViewName.Window:
                    _settingsWindowView ??= new SettingsWindowView();

                    SettingsContent = _settingsWindowView;
                    break;
                case SettingsViewName.Appearance:
                    _settingsApperanceView ??= new SettingsAppearanceView();

                    SettingsContent = _settingsApperanceView;
                    break;
                case SettingsViewName.Language:
                    _settingsLanguageView ??= new SettingsLanguageView();

                    SettingsContent = _settingsLanguageView;
                    break;
                case SettingsViewName.Network:
                    _settingsNetworkView ??= new SettingsNetworkView();

                    SettingsContent = _settingsNetworkView;
                    break;
                case SettingsViewName.Status:
                    _settingsStatusView ??= new SettingsStatusView();

                    SettingsContent = _settingsStatusView;
                    break;
                case SettingsViewName.HotKeys:
                    _settingsHotKeysView ??= new SettingsHotKeysView();

                    SettingsContent = _settingsHotKeysView;
                    break;
                case SettingsViewName.Autostart:
                    _settingsAutostartView ??= new SettingsAutostartView();

                    SettingsContent = _settingsAutostartView;
                    break;              
                case SettingsViewName.Update:
                    _settingsUpdateView ??= new SettingsUpdateView();

                    SettingsContent = _settingsUpdateView;
                    break;
                case SettingsViewName.Settings:
                    _settingsSettingsView ??= new SettingsSettingsView();

                    // Save settings
                    _settingsSettingsView.OnVisible();

                    SettingsContent = _settingsSettingsView;
                    break;
                case SettingsViewName.Profiles:
                    _settingsProfilesView ??= new SettingsProfilesView();

                    SettingsContent = _settingsProfilesView;
                    break;
                case SettingsViewName.Dashboard:
                    _dashboardSettingsView ??= new DashboardSettingsView();

                    SettingsContent = _dashboardSettingsView;
                    break;
                case SettingsViewName.IPScanner:
                    _ipScannerSettingsView ??= new IPScannerSettingsView();

                    SettingsContent = _ipScannerSettingsView;
                    break;
                case SettingsViewName.PortScanner:
                    _portScannerSettingsView ??= new PortScannerSettingsView();

                    SettingsContent = _portScannerSettingsView;
                    break;
                case SettingsViewName.PingMonitor:
                    _pingMonitorSettingsView ??= new PingMonitorSettingsView();

                    SettingsContent = _pingMonitorSettingsView;
                    break;
                case SettingsViewName.Traceroute:
                    _tracerouteSettingsView ??= new TracerouteSettingsView();

                    SettingsContent = _tracerouteSettingsView;
                    break;
                case SettingsViewName.DNSLookup:
                    _dnsLookupSettingsViewModel ??= new DNSLookupSettingsView();

                    SettingsContent = _dnsLookupSettingsViewModel;
                    break;
                case SettingsViewName.RemoteDesktop:
                    _remoteDesktopSettingsView ??= new RemoteDesktopSettingsView();

                    SettingsContent = _remoteDesktopSettingsView;
                    break;
                case SettingsViewName.PowerShell:
                    _powerShellSettingsView ??= new PowerShellSettingsView();

                    SettingsContent = _powerShellSettingsView;
                    break;
                case SettingsViewName.PuTTY:
                    _puTTYSettingsView ??= new PuTTYSettingsView();

                    SettingsContent = _puTTYSettingsView;
                    break;
                case SettingsViewName.AWSSessionManager:
                    _awsSessionManagerSettingsView ??= new AWSSessionManagerSettingsView();

                    SettingsContent = _awsSessionManagerSettingsView;
                    break;
                case SettingsViewName.TigerVNC:
                    _tigerVNCSettingsView ??= new TigerVNCSettingsView();

                    SettingsContent = _tigerVNCSettingsView;
                    break;
                case SettingsViewName.SNMP:
                    _snmpSettingsView ??= new SNMPSettingsView();

                    SettingsContent = _snmpSettingsView;
                    break;
                case SettingsViewName.WakeOnLAN:
                    _wakeOnLANSettingsView ??= new WakeOnLANSettingsView();

                    SettingsContent = _wakeOnLANSettingsView;
                    break;
                /*
                case SettingsViewName.Whois:
                    _whoisSettingsView ??= new WhoisSettingsView();

                    SettingsContent = _whoisSettingsView;
                    break;
                */
                case SettingsViewName.BitCalculator:
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
}