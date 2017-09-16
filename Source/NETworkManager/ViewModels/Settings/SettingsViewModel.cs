using System.Linq;
using NETworkManager.Views;
using System.Windows.Controls;
using NETworkManager.Views.Settings;
using System.ComponentModel;
using System.Windows.Data;
using System;

namespace NETworkManager.ViewModels.Settings
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Variables
        private ApplicationViewManager.Name _selectedApplicationName;
        public ApplicationViewManager.Name SelectedApplicationName
        {
            get { return _selectedApplicationName; }
            set
            {
                ChangeSelectedSettingsView(value.ToString());

                if (value == _selectedApplicationName)
                    return;

                _selectedApplicationName = value;
            }
        }

        private CollectionViewSource _settingsViewsSource;
        public ICollectionView SettingsViews
        {
            get { return _settingsViewsSource.View; }
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

        SettingsGeneralGeneralView _settingsGeneralGerneralView;
        SettingsGeneralWindowView _settingsGeneralWindowView;
        SettingsGeneralAppearanceView _settingsGeneralApperanceView;
        SettingsGeneralLanguageView _settingsGeneralLanguageView;
        SettingsGeneralHotKeysView _settingsGeneralHotKeysView;
        SettingsGeneralAutostartView _settingsGeneralAutostartView;
        SettingsGeneralSettingsView _settingsGeneralSettingsView;
        SettingsGeneralImportExportView _settingsGeneralImportExportView;
        SettingsApplicationIPScannerView _settingsApplicationIPScannerView;
        SettingsApplicationPortScannerView _settingsApplicationPortScannerView;
        SettingsApplicationWakeOnLANView _settingsApplicationWakeOnLANView;
        SettingsApplicationPingView _settingsApplicationPingView;
        SettingsApplicationTracerouteView _settingsApplicationTracerouteView;
        SettingsApplicationDNSLookupView _settingsApplicationDNSLookupView;
        #endregion

        #region Contructor, load settings
        public SettingsViewModel()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            // General
            _settingsViewsSource = new CollectionViewSource()
            {
                Source = SettingsViewManager.List
            };

            _settingsViewsSource.GroupDescriptions.Add(new PropertyGroupDescription("TranslatedGroup"));
        }
        #endregion

        #region Methods
        private void ChangeSelectedSettingsView(string applicationName)
        {
            if (Enum.GetNames(typeof(SettingsViewManager.Name)).Contains(applicationName))
                SelectedSettingsView = SettingsViews.SourceCollection.Cast<SettingsViewInfo>().FirstOrDefault(x => x.Name.ToString() == applicationName);
            else
                SelectedSettingsView = SettingsViews.SourceCollection.Cast<SettingsViewInfo>().FirstOrDefault(x => x.Name == SettingsViewManager.Name.General);
        }

        private void ChangeSettingsContent(SettingsViewInfo settingsViewInfo)
        {
            switch (settingsViewInfo.Name)
            {
                case SettingsViewManager.Name.General:
                    if (_settingsGeneralGerneralView == null)
                        _settingsGeneralGerneralView = new SettingsGeneralGeneralView();

                    SettingsContent = _settingsGeneralGerneralView;
                    break;
                case SettingsViewManager.Name.Window:
                    if (_settingsGeneralWindowView == null)
                        _settingsGeneralWindowView = new SettingsGeneralWindowView();

                    SettingsContent = _settingsGeneralWindowView;
                    break;
                case SettingsViewManager.Name.Appearance:
                    if (_settingsGeneralApperanceView == null)
                        _settingsGeneralApperanceView = new SettingsGeneralAppearanceView();

                    SettingsContent = _settingsGeneralApperanceView;
                    break;
                case SettingsViewManager.Name.Language:
                    if (_settingsGeneralLanguageView == null)
                        _settingsGeneralLanguageView = new SettingsGeneralLanguageView();

                    SettingsContent = _settingsGeneralLanguageView;
                    break;
                case SettingsViewManager.Name.HotKeys:
                    if (_settingsGeneralHotKeysView == null)
                        _settingsGeneralHotKeysView = new SettingsGeneralHotKeysView();

                    SettingsContent = _settingsGeneralHotKeysView;
                    break;
                case SettingsViewManager.Name.Autostart:
                    if (_settingsGeneralAutostartView == null)
                        _settingsGeneralAutostartView = new SettingsGeneralAutostartView();

                    SettingsContent = _settingsGeneralAutostartView;
                    break;
                case SettingsViewManager.Name.Settings:
                    if (_settingsGeneralSettingsView == null)
                        _settingsGeneralSettingsView = new SettingsGeneralSettingsView();

                    SettingsContent = _settingsGeneralSettingsView;
                    break;
                case SettingsViewManager.Name.ImportExport:
                    if (_settingsGeneralImportExportView == null)
                        _settingsGeneralImportExportView = new SettingsGeneralImportExportView();

                    SettingsContent = _settingsGeneralImportExportView;
                    break;
                case SettingsViewManager.Name.IPScanner:
                    if (_settingsApplicationIPScannerView == null)
                        _settingsApplicationIPScannerView = new SettingsApplicationIPScannerView();

                    SettingsContent = _settingsApplicationIPScannerView;
                    break;
                case SettingsViewManager.Name.PortScanner:
                    if (_settingsApplicationPortScannerView == null)
                        _settingsApplicationPortScannerView = new SettingsApplicationPortScannerView();

                    SettingsContent = _settingsApplicationPortScannerView;
                    break;
                case SettingsViewManager.Name.WakeOnLAN:
                    if (_settingsApplicationWakeOnLANView == null)
                        _settingsApplicationWakeOnLANView = new SettingsApplicationWakeOnLANView();

                    SettingsContent = _settingsApplicationWakeOnLANView;
                    break;
                case SettingsViewManager.Name.Ping:
                    if (_settingsApplicationPingView == null)
                        _settingsApplicationPingView = new SettingsApplicationPingView();

                    SettingsContent = _settingsApplicationPingView;
                    break;
                case SettingsViewManager.Name.Traceroute:
                    if (_settingsApplicationTracerouteView == null)
                        _settingsApplicationTracerouteView = new SettingsApplicationTracerouteView();

                    SettingsContent = _settingsApplicationTracerouteView;
                    break;
                case SettingsViewManager.Name.DNSLookup:
                    if (_settingsApplicationDNSLookupView == null)
                        _settingsApplicationDNSLookupView = new SettingsApplicationDNSLookupView();

                    SettingsContent = _settingsApplicationDNSLookupView;
                    break;
            }
        }
        #endregion
    }
}