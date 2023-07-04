using NETworkManager.Models.Network;
using NETworkManager.Settings;
using NETworkManager.Utilities;
using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class IPDNSApiViewModel : ViewModelBase
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

    private IPDNSApiInfo _ipDNSApiInfo;
    public IPDNSApiInfo IPDNSApiInfo
    {
        get => _ipDNSApiInfo;
        set
        {
            if (value == _ipDNSApiInfo)
                return;

            _ipDNSApiInfo = value;
            OnPropertyChanged();
        }
    }

    public bool CheckIPDNSApiEnabled => SettingsManager.Current.Dashboard_CheckIPDNSApiEnabled;
    #endregion

    #region Constructor, load settings

    public IPDNSApiViewModel()
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
        Debug.WriteLine("Check dns api....");

        CheckAsync().ConfigureAwait(false);
    }

    private async Task CheckAsync()
    {
        IsChecking = true;
        IPDNSApiInfo = null;

        var result = await IPDNSApiService.GetInstance().GetIPDNSDetailsAsync();

        if (!result.HasError)
            IPDNSApiInfo = result.Info;

        // Show error & is disabled

        IsChecking = false;
    }
    #endregion

    #region Events
    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SettingsInfo.Dashboard_CheckIPDNSApiEnabled):
                OnPropertyChanged(nameof(CheckIPDNSApiEnabled));

                // Check if enabled via settings
                if (CheckIPDNSApiEnabled)
                    Check();

                break;
        }
    }
    #endregion
}
