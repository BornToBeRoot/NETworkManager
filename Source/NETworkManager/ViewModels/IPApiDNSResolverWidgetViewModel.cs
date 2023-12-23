using System.ComponentModel;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Input;
using NETworkManager.Models.IPApi;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class IPApiDNSResolverWidgetViewModel : ViewModelBase
{
    #region Events

    private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SettingsInfo.Dashboard_CheckIPApiDNSResolver):
                // Check if enabled via settings
                if (SettingsManager.Current.Dashboard_CheckIPApiDNSResolver)
                    Check();

                break;
        }
    }

    #endregion

    #region Variables

    private bool _isRunning;

    public bool IsRunning
    {
        get => _isRunning;
        set
        {
            if (value == _isRunning)
                return;

            _isRunning = value;
            OnPropertyChanged();
        }
    }

    private DNSResolverResult _result;

    public DNSResolverResult Result
    {
        get => _result;
        private set
        {
            if (value == _result)
                return;

            _result = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Constructor, load settings

    public IPApiDNSResolverWidgetViewModel()
    {
        // Detect if network address or status changed...
        NetworkChange.NetworkAvailabilityChanged += (_, _) => Check();
        NetworkChange.NetworkAddressChanged += (_, _) => Check();

        LoadSettings();

        // Detect if settings have changed...
        SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;
    }

    private void LoadSettings()
    {
    }

    #endregion

    #region ICommands & Actions

    public ICommand CheckViaHotkeyCommand => new RelayCommand(_ => CheckViaHotkeyAction());

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
        // Check is disabled via settings
        if (!SettingsManager.Current.Dashboard_CheckIPApiDNSResolver)
            return;

        // Don't check multiple times if already running
        if (IsRunning)
            return;

        IsRunning = true;
        Result = null;

        // Make the user happy, let him see a reload animation (and he cannot spam the reload command)        
        await Task.Delay(2000);

        Result = await DNSResolverService.GetInstance().GetDNSResolverAsync();

        Debug.WriteLine(Result);

        IsRunning = false;
    }

    #endregion
}