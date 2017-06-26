using System.Linq;
using NETworkManager.Views;
using System.Windows.Controls;
using NETworkManager.Views.Settings;
using System.Collections.Generic;

namespace NETworkManager.ViewModels.Settings
{
    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel()
        {
            // Select the default view
            SettingsViews = SettingsViewManager.List;
            SelectedSettingsView = SettingsViews.Where(p => p.Name == SettingsViewManager.Name.Window).FirstOrDefault();
        }

        private List<SettingsViewInfo> _settingsViews = new List<SettingsViewInfo>();
        public List<SettingsViewInfo> SettingsViews
        {
            get { return _settingsViews; }
            set
            {
                if (value == _settingsViews)
                    return;

                _settingsViews = value;
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
                    ChangeView(value);

                _selectedSettingsView = value;
                OnPropertyChanged();
            }
        }

        SettingsGeneralView settingsGerneralView;
        SettingsWindowView settingsWindowView;
        SettingsAppearanceView settingsApperanceView;
        SettingsLanguageView settingsLanguageView;
        SettingsHotKeysView settingsHotKeysView;
        SettingsAutostartView settingsAutostartView;
        SettingsSettingsView settingsSettingsView;
        SettingsImportExportView settingsImportExportView;
        SettingsDeveloperView settingsExperimentalView;

        private void ChangeView(SettingsViewInfo settingsViewInfo)
        {
            switch (settingsViewInfo.Name)
            {
                case SettingsViewManager.Name.General:
                    if (settingsGerneralView == null)
                        settingsGerneralView = new SettingsGeneralView();

                    SettingsContent = settingsGerneralView;
                    break;
                case SettingsViewManager.Name.Window:
                    if (settingsWindowView == null)
                        settingsWindowView = new SettingsWindowView();

                    SettingsContent = settingsWindowView;
                    break;
                case SettingsViewManager.Name.Appearance:
                    if (settingsApperanceView == null)
                        settingsApperanceView = new SettingsAppearanceView();

                    SettingsContent = settingsApperanceView;
                    break;
                case SettingsViewManager.Name.Language:
                    if (settingsLanguageView == null)
                        settingsLanguageView = new SettingsLanguageView();

                    SettingsContent = settingsLanguageView;
                    break;
                case SettingsViewManager.Name.HotKeys:
                    if (settingsHotKeysView == null)
                        settingsHotKeysView = new SettingsHotKeysView();

                    SettingsContent = settingsHotKeysView;
                    break;
                case SettingsViewManager.Name.Autostart:
                    if (settingsAutostartView == null)
                        settingsAutostartView = new SettingsAutostartView();

                    SettingsContent = settingsAutostartView;
                    break;
                case SettingsViewManager.Name.Settings:
                    if (settingsSettingsView == null)
                        settingsSettingsView = new SettingsSettingsView();

                    SettingsContent = settingsSettingsView;
                    break;
                case SettingsViewManager.Name.ImportExport:
                    if (settingsImportExportView == null)
                        settingsImportExportView = new SettingsImportExportView();

                    SettingsContent = settingsImportExportView;
                    break;
                case SettingsViewManager.Name.Developer:
                    if (settingsExperimentalView == null)
                        settingsExperimentalView = new SettingsDeveloperView();

                    SettingsContent = settingsExperimentalView;
                    break;
            }
        }
    }
}