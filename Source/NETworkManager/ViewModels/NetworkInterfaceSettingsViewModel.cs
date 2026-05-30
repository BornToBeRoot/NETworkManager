using NETworkManager.Settings;

namespace NETworkManager.ViewModels;

/// <summary>
/// Represents the settings for the Network Interface.
/// </summary>
public class NetworkInterfaceSettingsViewModel : ViewModelBase
{
    #region Variables

    private readonly bool _isLoading;

    /// <summary>
    /// Gets or sets the bandwidth chart time window in seconds.
    /// </summary>
    public int BandwidthChartTime
    {
        get;
        set
        {
            if (value == field)
                return;

            if (!_isLoading)
                SettingsManager.Current.NetworkInterface_BandwidthChartTime = value;

            field = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Contructor, load settings

    /// <summary>
    /// Initializes a new instance of the <see cref="NetworkInterfaceSettingsViewModel"/> class.
    /// </summary>
    public NetworkInterfaceSettingsViewModel()
    {
        _isLoading = true;

        LoadSettings();

        _isLoading = false;
    }

    private void LoadSettings()
    {
        BandwidthChartTime = SettingsManager.Current.NetworkInterface_BandwidthChartTime;
    }

    #endregion
}
