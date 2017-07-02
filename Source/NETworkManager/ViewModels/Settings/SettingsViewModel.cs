using System.Linq;
using NETworkManager.Views;
using System.Windows.Controls;
using NETworkManager.Views.Settings;
using System.Collections.Generic;

namespace NETworkManager.ViewModels.Settings
{
    public class SettingsViewModel : ViewModelBase
    {
        #region Variables

        private int _selectedSettingsTabIndex;
        public int SelectedSettingsTabIndex
        {
            get { return _selectedSettingsTabIndex; }
            set
            {
                if (value == _selectedSettingsTabIndex)
                    return;

                _selectedSettingsTabIndex = value;
                OnPropertyChanged();
            }
        }

        #region General
        private List<SettingsGeneralViewInfo> _settingsGeneralViews = new List<SettingsGeneralViewInfo>();
        public List<SettingsGeneralViewInfo> SettingsGeneralViews
        {
            get { return _settingsGeneralViews; }
            set
            {
                if (value == _settingsGeneralViews)
                    return;

                _settingsGeneralViews = value;
                OnPropertyChanged();
            }
        }

        private UserControl _settingsGeneralContent;
        public UserControl SettingsGeneralContent
        {
            get { return _settingsGeneralContent; }
            set
            {
                if (value == _settingsGeneralContent)
                    return;

                _settingsGeneralContent = value;
                OnPropertyChanged();
            }
        }

