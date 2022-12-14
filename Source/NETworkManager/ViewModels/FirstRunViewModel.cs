using NETworkManager.Utilities;
using System;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class FirstRunViewModel : ViewModelBase
    {
        public ICommand ContinueCommand { get; }

        private bool _checkForUpdatesAtStartup = true;
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

        private bool _checkPublicIPAddress = true;
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

        public FirstRunViewModel(Action<FirstRunViewModel> continueCommand)
        {
            ContinueCommand = new RelayCommand(p => continueCommand(this));
        }
    }
}