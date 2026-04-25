using MahApps.Metro.Controls;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class SettingsHotKeysViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    public bool HotKeyShowWindowEnabled
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
            {
                SettingsManager.Current.HotKey_ShowWindowEnabled = value;

                SettingsManager.HotKeysChanged = true;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    public HotKey HotKeyShowWindow
    {
        get;
        set
        {
            if (Equals(value, field))
                return;

            if (!_isLoading && value != null)
            {
                SettingsManager.Current.HotKey_ShowWindowKey = (int)HotKeys.WpfKeyToFormsKeys(value.Key);
                SettingsManager.Current.HotKey_ShowWindowModifier = HotKeys.GetModifierKeysSum(value.ModifierKeys);

                SettingsManager.HotKeysChanged = true;
            }

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, LoadSettings

    public SettingsHotKeysViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        HotKeyShowWindowEnabled = SettingsManager.Current.HotKey_ShowWindowEnabled;
        HotKeyShowWindow = new HotKey(HotKeys.FormsKeysToWpfKey(SettingsManager.Current.HotKey_ShowWindowKey),
            HotKeys.GetModifierKeysFromInt(SettingsManager.Current.HotKey_ShowWindowModifier));
    }

    #endregion
}