        private SettingsGeneralViewInfo _selectedSettingsGeneralView;
        public SettingsGeneralViewInfo SelectedSettingsGeneralView
        {
            get { return _selectedSettingsGeneralView; }
            set
            {
                if (value == _selectedSettingsGeneralView)
                    return;

                if (value != null)
                    ChangeSettingsGeneralView(value);

                _selectedSettingsGeneralView = value;
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
        SettingsGeneralDeveloperView _settingsGeneralExperimentalView;
        #endregion

        #region Application
        private List<ApplicationViewInfo> _settingsApplicationViews = new List<ApplicationViewInfo>();
        public List<ApplicationViewInfo> SettingsApplicationViews
        {
            get { return _settingsApplicationViews; }
            set
            {
                if (value == _settingsApplicationViews)
                    return;

                _settingsApplicationViews = value;
                OnPropertyChanged();
            }
        }

        private UserControl _settingsApplicationContent;
        public UserControl SettingsApplicationContent
        {
            get { return _settingsApplicationContent; }
            set
            {
                if (value == _settingsApplicationContent)
                    return;

                _settingsApplicationContent = value;
                OnPropertyChanged();
            }
        }

        private ApplicationViewInfo _selectedSettingsApplicationView;
        public ApplicationViewInfo SelectedSettingsApplicationView
        {
            get { return _selectedSettingsApplicationView; }
            set
            {
                if (value == _selectedSettingsApplicationView)
                    return;

                if (value != null)
                    ChangeSettingsApplicationView(value);

                _selectedSettingsApplicationView = value;
                OnPropertyChanged();
            }
        }

        SettingsApplicationIPScannerView _settingsApplicationIPScannerView;
        SettingsApplicationPortScannerView _settingsApplicationPortScannerView;
        SettingsApplicationPingView _settingsApplicationPingView;
        SettingsApplicationTracerouteView _settingsApplicationTracerouteView;
        #endregion
        #endregion

        #region Contructor, load settings
        public SettingsViewModel(ApplicationViewManager.Name selectedApplicationName)
        {
            LoadSettings(selectedApplicationName);
        }

        private void LoadSettings(ApplicationViewManager.Name selectedApplicationName)
        {
            // General
            SettingsGeneralViews = SettingsGeneralViewManager.List;
            SelectedSettingsGeneralView = SettingsGeneralViews.Where(x => x.Name == SettingsGeneralViewManager.Name.General).FirstOrDefault();

            // Application
            SettingsApplicationViews = SettingsApplicationViewManager.List;
            ApplicationViewInfo applicationViewInfo = SettingsApplicationViews.FirstOrDefault(x => x.Name == selectedApplicationName);

            if (applicationViewInfo == null || applicationViewInfo.Name == ApplicationViewManager.Name.None)
            {
                SelectedSettingsApplicationView = SettingsApplicationViews.First();
            }
            else
            {
                SelectedSettingsApplicationView = applicationViewInfo;
                SelectedSettingsTabIndex = 1;
            }
        }
        #endregion

        #region Methods
        private void ChangeSettingsGeneralView(SettingsGeneralViewInfo settingsViewInfo)
        {
            switch (settingsViewInfo.Name)
            {
                case SettingsGeneralViewManager.Name.General:
                    if (_settingsGeneralGerneralView == null)
                        _settingsGeneralGerneralView = new SettingsGeneralGeneralView();

                    SettingsGeneralContent = _settingsGeneralGerneralView;
                    break;
                case SettingsGeneralViewManager.Name.Window:
                    if (_settingsGeneralWindowView == null)
                        _settingsGeneralWindowView = new SettingsGeneralWindowView();

                    SettingsGeneralContent = _settingsGeneralWindowView;
                    break;
                case SettingsGeneralViewManager.Name.Appearance:
                    if (_settingsGeneralApperanceView == null)
                        _settingsGeneralApperanceView = new SettingsGeneralAppearanceView();

                    SettingsGeneralContent = _settingsGeneralApperanceView;
                    break;
                case SettingsGeneralViewManager.Name.Language:
                    if (_settingsGeneralLanguageView == null)
                        _settingsGeneralLanguageView = new SettingsGeneralLanguageView();

                    SettingsGeneralContent = _settingsGeneralLanguageView;
                    break;
                case SettingsGeneralViewManager.Name.HotKeys:
                    if (_settingsGeneralHotKeysView == null)
                        _settingsGeneralHotKeysView = new SettingsGeneralHotKeysView();

                    SettingsGeneralContent = _settingsGeneralHotKeysView;
                    break;
                case SettingsGeneralViewManager.Name.Autostart:
                    if (_settingsGeneralAutostartView == null)
                        _settingsGeneralAutostartView = new SettingsGeneralAutostartView();

                    SettingsGeneralContent = _settingsGeneralAutostartView;
                    break;
                case SettingsGeneralViewManager.Name.Settings:
                    if (_settingsGeneralSettingsView == null)
                        _settingsGeneralSettingsView = new SettingsGeneralSettingsView();

                    SettingsGeneralContent = _settingsGeneralSettingsView;
                    break;
                case SettingsGeneralViewManager.Name.ImportExport:
                    if (_settingsGeneralImportExportView == null)
                        _settingsGeneralImportExportView = new SettingsGeneralImportExportView();

                    SettingsGeneralContent = _settingsGeneralImportExportView;
                    break;
                case SettingsGeneralViewManager.Name.Developer:
                    if (_settingsGeneralExperimentalView == null)
                        _settingsGeneralExperimentalView = new SettingsGeneralDeveloperView();

                    SettingsGeneralContent = _settingsGeneralExperimentalView;
                    break;
            }
        }

        private void ChangeSettingsApplicationView(ApplicationViewInfo applicationViewInfo)
        {
            switch (applicationViewInfo.Name)
            {
                case ApplicationViewManager.Name.IPScanner:
                    if (_settingsApplicationIPScannerView == null)
                        _settingsApplicationIPScannerView = new SettingsApplicationIPScannerView();

                    SettingsApplicationContent = _settingsApplicationIPScannerView;
                    break;
                case ApplicationViewManager.Name.PortScanner:
                    if (_settingsApplicationPortScannerView == null)
                        _settingsApplicationPortScannerView = new SettingsApplicationPortScannerView();

                    SettingsApplicationContent = _settingsApplicationPortScannerView;
                    break;
                case ApplicationViewManager.Name.Ping:
                    if (_settingsApplicationPingView == null)
                        _settingsApplicationPingView = new SettingsApplicationPingView();

                    SettingsApplicationContent = _settingsApplicationPingView;
                    break;
                case ApplicationViewManager.Name.Traceroute:
                    if (_settingsApplicationTracerouteView == null)
                        _settingsApplicationTracerouteView = new SettingsApplicationTracerouteView();

                    SettingsApplicationContent = _settingsApplicationTracerouteView;
                    break;
            }
        }
        #endregion
    }
}