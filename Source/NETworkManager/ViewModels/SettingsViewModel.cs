using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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
        private SettingsSettingsView _settingsSettingsView;
        private SettingsUpdateView _settingsUpdateView;
        private SettingsImportExportView _settingsImportExportView;
        private DashboardSettingsView _dashboardSettingsView;
        private IPScannerSettingsView _ipScannerSettingsView;
        private PortScannerSettingsView _portScannerSettingsView;
        private PingSettingsView _pingSettingsViewModel;
        private TracerouteSettingsView _tracerouteSettingsView;
        private DNSLookupSettingsView _dnsLookupSettingsViewModel;
        private RemoteDesktopSettingsView _remoteDesktopSettingsView;
        private PowerShellSettingsView _powerShellSettingsView;
        private PuTTYSettingsView _puTTYSettingsView;
        private TigerVNCSettingsView _tigerVNCSettingsView;
        private SNMPSettingsView _snmpSettingsView;
        private WakeOnLANSettingsView _wakeOnLANSettingsView;
        private HTTPHeadersSettingsView _httpHeadersSettingsView;
        private WhoisSettingsView _whoisSettingsView;
        #endregion

        #region Contructor, load settings
        public SettingsViewModel(ApplicationViewManager.Name applicationName)
        {
            LoadSettings();

            ChangeSettingsView(applicationName);
        }

        private void LoadSettings()
        {
            SettingsViews = new CollectionViewSource { Source = SettingsViewManager.List }.View;
            SettingsViews.GroupDescriptions.Add(new PropertyGroupDescription(nameof(SettingsViewInfo.TranslatedGroup)));
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
                return regex.Replace(info.TranslatedName, "").IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1 || (regex.Replace(info.Name.ToString(), "").IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1);
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
        public void ChangeSettingsView(ApplicationViewManager.Name applicationName)
        {
            // Don't change the view, if the user has filtered the settings...
            if (!string.IsNullOrEmpty(Search))
                return;

            if (System.Enum.GetNames(typeof(SettingsViewManager.Name)).Contains(applicationName.ToString()) && ApplicationViewManager.Name.None.ToString() != applicationName.ToString())
                SelectedSettingsView = SettingsViews.SourceCollection.Cast<SettingsViewInfo>().FirstOrDefault(x => x.Name.ToString() == applicationName.ToString());
            else
                SelectedSettingsView = SettingsViews.SourceCollection.Cast<SettingsViewInfo>().FirstOrDefault(x => x.Name == SettingsViewManager.Name.General);
        }

        private void ChangeSettingsContent(SettingsViewInfo settingsViewInfo)
        {
            switch (settingsViewInfo.Name)
            {
                case SettingsViewManager.Name.General:
                    if (_settingsGerneralView == null)
                        _settingsGerneralView = new SettingsGeneralView();

                    SettingsContent = _settingsGerneralView;
                    break;
                case SettingsViewManager.Name.Window:
                    if (_settingsWindowView == null)
                        _settingsWindowView = new SettingsWindowView();

                    SettingsContent = _settingsWindowView;
                    break;
                case SettingsViewManager.Name.Appearance:
                    if (_settingsApperanceView == null)
                        _settingsApperanceView = new SettingsAppearanceView();

                    SettingsContent = _settingsApperanceView;
                    break;
                case SettingsViewManager.Name.Language:
                    if (_settingsLanguageView == null)
                        _settingsLanguageView = new SettingsLanguageView();

                    SettingsContent = _settingsLanguageView;
                    break;
                case SettingsViewManager.Name.Status:
                    if (_settingsStatusView == null)
                        _settingsStatusView = new SettingsStatusView();

                    SettingsContent = _settingsStatusView;
                    break;
                case SettingsViewManager.Name.HotKeys:
                    if (_settingsHotKeysView == null)
                        _settingsHotKeysView = new SettingsHotKeysView();

                    SettingsContent = _settingsHotKeysView;
                    break;
                case SettingsViewManager.Name.Autostart:
                    if (_settingsAutostartView == null)
                        _settingsAutostartView = new SettingsAutostartView();

                    SettingsContent = _settingsAutostartView;
                    break;
                case SettingsViewManager.Name.Settings:
                    if (_settingsSettingsView == null)
                        _settingsSettingsView = new SettingsSettingsView();

                    // Save settings (if changed) and check if files exists
                    _settingsSettingsView.OnVisible();

                    SettingsContent = _settingsSettingsView;
                    break;
                case SettingsViewManager.Name.Update:
                    if (_settingsUpdateView == null)
                        _settingsUpdateView = new SettingsUpdateView();

                    SettingsContent = _settingsUpdateView;
                    break;
                case SettingsViewManager.Name.ImportExport:
                    if (_settingsImportExportView == null)
                        _settingsImportExportView = new SettingsImportExportView();

                    // Save settings (if changed) and check if files exists
                    _settingsImportExportView.OnVisible();

                    SettingsContent = _settingsImportExportView;
                    break;
                case SettingsViewManager.Name.Dashboard:
                    if (_dashboardSettingsView == null)
                        _dashboardSettingsView = new DashboardSettingsView();

                    SettingsContent = _dashboardSettingsView;
                    break;
                case SettingsViewManager.Name.IPScanner:
                    if (_ipScannerSettingsView == null)
                        _ipScannerSettingsView = new IPScannerSettingsView();

                    SettingsContent = _ipScannerSettingsView;
                    break;
                case SettingsViewManager.Name.PortScanner:
                    if (_portScannerSettingsView == null)
                        _portScannerSettingsView = new PortScannerSettingsView();

                    SettingsContent = _portScannerSettingsView;
                    break;

                case SettingsViewManager.Name.Ping:
                    if (_pingSettingsViewModel == null)
                        _pingSettingsViewModel = new PingSettingsView();

                    SettingsContent = _pingSettingsViewModel;
                    break;
                case SettingsViewManager.Name.Traceroute:
                    if (_tracerouteSettingsView == null)
                        _tracerouteSettingsView = new TracerouteSettingsView();

                    SettingsContent = _tracerouteSettingsView;
                    break;
                case SettingsViewManager.Name.DNSLookup:
                    if (_dnsLookupSettingsViewModel == null)
                        _dnsLookupSettingsViewModel = new DNSLookupSettingsView();

                    SettingsContent = _dnsLookupSettingsViewModel;
                    break;
                case SettingsViewManager.Name.RemoteDesktop:
                    if (_remoteDesktopSettingsView == null)
                        _remoteDesktopSettingsView = new RemoteDesktopSettingsView();

                    SettingsContent = _remoteDesktopSettingsView;
                    break;
                case SettingsViewManager.Name.PowerShell:
                    if (_powerShellSettingsView == null)
                        _powerShellSettingsView = new PowerShellSettingsView();

                    SettingsContent = _powerShellSettingsView;
                    break;
                case SettingsViewManager.Name.PuTTY:
                    if (_puTTYSettingsView == null)
                        _puTTYSettingsView = new PuTTYSettingsView();

                    SettingsContent = _puTTYSettingsView;
                    break;
                case SettingsViewManager.Name.TigerVNC:
                    if (_tigerVNCSettingsView == null)
                        _tigerVNCSettingsView = new TigerVNCSettingsView();

                    SettingsContent = _tigerVNCSettingsView;
                    break;
                case SettingsViewManager.Name.SNMP:
                    if (_snmpSettingsView == null)
                        _snmpSettingsView = new SNMPSettingsView();

                    SettingsContent = _snmpSettingsView;
                    break;
                case SettingsViewManager.Name.WakeOnLAN:
                    if (_wakeOnLANSettingsView == null)
                        _wakeOnLANSettingsView = new WakeOnLANSettingsView();

                    SettingsContent = _wakeOnLANSettingsView;
                    break;
                case SettingsViewManager.Name.HTTPHeaders:
                    if (_httpHeadersSettingsView == null)
                        _httpHeadersSettingsView = new HTTPHeadersSettingsView();

                    SettingsContent = _httpHeadersSettingsView;
                    break;
                case SettingsViewManager.Name.Whois:
                    if (_whoisSettingsView == null)
                        _whoisSettingsView = new WhoisSettingsView();

                    SettingsContent = _whoisSettingsView;
                    break;
            }
        }
        #endregion
    }
}