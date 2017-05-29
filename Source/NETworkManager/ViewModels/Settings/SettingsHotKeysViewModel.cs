using NETworkManager.Models.Settings;
using MahApps.Metro.Controls;
using NETworkManager.Helpers;

namespace NETworkManager.ViewModels.Settings
{
    public class SettingsHotKeysViewModel : ViewModelBase
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
                    SettingsManager.Current.HotKey_ShowWindowKey = (int)HotKeysHelper.WpfKeyToFormsKeys(value.Key);
                    SettingsManager.Current.HotKey_ShowWindowModifier = HotKeysHelper.GetModifierKeysSum(value.ModifierKeys);

                    SettingsManager.HotKeysChanged = true;
                }

                _hotKeyShowWindow = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Constructor, LoadSettings
        public SettingsHotKeysViewModel()
        {
            LoadSettings();

            _isLoading = false;
        }

        private void LoadSettings()
        {
            HotKeyShowWindowEnabled = SettingsManager.Current.HotKey_ShowWindowEnabled;
            HotKeyShowWindow = new HotKey(HotKeysHelper.FormsKeysToWpfKey(SettingsManager.Current.HotKey_ShowWindowKey), HotKeysHelper.GetModifierKeysFromInt(SettingsManager.Current.HotKey_ShowWindowModifier));
        }
        #endregion        
    }
}