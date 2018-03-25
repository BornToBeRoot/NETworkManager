﻿using System.Linq;
using NETworkManager.Views;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Data;
using System;
using System.Text.RegularExpressions;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels.Settings
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Variables
        private CollectionViewSource _settingsViewsSource;
        public ICollectionView SettingsViews
        {
            get { return _settingsViewsSource.View; }
        }

        private string _search;
        public string Search
        {
            get { return _search; }
            set
            {
                if (value == _search)
                    return;

                _search = value;

                SettingsViews.Refresh();

                // Show note when there was nothing found
                SearchNothingFound = SettingsViews.Cast<SettingsViewInfo>().Count() == 0;

                OnPropertyChanged();
            }
        }

        private bool _searchNothingFound;
        public bool SearchNothingFound
        {
            get { return _searchNothingFound; }
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
            get { return _settingsContent; }
            set
            {
                if (value == _settingsContent)
                    return;

                _settingsContent = value;
                OnPropertyChanged();
            }
        }

        private SettingsViewInfo _selectedSettingsView;
        public SettingsViewInfo SelectedSettingsView
        {
            get { return _selectedSettingsView; }
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

        SettingsGeneralView _settingsGerneralView;
        SettingsWindowView _settingsWindowView;
        SettingsAppearanceView _settingsApperanceView;
        SettingsLanguageView _settingsLanguageView;
        SettingsHotKeysView _settingsHotKeysView;
        SettingsAutostartView _settingsAutostartView;
        SettingsSettingsView _settingsSettingsView;
        SettingsUpdateView _settingsUpdateView;
        SettingsImportExportView _settingsImportExportView;
        IPScannerSettingsView _ipScannerSettingsView;
        PortScannerSettingsView _portScannerSettingsView;
        PingSettingsView _pingSettingsViewModel;
        TracerouteSettingsView _tracerouteSettingsView;
        DNSLookupSettingsView _dnsLookupSettingsViewModel;
        RemoteDesktopSettingsView _remoteDesktopSettingsView;
        PuTTYSettingsView _puTTYSettingsView;
        SNMPSettingsView _snmpSettingsView;
        WakeOnLANSettingsView _wakeOnLANSettingsView;
        HTTPHeadersSettingsView _httpHeadersSettingsView;
        #endregion

        #region Contructor, load settings
        public SettingsViewModel(ApplicationViewManager.Name applicationName)
        {
            LoadSettings();

            ChangeSettingsView(applicationName);
        }

        private void LoadSettings()
        {
            // General
            _settingsViewsSource = new CollectionViewSource()
            {
                Source = SettingsViewManager.List
            };

            SettingsViews.GroupDescriptions.Add(new PropertyGroupDescription("TranslatedGroup"));
            SettingsViews.SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));

            SettingsViews.Filter = o =>
            {
                if (string.IsNullOrEmpty(Search))
                    return true;

                // Settings view without "-" and " "
                SettingsViewInfo info = o as SettingsViewInfo;

                Regex regex = new Regex(@" |-");

                string search = regex.Replace(Search, "");

                // Search by TranslatedName and Name
                return (regex.Replace(info.TranslatedName, "").IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1) || (regex.Replace(info.Name.ToString(), "").IndexOf(search, StringComparison.OrdinalIgnoreCase) > -1);
            };
        }
        #endregion

        #region Methods
        public void ChangeSettingsView(ApplicationViewManager.Name applicationName)
        {
            if (Enum.GetNames(typeof(SettingsViewManager.Name)).Contains(applicationName.ToString()) && ApplicationViewManager.Name.None.ToString() != applicationName.ToString())
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
                    _settingsSettingsView.SaveAndCheckSettings();

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
                    _settingsImportExportView.SaveAndCheckSettings();

                    SettingsContent = _settingsImportExportView;
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
                case SettingsViewManager.Name.PuTTY:
                    if (_puTTYSettingsView == null)
                        _puTTYSettingsView = new PuTTYSettingsView();

                    SettingsContent = _puTTYSettingsView;
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
            }
        }
        #endregion
    }
}