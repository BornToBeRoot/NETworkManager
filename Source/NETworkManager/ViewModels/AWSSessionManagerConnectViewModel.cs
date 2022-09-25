using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class AWSSessionManagerConnectViewModel : ViewModelBase
    {
        public ICommand ConnectCommand { get; }
        public ICommand CancelCommand { get; }

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

                _region = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView RegionHistoryView { get; }

        public AWSSessionManagerConnectViewModel(Action<AWSSessionManagerConnectViewModel> connectCommand, Action<AWSSessionManagerConnectViewModel> cancelHandler)
        {
            ConnectCommand = new RelayCommand(p => connectCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            InstanceIDHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.AWSSessionManager_InstanceIDHistory);
            ProfileHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.AWSSessionManager_ProfileHistory);
            RegionHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.AWSSessionManager_RegionHistory);

            LoadSettings();
        }

        private void LoadSettings()
        {
            Profile = SettingsManager.Current.AWSSessionManager_DefaultProfile;
            Region = SettingsManager.Current.AWSSessionManager_DefaultRegion;                
        }
    }
}
