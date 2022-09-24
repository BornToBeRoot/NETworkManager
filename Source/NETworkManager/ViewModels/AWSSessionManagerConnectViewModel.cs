using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using NETworkManager.Models.PowerShell;

namespace NETworkManager.ViewModels
{
    public class AWSSessionManagerConnectViewModel : ViewModelBase
    {
        public ICommand ConnectCommand { get; }
        public ICommand CancelCommand { get; }

        private bool _enableRemoteConsole;
        public bool EnableRemoteConsole
        {
            get => _enableRemoteConsole;
            set
            {
                if (value == _enableRemoteConsole)
                    return;

                _enableRemoteConsole = value;
                OnPropertyChanged();
            }
        }

        private string _instanceID;
        public string InstanceID
        {
            get => _instanceID;
            set
            {
                if (value == _instanceID)
                    return;

                _instanceID = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView InstanceIDHistoryView { get; }

        private string _profile;
        public string Profile
        {
            get => _profile;
            set
            {
                if (value == _profile)
                    return;

                _profile = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView ProfileHistoryView { get; }

        private string _region;
        public string Region
        {
            get => _region;
            set
            {
                if (value == _region)
                    return;

                _instanceID = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView RegionHistoryView { get; }

        public AWSSessionManagerConnectViewModel(Action<AWSSessionManagerConnectViewModel> connectCommand, Action<AWSSessionManagerConnectViewModel> cancelHandler)
        {
            ConnectCommand = new RelayCommand(p => connectCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));


            //HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PowerShell_HostHistory);

            LoadSettings();
        }

        private void LoadSettings()
        {
             //   AdditionalCommandLine = SettingsManager.Current.PowerShell_AdditionalCommandLine;
        }
    }
}
