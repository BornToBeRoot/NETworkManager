using NETworkManager.Models.Settings;
using MahApps.Metro.Controls;
using NETworkManager.Helpers;
using NETworkManager.Utils;

namespace NETworkManager.ViewModels.Settings
{
    public class SettingsGeneralHotKeysViewModel : ViewModelBase
    {
        #region Variables
        private bool _isLoading = true;

        private bool _hotKeyShowWindowEnabled;
        public bool HotKeyShowWindowEnabled
        {
            get { return _hotKeyShowWindowEnabled; }
            set
            {
                if (value == _hotKeyShowWindowEnabled)
                    return;

                if (!_isLoading)
                {
                    SettingsManager.Current.HotKey_ShowWindowEnabled = value;

                    SettingsManager.HotKeysChanged = true;                    
                }

                _hotKeyShowWindowEnabled = value;
                OnPropertyChanged();
            }
        }

        private HotKey _hotKeyShowWindow;
        public HotKey HotKeyShowWindow
        {
            get { return _hotKeyShowWindow; }
            set
            {
                if (value == _hotKeyShowWindow)
                    return;

                if (!_isLoading && value != null)
                {
                    SettingsManager.Current.HotKey_ShowWindowKey = (int)HotKeys.WpfKeyToFormsKeys(value.Key);
                    SettingsManager.Current.HotKey_ShowWindowModifier = HotKeys.GetModifierKeysSum(value.ModifierKeys);

                    SettingsManager.HotKeysChanged = true;
                }

                _hotKeyShowWindow = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, LoadSettings
        public SettingsGeneralHotKeysViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            HotKeyShowWindowEnabled = SettingsManager.Current.HotKey_ShowWindowEnabled;
            HotKeyShowWindow = new HotKey(HotKeys.FormsKeysToWpfKey(SettingsManager.Current.HotKey_ShowWindowKey), HotKeys.GetModifierKeysFromInt(SettingsManager.Current.HotKey_ShowWindowModifier));
        }
        #endregion        
    }
}