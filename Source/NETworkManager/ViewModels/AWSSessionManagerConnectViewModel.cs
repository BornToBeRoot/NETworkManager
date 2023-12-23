using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class AWSSessionManagerConnectViewModel : ViewModelBase
{
    private string _instanceID;

    private string _profile;

    private string _region;

    public AWSSessionManagerConnectViewModel(Action<AWSSessionManagerConnectViewModel> connectCommand,
        Action<AWSSessionManagerConnectViewModel> cancelHandler)
    {
        ConnectCommand = new RelayCommand(_ => connectCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        InstanceIDHistoryView =
            CollectionViewSource.GetDefaultView(SettingsManager.Current.AWSSessionManager_InstanceIDHistory);
        ProfileHistoryView =
            CollectionViewSource.GetDefaultView(SettingsManager.Current.AWSSessionManager_ProfileHistory);
        RegionHistoryView =
            CollectionViewSource.GetDefaultView(SettingsManager.Current.AWSSessionManager_RegionHistory);

        LoadSettings();
    }

    public ICommand ConnectCommand { get; }
    public ICommand CancelCommand { get; }

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

    private void LoadSettings()
    {
        Profile = SettingsManager.Current.AWSSessionManager_Profile;
        Region = SettingsManager.Current.AWSSessionManager_Region;
    }
}