using System;
using System.Windows.Input;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class WelcomeViewModel : ViewModelBase
{
    private bool _checkForUpdatesAtStartup = GlobalStaticConfiguration.Update_CheckForUpdatesAtStartup;

    private bool _checkIPApiDNSResolver = GlobalStaticConfiguration.Dashboard_CheckIPApiDNSResolver;

    private bool _checkIPApiIPGeolocation = GlobalStaticConfiguration.Dashboard_CheckIPApiIPGeolocation;

    private bool _checkPublicIPAddress = GlobalStaticConfiguration.Dashboard_CheckPublicIPAddress;

    private bool _powerShellModifyGlobalProfile;

    public WelcomeViewModel(Action<WelcomeViewModel> continueCommand)
    {
        ContinueCommand = new RelayCommand(_ => continueCommand(this));
    }

    public ICommand ContinueCommand { get; }

    public bool CheckForUpdatesAtStartup
    {
        get => _checkForUpdatesAtStartup;
        set
        {
            if (value == _checkForUpdatesAtStartup)
                return;

            _checkForUpdatesAtStartup = value;
            OnPropertyChanged();
        }
    }

    public bool CheckPublicIPAddress
    {
        get => _checkPublicIPAddress;
        set
        {
            if (value == _checkPublicIPAddress)
                return;

            _checkPublicIPAddress = value;
            OnPropertyChanged();
        }
    }

    public bool CheckIPApiIPGeolocation
    {
        get => _checkIPApiIPGeolocation;
        set
        {
            if (value == _checkIPApiIPGeolocation)
                return;

            _checkIPApiIPGeolocation = value;
            OnPropertyChanged();
        }
    }

    public bool CheckIPApiDNSResolver
    {
        get => _checkIPApiDNSResolver;
        set
        {
            if (value == _checkIPApiDNSResolver)
                return;

            _checkIPApiDNSResolver = value;
            OnPropertyChanged();
        }
    }

    public bool PowerShellModifyGlobalProfile
    {
        get => _powerShellModifyGlobalProfile;
        set
        {
            if (value == _powerShellModifyGlobalProfile)
                return;

            _powerShellModifyGlobalProfile = value;
            OnPropertyChanged();
        }
    }
}