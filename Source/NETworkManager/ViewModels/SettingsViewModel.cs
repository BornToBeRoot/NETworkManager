using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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
        private WhoisSettingsView _whoisSettingsView;
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

                if (!(o is SettingsViewInfo info))
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

            if (System.Enum.GetNames(typeof(SettingsViewName)).Contains(applicationName.ToString()) && ApplicationName.None.ToString() != applicationName.ToString())
                SelectedSettingsView = SettingsViews.SourceCollection.Cast<SettingsViewInfo>().FirstOrDefault(x => x.Name.ToString() == applicationName.ToString());
            else
                SelectedSettingsView = SettingsViews.SourceCollection.Cast<SettingsViewInfo>().FirstOrDefault(x => x.Name == SettingsViewName.General);
        }

        private void ChangeSettingsContent(SettingsViewInfo settingsViewInfo)
        {
            switch (settingsViewInfo.Name)
            {
                case SettingsViewName.General:
                    if (_settingsGerneralView == null)
                        _settingsGerneralView = new SettingsGeneralView();

                    SettingsContent = _settingsGerneralView;
                    break;
                case SettingsViewName.Window:
                    if (_settingsWindowView == null)
                        _settingsWindowView = new SettingsWindowView();

                    SettingsContent = _settingsWindowView;
                    break;
                case SettingsViewName.Appearance:
                    if (_settingsApperanceView == null)
                        _settingsApperanceView = new SettingsAppearanceView();

                    SettingsContent = _settingsApperanceView;
                    break;
                case SettingsViewName.Language:
                    if (_settingsLanguageView == null)
                        _settingsLanguageView = new SettingsLanguageView();

                    SettingsContent = _settingsLanguageView;
                    break;
                case SettingsViewName.Status:
                    if (_settingsStatusView == null)
                        _settingsStatusView = new SettingsStatusView();

                    SettingsContent = _settingsStatusView;
                    break;
                case SettingsViewName.HotKeys:
                    if (_settingsHotKeysView == null)
                        _settingsHotKeysView = new SettingsHotKeysView();

                    SettingsContent = _settingsHotKeysView;
                    break;
                case SettingsViewName.Autostart:
                    if (_settingsAutostartView == null)
                        _settingsAutostartView = new SettingsAutostartView();

                    SettingsContent = _settingsAutostartView;
                    break;              
                case SettingsViewName.Update:
                    if (_settingsUpdateView == null)
                        _settingsUpdateView = new SettingsUpdateView();

                    SettingsContent = _settingsUpdateView;
                    break;
                case SettingsViewName.Settings:
                    if (_settingsSettingsView == null)
                        _settingsSettingsView = new SettingsSettingsView();

                    // Save settings
                    _settingsSettingsView.OnVisible();

                    SettingsContent = _settingsSettingsView;
                    break;
                case SettingsViewName.Profiles:
                    if (_settingsProfilesView == null)
                        _settingsProfilesView = new SettingsProfilesView();

                    SettingsContent = _settingsProfilesView;
                    break;
                case SettingsViewName.Dashboard:
                    if (_dashboardSettingsView == null)
                        _dashboardSettingsView = new DashboardSettingsView();

                    SettingsContent = _dashboardSettingsView;
                    break;
                case SettingsViewName.IPScanner:
                    if (_ipScannerSettingsView == null)
                        _ipScannerSettingsView = new IPScannerSettingsView();

                    SettingsContent = _ipScannerSettingsView;
                    break;
                case SettingsViewName.PortScanner:
                    if (_portScannerSettingsView == null)
                        _portScannerSettingsView = new PortScannerSettingsView();

                    SettingsContent = _portScannerSettingsView;
                    break;
                case SettingsViewName.PingMonitor:
                    if (_pingMonitorSettingsView == null)
                        _pingMonitorSettingsView = new PingMonitorSettingsView();

                    SettingsContent = _pingMonitorSettingsView;
                    break;
                case SettingsViewName.Traceroute:
                    if (_tracerouteSettingsView == null)
                        _tracerouteSettingsView = new TracerouteSettingsView();

                    SettingsContent = _tracerouteSettingsView;
                    break;
                case SettingsViewName.DNSLookup:
                    if (_dnsLookupSettingsViewModel == null)
                        _dnsLookupSettingsViewModel = new DNSLookupSettingsView();

                    SettingsContent = _dnsLookupSettingsViewModel;
                    break;
                case SettingsViewName.RemoteDesktop:
                    if (_remoteDesktopSettingsView == null)
                        _remoteDesktopSettingsView = new RemoteDesktopSettingsView();

                    SettingsContent = _remoteDesktopSettingsView;
                    break;
                case SettingsViewName.PowerShell:
                    if (_powerShellSettingsView == null)
                        _powerShellSettingsView = new PowerShellSettingsView();

                    SettingsContent = _powerShellSettingsView;
                    break;
                case SettingsViewName.PuTTY:
                    if (_puTTYSettingsView == null)
                        _puTTYSettingsView = new PuTTYSettingsView();

                    SettingsContent = _puTTYSettingsView;
                    break;
                case SettingsViewName.AWSSessionManager:
                    if (_awsSessionManagerSettingsView == null)
                        _awsSessionManagerSettingsView = new AWSSessionManagerSettingsView();

                    SettingsContent = _awsSessionManagerSettingsView;
                    break;
                case SettingsViewName.TigerVNC:
                    if (_tigerVNCSettingsView == null)
                        _tigerVNCSettingsView = new TigerVNCSettingsView();

                    SettingsContent = _tigerVNCSettingsView;
                    break;
                case SettingsViewName.SNMP:
                    if (_snmpSettingsView == null)
                        _snmpSettingsView = new SNMPSettingsView();

                    SettingsContent = _snmpSettingsView;
                    break;
                case SettingsViewName.WakeOnLAN:
                    if (_wakeOnLANSettingsView == null)
                        _wakeOnLANSettingsView = new WakeOnLANSettingsView();

                    SettingsContent = _wakeOnLANSettingsView;
                    break;
                case SettingsViewName.Whois:
                    if (_whoisSettingsView == null)
                        _whoisSettingsView = new WhoisSettingsView();

                    SettingsContent = _whoisSettingsView;
                    break;
            }
        }
        #endregion
    }
}