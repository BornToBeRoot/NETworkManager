using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class IPGeoApiViewModel : ViewModelBase
{
    #region  Variables 
    private bool _isChecking;
    public bool IsChecking
    {
        get => _isChecking;
        set
        {
            if (value == _isChecking)
                return;

            _isChecking = value;
            OnPropertyChanged();
        }
    }

    private IPGeoApiInfo _ipGeoApiInfo;
    public IPGeoApiInfo IPGeoApiInfo
    {
        get => _ipGeoApiInfo;
        set
        {
            if (value == _ipGeoApiInfo)
                return;

            _ipGeoApiInfo = value;
            OnPropertyChanged();
        }
    }

    public bool CheckIPGeoApi => SettingsManager.Current.Dashboard_CheckPublicIPAddress;
    #endregion

    #region Constructor, load settings

    public IPGeoApiViewModel()
    {
        // Detect if network address or status changed...
        NetworkChange.NetworkAvailabilityChanged += (sender, args) => Check();
        NetworkChange.NetworkAddressChanged += (sender, args) => Check();

        LoadSettings();

        // Detect if settings have changed...
        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;
    }

    private void LoadSettings()
    {

    }
    #endregion

    #region ICommands & Actions
    public ICommand CheckViaHotkeyCommand => new RelayCommand(p => CheckViaHotkeyAction());

    private void CheckViaHotkeyAction()
    {
        Check();
    }
    #endregion

    #region Methods
    public void Check()
    {
        CheckAsync().ConfigureAwait(false);
    }

    private async Task CheckAsync()
    {
        IsChecking = true;
        IPGeoApiInfo = null;

        var result = await IPGeoApiService.GetInstance().GetIPDetailsAsync();

        if (!result.HasError)
            IPGeoApiInfo = result.Info;

        // Show error & is disabled

        IsChecking = false;
    }
    #endregion

    #region Events
    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SettingsInfo.Dashboard_CheckPublicIPAddress):
                OnPropertyChanged(nameof(CheckIPGeoApi));
                break;
        }
    }
    #endregion
}
