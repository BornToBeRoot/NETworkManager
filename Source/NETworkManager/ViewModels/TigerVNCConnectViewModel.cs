using NETworkManager.Settings;
using NETworkManager.Utilities;
using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class TigerVNCConnectViewModel : ViewModelBase
    {
        public ICommand ConnectCommand { get; }
        public ICommand CancelCommand { get; }

        private string _host;
        public string Host
        {
            get => _host;
            set
            {
                if (value == _host)
                    return;

                _host = value;
                OnPropertyChanged();
            }
        }
        
        public ICollectionView HostHistoryView { get; }

        private int _port;
        public int Port
        {
            get => _port;
            set
            {
                if (value == _port)
                    return;

                _port = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView PortHistoryView { get; }
        
        public TigerVNCConnectViewModel(Action<TigerVNCConnectViewModel> connectCommand, Action<TigerVNCConnectViewModel> cancelHandler, string host = null)
        {
            ConnectCommand = new RelayCommand(p => connectCommand(this));
            CancelCommand = new RelayCommand(p => cancelHandler(this));

            if (!string.IsNullOrEmpty(host))
                Host = host;
            
            HostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.TigerVNC_HostHistory);
            PortHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.TigerVNC_PortHistory);

            LoadSettings();
        }

        private void LoadSettings()
        {
            Port = SettingsManager.Current.TigerVNC_Port;
        }
    }
}
