using NETworkManager.Models.Settings;
using NETworkManager.Utilities;
using System;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace NETworkManager.ViewModels
{
    public class PuTTYSessionConnectViewModel : ViewModelBase
    {
        private readonly ICommand _connectCommand;
        public ICommand ConnectCommand
        {
            get { return _connectCommand; }
        }

        private readonly ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get { return _cancelCommand; }
        }

        private string _host;
        public string Host
        {
            get { return _host; }
            set
            {
                if (value == _host)
                    return;

                _host = value;
                OnPropertyChanged();
            }
        }

        private ICollectionView _hostHistoryView;
        public ICollectionView HostHistoryView
        {
            get { return _hostHistoryView; }
        }

        public PuTTYSessionConnectViewModel(Action<PuTTYSessionConnectViewModel> connectCommand, Action<PuTTYSessionConnectViewModel> cancelHandler)
        {
            _connectCommand = new RelayCommand(p => connectCommand(this));
            _cancelCommand = new RelayCommand(p => cancelHandler(this));

            _hostHistoryView = CollectionViewSource.GetDefaultView(SettingsManager.Current.PuTTY_HostHistory);
        }
    }
}
