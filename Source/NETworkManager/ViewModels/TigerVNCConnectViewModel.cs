using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using NETworkManager.Settings;
using NETworkManager.Utilities;

namespace NETworkManager.ViewModels;

public class TigerVNCConnectViewModel : ViewModelBase
{
    public TigerVNCConnectViewModel(Action<TigerVNCConnectViewModel> connectCommand,
        Action<TigerVNCConnectViewModel> cancelHandler, string host = null)
    {
        ConnectCommand = new RelayCommand(_ => connectCommand(this));
        CancelCommand = new RelayCommand(_ => cancelHandler(this));

        if (!string.IsNullOrEmpty(host))
            Host = host;

        HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.TigerVNC_HostHistory);
        PortHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.TigerVNC_PortHistory);

        LoadSettings();
    }

    public ICommand ConnectCommand { get; }
    public ICommand CancelCommand { get; }

    public string Host
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView HostHistoryView { get; }

    public int Port
    {
        get;
        set
        {
            if (value == field)
                return;

            field = value;
            OnPropertyChanged();
        }
    }

    public ICollectionView PortHistoryView { get; }

    private void LoadSettings()
    {
        Port = SettingsManager.Current.TigerVNC_Port;
    }
}