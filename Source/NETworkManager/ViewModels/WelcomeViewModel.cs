using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels;

public class WelcomeViewModel : ViewModelBase
{
    public ICommand ContinueCommand { get; }

    private bool _checkForUpdatesAtStartup = GlobalStaticConfiguration.Update_CheckForUpdatesAtStartup;
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

    private bool _checkPublicIPAddress = GlobalStaticConfiguration.Dashboard_CheckPublicIPAddress;
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

    private bool _checkIPApiIPGeolocation = GlobalStaticConfiguration.Dashboard_CheckIPApiIPGeolocation;
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

    private bool _checkIPApiDNSResolver = GlobalStaticConfiguration.Dashboard_CheckIPApiDNSResolver;
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

    private bool _powerShellModifyGlobalProfile;
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

    public WelcomeViewModel(Action<WelcomeViewModel> continueCommand)
    {
        ContinueCommand = new RelayCommand(_ => continueCommand(this));
    }
}